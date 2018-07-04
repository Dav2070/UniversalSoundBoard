﻿using System.Linq;
using UniversalSoundBoard.Models;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using UniversalSoundBoard.DataAccess;
using UniversalSoundboard.Pages;

namespace UniversalSoundBoard.Pages
{
    public sealed partial class MainPage : Page
    {
        int sideBarCollapsedMaxWidth = FileManager.sideBarCollapsedMaxWidth;
        public static CoreDispatcher dispatcher;


        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            CustomiseTitleBar();
        }
        
        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetDataContext();
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            InitializeLocalSettings();
            (App.Current as App)._itemViewHolder.Page = typeof(SoundPage);
            SideBar.MenuItemsSource = (App.Current as App)._itemViewHolder.Categories;

            InitializeAccountSettings();

            FileManager.CreatePlayingSoundsList();
            await FileManager.ShowAllSounds();
        }

        private void SetDataContext()
        {
            SideBar.DataContext = (App.Current as App)._itemViewHolder;
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            FileManager.GoBack();
            e.Handled = true;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 640)
                WindowTitleTextBox.Margin = new Thickness(110, 14, 0, 0);
            else
                WindowTitleTextBox.Margin = new Thickness(67, 14, 0, 0);
        }

        public void InitializeAccountSettings()
        {
            (App.Current as App)._itemViewHolder.LoginMenuItemVisibility = !(App.Current as App)._itemViewHolder.User.IsLoggedIn;
        }
        
        private void InitializeLocalSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values[FileManager.playingSoundsListVisibleKey] == null)
            {
                localSettings.Values[FileManager.playingSoundsListVisibleKey] = FileManager.playingSoundsListVisible;
                (App.Current as App)._itemViewHolder.PlayingSoundsListVisibility = FileManager.playingSoundsListVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                (App.Current as App)._itemViewHolder.PlayingSoundsListVisibility = (bool)localSettings.Values[FileManager.playingSoundsListVisibleKey] ? Visibility.Visible : Visibility.Collapsed;
            }

            if (localSettings.Values[FileManager.playOneSoundAtOnceKey] == null)
            {
                localSettings.Values[FileManager.playOneSoundAtOnceKey] = FileManager.playOneSoundAtOnce;
                (App.Current as App)._itemViewHolder.PlayOneSoundAtOnce = FileManager.playOneSoundAtOnce;
            }
            else
            {
                (App.Current as App)._itemViewHolder.PlayOneSoundAtOnce = (bool)localSettings.Values[FileManager.playOneSoundAtOnceKey];
            }

            if (localSettings.Values[FileManager.liveTileKey] == null)
            {
                localSettings.Values[FileManager.liveTileKey] = FileManager.liveTile;
            }

            if (localSettings.Values[FileManager.showCategoryIconKey] == null)
            {
                localSettings.Values[FileManager.showCategoryIconKey] = FileManager.showCategoryIcon;
                (App.Current as App)._itemViewHolder.ShowCategoryIcon = FileManager.showCategoryIcon;
            }
            else
            {
                (App.Current as App)._itemViewHolder.ShowCategoryIcon = (bool)localSettings.Values[FileManager.showCategoryIconKey];
            }

            if (localSettings.Values[FileManager.showSoundsPivotKey] == null)
            {
                localSettings.Values[FileManager.showSoundsPivotKey] = FileManager.showSoundsPivot;
                (App.Current as App)._itemViewHolder.ShowSoundsPivot = FileManager.showSoundsPivot;
            }
            else
            {
                (App.Current as App)._itemViewHolder.ShowSoundsPivot = (bool)localSettings.Values[FileManager.showSoundsPivotKey];
            }

            if(localSettings.Values[FileManager.savePlayingSoundsKey] == null)
            {
                localSettings.Values[FileManager.savePlayingSoundsKey] = FileManager.savePlayingSounds;
                (App.Current as App)._itemViewHolder.SavePlayingSounds = FileManager.savePlayingSounds;
            }
            else
            {
                (App.Current as App)._itemViewHolder.savePlayingSounds = (bool)localSettings.Values[FileManager.savePlayingSoundsKey];
            }
        }
        
        private void CustomiseTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = ((App.Current as App).RequestedTheme == ApplicationTheme.Dark) ? Colors.White : Colors.Black;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void SideBar_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            FileManager.GoBack();
        }
        
        private async void SideBar_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            (App.Current as App)._itemViewHolder.SelectedSounds.Clear();

            FileManager.ResetSearchArea();

            // Display all Sounds with the selected category
            if (args.IsSettingsInvoked == true)
            {
                (App.Current as App)._itemViewHolder.Page = typeof(SettingsPage);
                (App.Current as App)._itemViewHolder.Title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("Settings-Title");
                (App.Current as App)._itemViewHolder.EditButtonVisibility = Visibility.Collapsed;
                (App.Current as App)._itemViewHolder.PlayAllButtonVisibility = Visibility.Collapsed;
                (App.Current as App)._itemViewHolder.IsBackButtonEnabled = true;
            }
            else
            {
                // Find the selected category in the categories list and set selectedCategory
                var category = (Category)args.InvokedItem;
                for(int i = 0; i < (App.Current as App)._itemViewHolder.Categories.Count(); i++)
                {
                    if ((App.Current as App)._itemViewHolder.Categories[i].Uuid == category.Uuid)
                    {
                        (App.Current as App)._itemViewHolder.SelectedCategory = i;
                    }
                }

                if ((App.Current as App)._itemViewHolder.SelectedCategory == 0)
                {
                    await FileManager.ShowAllSounds();
                }
                else
                {
                    await FileManager.ShowCategory(category.Uuid);
                }
            }
        }

        private void LogInMenuItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            (App.Current as App)._itemViewHolder.Title = (new Windows.ApplicationModel.Resources.ResourceLoader()).GetString("Account-Title");
            (App.Current as App)._itemViewHolder.Page = typeof(AccountPage);
            (App.Current as App)._itemViewHolder.EditButtonVisibility = Visibility.Collapsed;
            (App.Current as App)._itemViewHolder.PlayAllButtonVisibility = Visibility.Collapsed;
            (App.Current as App)._itemViewHolder.IsBackButtonEnabled = true;

            if (SideBar.DisplayMode == NavigationViewDisplayMode.Compact ||
                SideBar.DisplayMode == NavigationViewDisplayMode.Minimal)
                SideBar.IsPaneOpen = false;
        }
    }
}