﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UniversalSoundBoard.Common;
using UniversalSoundBoard.DataAccess;
using UniversalSoundBoard.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UniversalSoundboard.Components
{
    public class SoundItemManager
    {

    }

    public class SoundItem
    {
        private Sound sound;
        private DataTemplate setCategoryItemTemplate;

        public event EventHandler<bool> FavouriteChanged;

        private bool downloadFileWasCanceled = false;
        private bool downloadFileThrewError = false;
        private bool downloadFileIsExecuting = false;
        private List<StorageFile> soundFiles = new List<StorageFile>();

        public SoundItem(Sound sound, DataTemplate setCategoryItemTemplate)
        {
            this.sound = sound;
            this.setCategoryItemTemplate = setCategoryItemTemplate;
        }

        public void ShowFlyout(object sender, Point position)
        {
            if (sound == null) return;
            var flyout = new SoundItemOptionsFlyout(sound.Uuid, sound.Favourite);

            flyout.SetCategoriesFlyoutItemClick += OptionsFlyout_SetCategoriesFlyoutItemClick;
            flyout.SetFavouriteFlyoutItemClick += OptionsFlyout_SetFavouriteFlyoutItemClick;
            flyout.ShareFlyoutItemClick += OptionsFlyout_ShareFlyoutItemClick;
            flyout.ExportFlyoutItemClick += OptionsFlyout_ExportFlyoutItemClick;
            flyout.PinFlyoutItemClick += OptionsFlyout_PinFlyoutItemClick;
            flyout.SetImageFlyoutItemClick += OptionsFlyout_SetImageFlyoutItemClick;
            flyout.RenameFlyoutItemClick += OptionsFlyout_RenameFlyoutItemClick;
            flyout.DeleteFlyoutItemClick += OptionsFlyout_DeleteFlyoutItemClick;

            flyout.ShowAt(sender as UIElement, position);
        }

        private async void OptionsFlyout_SetCategoriesFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            List<Sound> soundsList = new List<Sound> { sound };
            var SetCategoryContentDialog = ContentDialogs.CreateSetCategoryContentDialog(soundsList, setCategoryItemTemplate);
            SetCategoryContentDialog.PrimaryButtonClick += SetCategoryContentDialog_PrimaryButtonClick;
            await SetCategoryContentDialog.ShowAsync();
        }

        private async void SetCategoryContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get the selected categories from the SelectedCategories Dictionary in ContentDialogs
            List<Guid> categoryUuids = new List<Guid>();
            foreach (var entry in ContentDialogs.SelectedCategories)
                if (entry.Value) categoryUuids.Add(entry.Key);

            await FileManager.SetCategoriesOfSoundAsync(sound.Uuid, categoryUuids);
            await FileManager.UpdateGridViewAsync();
        }

        private async void OptionsFlyout_SetFavouriteFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            bool newFav = !sound.Favourite;

            // Update all lists containing sounds with the new favourite value
            List<ObservableCollection<Sound>> soundLists = new List<ObservableCollection<Sound>>
            {
                FileManager.itemViewHolder.Sounds,
                FileManager.itemViewHolder.AllSounds,
                FileManager.itemViewHolder.FavouriteSounds
            };

            foreach (ObservableCollection<Sound> soundList in soundLists)
            {
                var sounds = soundList.Where(s => s.Uuid == sound.Uuid);
                if (sounds.Count() > 0)
                    sounds.First().Favourite = newFav;
            }

            if (newFav)
            {
                // Add to favourites
                FileManager.itemViewHolder.FavouriteSounds.Add(sound);
            }
            else
            {
                // Remove sound from favourites
                FileManager.itemViewHolder.FavouriteSounds.Remove(sound);
            }

            await FileManager.SetSoundAsFavouriteAsync(sound.Uuid, newFav);
            FavouriteChanged?.Invoke(this, newFav);
        }

        private async void OptionsFlyout_ShareFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            if (!await DownloadFile()) return;

            // Copy the file into the temp folder
            soundFiles.Clear();
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            var audioFile = await sound.GetAudioFileAsync();
            if (audioFile == null) return;
            string ext = await sound.GetAudioFileExtensionAsync();

            if (string.IsNullOrEmpty(ext))
                ext = "mp3";

            StorageFile tempFile = await audioFile.CopyAsync(tempFolder, sound.Name + "." + ext, NameCollisionOption.ReplaceExisting);
            soundFiles.Add(tempFile);

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (soundFiles.Count == 0) return;
            var loader = new ResourceLoader();

            DataRequest request = args.Request;
            request.Data.SetStorageItems(soundFiles);
            request.Data.Properties.Title = loader.GetString("ShareDialog-Title");
            request.Data.Properties.Description = soundFiles.First().Name;
        }

        private async void OptionsFlyout_ExportFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            if (!await DownloadFile()) return;

            // Open a folder picker and save the file there
            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };

            string ext = await sound.GetAudioFileExtensionAsync();

            if (string.IsNullOrEmpty(ext))
                ext = "mp3";

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Audio", new List<string>() { "." + ext });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = sound.Name;

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                var audioFile = await sound.GetAudioFileAsync();
                await FileIO.WriteBytesAsync(file, await FileManager.GetBytesAsync(audioFile));
                await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        private async void OptionsFlyout_PinFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            bool isPinned = SecondaryTile.Exists(sound.Uuid.ToString());

            if (isPinned)
            {
                SecondaryTile tile = new SecondaryTile(sound.Uuid.ToString());
                await tile.RequestDeleteAsync();
            }
            else
            {
                // Initialize the tile
                SecondaryTile tile = new SecondaryTile(
                    sound.Uuid.ToString(),
                    sound.Name,
                    sound.Uuid.ToString(),
                    new Uri("ms-appx:///Assets/Icons/Square150x150Logo.png"),
                    TileSize.Default
                );

                // Set the logos for all tile sizes
                tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Icons/Wide310x150Logo.png");
                tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Icons/Square310x310Logo.png");
                tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Icons/Square71x71Logo.png");
                tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Icons/Square44x44Logo.png");

                // Show the display name on all sizes
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.ShowNameOnWide310x150Logo = true;
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                // Pin the tile
                if (!await tile.RequestCreateAsync()) return;

                // Update the tile with the image of the sound
                var imageFile = await sound.GetImageFileAsync();
                if (imageFile == null)
                    imageFile = await StorageFile.GetFileFromApplicationUriAsync(Sound.GetDefaultImageUri());

                NotificationsExtensions.Tiles.TileBinding binding = new NotificationsExtensions.Tiles.TileBinding()
                {
                    Branding = NotificationsExtensions.Tiles.TileBranding.NameAndLogo,

                    Content = new NotificationsExtensions.Tiles.TileBindingContentAdaptive()
                    {
                        BackgroundImage = new NotificationsExtensions.Tiles.TileBackgroundImage()
                        {
                            Source = imageFile.Path,
                            AlternateText = sound.Name
                        }
                    }
                };

                NotificationsExtensions.Tiles.TileContent content = new NotificationsExtensions.Tiles.TileContent()
                {
                    Visual = new NotificationsExtensions.Tiles.TileVisual()
                    {
                        TileSmall = binding,
                        TileMedium = binding,
                        TileWide = binding,
                        TileLarge = binding
                    }
                };

                var notification = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(tile.TileId).Update(notification);
            }
        }

        private async void OptionsFlyout_SetImageFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                FileManager.itemViewHolder.ProgressRingIsActive = true;
                await FileManager.UpdateImageOfSoundAsync(sound.Uuid, file);
                FileManager.itemViewHolder.ProgressRingIsActive = false;
                await FileManager.UpdateGridViewAsync();
            }
        }

        private async void OptionsFlyout_RenameFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            var RenameSoundContentDialog = ContentDialogs.CreateRenameSoundContentDialog(sound);
            RenameSoundContentDialog.PrimaryButtonClick += RenameSoundContentDialog_PrimaryButtonClick;
            await RenameSoundContentDialog.ShowAsync();
        }

        private async void RenameSoundContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Save new name
            if (ContentDialogs.RenameSoundTextBox.Text != sound.Name)
            {
                await FileManager.RenameSoundAsync(sound.Uuid, ContentDialogs.RenameSoundTextBox.Text);
                await FileManager.UpdateGridViewAsync();
            }
        }

        private async void OptionsFlyout_DeleteFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            var DeleteSoundContentDialog = ContentDialogs.CreateDeleteSoundContentDialog(sound.Name);
            DeleteSoundContentDialog.PrimaryButtonClick += DeleteSoundContentDialog_PrimaryButtonClick;
            await DeleteSoundContentDialog.ShowAsync();
        }

        private async void DeleteSoundContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await FileManager.DeleteSoundAsync(sound.Uuid);

            // UpdateGridView nicht in deleteSound, weil es auch in einer Schleife aufgerufen wird (löschen mehrerer Sounds)
            await FileManager.UpdateGridViewAsync();
        }

        private async Task<bool> DownloadFile()
        {
            var downloadStatus = await sound.GetAudioFileDownloadStatusAsync();
            if (downloadStatus == DownloadStatus.NoFileOrNotLoggedIn) return false;

            if (downloadStatus != DownloadStatus.Downloaded)
            {
                // Download the file and show the download dialog
                downloadFileIsExecuting = true;
                Progress<int> progress = new Progress<int>(FileDownloadProgress);
                await sound.DownloadFileAsync(progress);

                ContentDialogs.CreateDownloadFileContentDialog(sound.Name + "." + sound.GetAudioFileExtensionAsync());
                ContentDialogs.downloadFileProgressBar.IsIndeterminate = true;
                ContentDialogs.DownloadFileContentDialog.SecondaryButtonClick += DownloadFileContentDialog_SecondaryButtonClick;
                await ContentDialogs.DownloadFileContentDialog.ShowAsync();
            }

            if (downloadFileWasCanceled)
            {
                downloadFileWasCanceled = false;
                return false;
            }

            if (downloadFileThrewError)
            {
                var errorContentDialog = ContentDialogs.CreateDownloadFileErrorContentDialog();
                await errorContentDialog.ShowAsync();
                downloadFileThrewError = false;
                return false;
            }
            return true;
        }

        private void DownloadFileContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            downloadFileWasCanceled = true;
            downloadFileIsExecuting = false;
        }

        private void FileDownloadProgress(int value)
        {
            if (!downloadFileIsExecuting) return;

            if (value < 0)
            {
                // There was an error
                downloadFileThrewError = true;
                downloadFileIsExecuting = false;
                ContentDialogs.DownloadFileContentDialog.Hide();
            }
            else if (value > 100)
            {
                // Hide the download dialog
                ContentDialogs.DownloadFileContentDialog.Hide();
            }
        }
    }

    public class SoundItemOptionsFlyout
    {
        private readonly ResourceLoader loader;

        private MenuFlyout optionsFlyout;
        private MenuFlyoutItem setFavouriteFlyoutItem;
        private MenuFlyoutItem pinFlyoutItem;

        public event EventHandler<object> FlyoutOpened;
        public event RoutedEventHandler SetCategoriesFlyoutItemClick;
        public event RoutedEventHandler SetFavouriteFlyoutItemClick;
        public event RoutedEventHandler ShareFlyoutItemClick;
        public event RoutedEventHandler ExportFlyoutItemClick;
        public event RoutedEventHandler PinFlyoutItemClick;
        public event RoutedEventHandler SetImageFlyoutItemClick;
        public event RoutedEventHandler RenameFlyoutItemClick;
        public event RoutedEventHandler DeleteFlyoutItemClick;

        public SoundItemOptionsFlyout(Guid soundUuid, bool favourite) {
            loader = new ResourceLoader();

            // Create the flyout
            optionsFlyout = new MenuFlyout();
            optionsFlyout.Opened += (object sender, object e) => FlyoutOpened?.Invoke(sender, e);

            // Set categories
            MenuFlyoutItem setCategoriesFlyoutItem = new MenuFlyoutItem { Text = loader.GetString("SoundItemOptionsFlyout-SetCategories") };
            setCategoriesFlyoutItem.Click += (object sender, RoutedEventArgs e) => SetCategoriesFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(setCategoriesFlyoutItem);

            // Set favourite
            setFavouriteFlyoutItem = new MenuFlyoutItem { Text = loader.GetString(favourite ? "SoundItemOptionsFlyout-UnsetFavourite" : "SoundItemOptionsFlyout-SetFavourite") };
            setFavouriteFlyoutItem.Click += (object sender, RoutedEventArgs e) => SetFavouriteFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(setFavouriteFlyoutItem);

            // Share
            MenuFlyoutItem shareFlyoutItem = new MenuFlyoutItem { Text = loader.GetString("Share") };
            shareFlyoutItem.Click += (object sender, RoutedEventArgs e) => ShareFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(shareFlyoutItem);

            // Export
            MenuFlyoutItem exportFlyoutItem = new MenuFlyoutItem { Text = loader.GetString("Export") };
            exportFlyoutItem.Click += (object sender, RoutedEventArgs e) => ExportFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(exportFlyoutItem);

            // Pin
            pinFlyoutItem = new MenuFlyoutItem { Text = loader.GetString(SecondaryTile.Exists(soundUuid.ToString()) ? "Unpin" : "Pin") };
            pinFlyoutItem.Click += (object sender, RoutedEventArgs e) => PinFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(pinFlyoutItem);

            // Separator
            optionsFlyout.Items.Add(new MenuFlyoutSeparator());

            // Set image
            MenuFlyoutItem setImageFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundItemOptionsFlyout-SetImage") };
            setImageFlyout.Click += (object sender, RoutedEventArgs e) => SetImageFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(setImageFlyout);

            // Rename
            MenuFlyoutItem renameFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundItemOptionsFlyout-Rename") };
            renameFlyout.Click += (object sender, RoutedEventArgs e) => RenameFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(renameFlyout);

            // Delete
            MenuFlyoutItem deleteFlyout = new MenuFlyoutItem { Text = loader.GetString("SoundItemOptionsFlyout-Delete") };
            deleteFlyout.Click += (object sender, RoutedEventArgs e) => DeleteFlyoutItemClick?.Invoke(sender, e);
            optionsFlyout.Items.Add(deleteFlyout);
        }

        public void ShowAt(UIElement sender, Point position)
        {
            optionsFlyout.ShowAt(sender, position);
        }
    }
}