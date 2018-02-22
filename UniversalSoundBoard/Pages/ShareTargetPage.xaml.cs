﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UniversalSoundBoard.Common;
using UniversalSoundBoard.DataAccess;
using UniversalSoundBoard.Models;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UniversalSoundBoard.Pages
{
    public sealed partial class ShareTargetPage : Page
    {
        ObservableCollection<Category> categories;
        ShareOperation shareOperation;
        List<StorageFile> items;

        public ShareTargetPage()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            shareOperation = e.Parameter as ShareOperation;

            items = new List<StorageFile>();

            foreach (StorageFile file in await shareOperation.Data.GetStorageItemsAsync())
            {
                items.Add(file);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            categories = new ObservableCollection<Category>();
            //categories.Add(new Category((new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("AllSounds"), "\uE10F"));
            FileManager.CreateCategoriesObservableCollection();

            // Get all Categories and show them
            foreach(Category cat in (App.Current as App)._itemViewHolder.categories)
            {
                categories.Add(cat);
            }
            Bindings.Update();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddButton.IsEnabled = false;
            AddCategoryButton.IsEnabled = false;
            ProgressRing.IsActive = true;

            Category category = null;
            if(CategoriesListView.SelectedIndex != 0)
            {
                category = CategoriesListView.SelectedItem as Category;
            }

            if (items.Count > 0)
            {
                foreach (StorageFile storagefile in items)
                {
                    if (storagefile.ContentType == "audio/wav" || storagefile.ContentType == "audio/mpeg")
                    {
                        Sound sound = new Sound(storagefile.DisplayName, category, storagefile as StorageFile);
                        await FileManager.addSound(sound);
                    }
                }
            }
            shareOperation.ReportCompleted();
        }

        private void CategoriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ProgressRing.IsActive)
            {
                AddButton.IsEnabled = true;
            }
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Show new Category ContentDialog
            var newCategoryContentDialog = ContentDialogs.CreateNewCategoryContentDialog();
            newCategoryContentDialog.PrimaryButtonClick += NewCategoryContentDialog_PrimaryButtonClick;
            await newCategoryContentDialog.ShowAsync();
        }

        private async void NewCategoryContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get combobox value
            ComboBoxItem typeItem = (ComboBoxItem)ContentDialogs.IconSelectionComboBox.SelectedItem;
            string icon = typeItem.Content.ToString();

            Category category = new Category
            {
                Name = ContentDialogs.NewCategoryTextBox.Text,
                Icon = icon
            };

            categories.Add(category);
            Bindings.Update();

            ObservableCollection<Category> newCategories = new ObservableCollection<Category>();

            foreach (Category cat in categories)
                newCategories.Add(cat);

            newCategories.RemoveAt(0);
            await FileManager.SaveCategoriesListAsync(newCategories);
        }
    }
}
