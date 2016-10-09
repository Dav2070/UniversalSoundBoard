﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UniversalSoundBoard.Model;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UniversalSoundBoard.Model.Sound;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles; // NotificationsExtensions.Win10
using NotificationsExtensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalSoundBoard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<string> Suggestions;
        ObservableCollection<Category> Categories;
        ObservableCollection<Setting> SettingsListing;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            SystemNavigationManager.GetForCurrentView().BackRequested += onBackRequested;
            Suggestions = new List<string>();
            CreateCategoriesObservableCollection();

            SettingsListing = new ObservableCollection<Setting>();

            //SettingsListing.Add(new Setting { Icon = "\uE2AF", Text = "Log in" });
            SettingsListing.Add(new Setting { Icon = "\uE713", Text = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("Settings-Title"), Id = "Settings" });
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            setDataContext();
            (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            await SoundManager.GetAllSounds();

            FileManager.UpdateLiveTile();
        }

        private void setDataContext()
        {
            ContentRoot.DataContext = (App.Current as App)._itemViewHolder;
        }

        private async void CreateCategoriesObservableCollection()
        {
            Categories = new ObservableCollection<Category>();
            Categories.Add(new Category { Name = "Home", Icon = "\uE10F" });
            await FileManager.GetCategoriesListAsync();
            foreach(Category cat in (App.Current as App)._itemViewHolder.categories)
            {
                Categories.Add(cat);
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SideBar.IsPaneOpen = !SideBar.IsPaneOpen;
        }

        private async void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string text = sender.Text;
            if (String.IsNullOrEmpty(text)) goBack();

            (App.Current as App)._itemViewHolder.title = text;
            (App.Current as App)._itemViewHolder.searchQuery = text;
            await SoundManager.GetSoundsByName(text);
            Suggestions = (App.Current as App)._itemViewHolder.sounds.Where(p => p.Name.StartsWith(text)).Select(p => p.Name).ToList();
            SearchAutoSuggestBox.ItemsSource = Suggestions;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
        }

        private async void SearchAutoSuggestBox_TextChanged_Mobile(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string text = sender.Text;
            if (String.IsNullOrEmpty(text)) goBack();

            (App.Current as App)._itemViewHolder.title = text;
            (App.Current as App)._itemViewHolder.searchQuery = text;
            
            Suggestions = (App.Current as App)._itemViewHolder.sounds.Where(p => p.Name.StartsWith(text)).Select(p => p.Name).ToList();
            SearchAutoSuggestBox.ItemsSource = Suggestions;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
            await SoundManager.GetSoundsByName(text);
        }

        private async void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string text = sender.Text;
            (App.Current as App)._itemViewHolder.title = text;
            (App.Current as App)._itemViewHolder.searchQuery = text;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
            await SoundManager.GetSoundsByName(text);
        }

        private async void SearchAutoSuggestBox_QuerySubmitted_Mobile(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string text = sender.Text;
            (App.Current as App)._itemViewHolder.title = text;
            (App.Current as App)._itemViewHolder.searchQuery = text;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
            await SoundManager.GetSoundsByName(text);
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void goBack()
        {
            (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("AllSounds");
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
            MenuItemsListView.SelectedItem = Categories.First();
            SearchAutoSuggestBox.Text = "";
            await SoundManager.GetAllSounds();
        }

        private async void goBack_Mobile()
        {
            (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("AllSounds");
            MenuItemsListView.SelectedItem = Categories.First();

            SearchAutoSuggestBox.Text = "";
            SearchAutoSuggestBox.Visibility = Visibility.Collapsed;

            AddButton.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Visible;
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
            await SoundManager.GetAllSounds();
        }

        private void onBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;

            if(SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Collapsed)
            {
                App.Current.Exit();
            }

            if((App.Current as App)._itemViewHolder.page == typeof(SettingsPage))
            {
                (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            }

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                goBack_Mobile();
            }
            else
            {
                goBack();
            }
        }

        public void chooseGoBack()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                goBack_Mobile();
            }
            else
            {
                goBack();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            AddButton.Visibility = Visibility.Collapsed;
            SearchButton.Visibility = Visibility.Collapsed;
            SearchAutoSuggestBox.Visibility = Visibility.Visible;

            // slightly delay setting focus
            Task.Factory.StartNew(
                () => Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => SearchAutoSuggestBox.Focus(FocusState.Programmatic)));
        }

        private async void NewSoundFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            // Open file explorer
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");

            var files = await picker.PickMultipleFilesAsync();
            (App.Current as App)._itemViewHolder.progressRingIsActive = true;

            if (files.Count > 0)
            {
                // Application now has read/write access to the picked file(s)
                foreach (StorageFile sound in files)
                {
                    await SoundManager.addSound(sound);
                }
                chooseGoBack();
            }
            (App.Current as App)._itemViewHolder.progressRingIsActive = false;
        }

        private async void MenuItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var category = (Category)e.ClickedItem;
            SideBar.IsPaneOpen = false;

            if((App.Current as App)._itemViewHolder.page == typeof(SettingsPage))
            {
                (App.Current as App)._itemViewHolder.page = typeof(SoundPage);
            }

            // Display all Sounds with the selected category
            if (category == Categories.First())
            {
                chooseGoBack();
            }
            else
            {
                (App.Current as App)._itemViewHolder.title = WebUtility.HtmlDecode(category.Name);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Visible;
                await SoundManager.GetSoundsByCategory(category);
            }
        }


        // Content Dialog Methods

        private async void NewCategoryFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var newCategoryContentDialog = ContentDialogs.CreateNewCategoryContentDialog();
            newCategoryContentDialog.PrimaryButtonClick += NewCategoryContentDialog_PrimaryButtonClick;
            await newCategoryContentDialog.ShowAsync();
        }

        private async void NewCategoryContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get categories List and save with new value
            List<Category> categoriesList = await FileManager.GetCategoriesListAsync();

            // Get combobox value
            ComboBoxItem typeItem = (ComboBoxItem)ContentDialogs.IconSelectionComboBox.SelectedItem;
            string icon = typeItem.Content.ToString();

            Category category = new Category
            {
                Name = ContentDialogs.NewCategoryTextBox.Text,
                Icon = icon
            };

            categoriesList.Add(category);
            await FileManager.SaveCategoriesListAsync(categoriesList);

            // Reload page
            this.Frame.Navigate(this.GetType());
        }

        private async void CategoryDeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var deleteCategoryContentDialog = ContentDialogs.CreateDeleteCategoryContentDialogAsync();
            deleteCategoryContentDialog.PrimaryButtonClick += DeleteCategoryContentDialog_PrimaryButtonClick;
            await deleteCategoryContentDialog.ShowAsync();
        }

        private async void DeleteCategoryContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await FileManager.deleteCategory((App.Current as App)._itemViewHolder.title);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("AllSounds");
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;

            // Reload page
            this.Frame.Navigate(this.GetType());
        }

        private async void CategoryEditButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var editCategoryContentDialog = await ContentDialogs.CreateEditCategoryContentDialogAsync();
            editCategoryContentDialog.PrimaryButtonClick += EditCategoryContentDialog_PrimaryButtonClick;
            await editCategoryContentDialog.ShowAsync();
        }

        private async void EditCategoryContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get categories List and save with new value
            List<Category> categoriesList = await FileManager.GetCategoriesListAsync();

            // Get combobox value
            ComboBoxItem typeItem = (ComboBoxItem)ContentDialogs.IconSelectionComboBox.SelectedItem;
            string icon = typeItem.Content.ToString();

            string newName = ContentDialogs.EditCategoryTextBox.Text;
            string oldName = (App.Current as App)._itemViewHolder.title;

            categoriesList.Find(p => p.Name == oldName).Icon = icon;
            categoriesList.Find(p => p.Name == oldName).Name = newName;

            await FileManager.SaveCategoriesListAsync(categoriesList);
            await FileManager.renameCategory(oldName, newName);

            // Reload page
            this.Frame.Navigate(this.GetType());
            (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("AllSounds");
            (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
        }

        private void SettingsMenuListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var setting = (Setting)e.ClickedItem;

            if (setting.Id == "Settings")
            {
                // TODO Add Settings page
                (App.Current as App)._itemViewHolder.page = typeof(SettingsPage);
                (App.Current as App)._itemViewHolder.title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("Settings-Title");
                (App.Current as App)._itemViewHolder.editButtonVisibility = Visibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
           /* else if (setting.Text == "Log in")
            {
                // TODO Add login page
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                (App.Current as App)._itemViewHolder.page = typeof(LoginPage);
            }
            */
        }
    }
}
