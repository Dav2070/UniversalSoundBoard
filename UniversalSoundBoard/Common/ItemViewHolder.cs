﻿using davClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UniversalSoundboard.Models;
using UniversalSoundBoard.DataAccess;
using UniversalSoundBoard.Models;
using UniversalSoundBoard.Pages;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UniversalSoundBoard.Common
{
    public class ItemViewHolder : INotifyPropertyChanged
    {
        #region Constants for the localSettings keys
        private const string themeKey = "theme";
        private const string playingSoundsListVisibleKey = "playingSoundsListVisible";
        private const string playingSoundsBarWidthKey = "playingSoundsBarWidth";
        private const string openMultipleSoundsKey = "openMultipleSounds";
        private const string multiSoundPlaybackKey = "multiSoundPlayback";
        private const string liveTileKey = "liveTile";
        private const string showListViewKey = "showListView";
        private const string showCategoryIconKey = "showCategoryIcon";
        private const string showSoundsPivotKey = "showSoundsPivot";
        private const string savePlayingSoundsKey = "savePlayingSounds";
        private const string volumeKey = "volume";
        private const string mutedKey = "muted";
        private const string showAcrylicBackgroundKey = "showAcrylicBackground";
        private const string soundOrderKey = "soundOrder";
        private const string soundOrderReversedKey = "soundOrderReversed";
        #endregion

        #region Constants for localSettings defaults
        private const FileManager.AppTheme themeDefault = FileManager.AppTheme.System;
        private const bool playingSoundsListVisibleDefault = true;
        private const double playingSoundsBarWidthDefault = 0.35;
        private const bool openMultipleSoundsDefault = true;
        private const bool multiSoundPlaybackDefault = false;
        private const bool liveTileDefault = true;
        private const bool showListViewDefault = false;
        private const bool showCategoryIconDefault = true;
        private const bool showSoundsPivotDefault = true;
        private const bool savePlayingSoundsDefault = true;
        private const int volumeDefault = 100;
        private const bool mutedDefault = false;
        private const bool showAcrylicBackgroundDefault = true;
        private const FileManager.SoundOrder soundOrderDefault = FileManager.SoundOrder.Custom;
        private const bool soundOrderReversedDefault = false;
        #endregion

        #region Variables
        #region State
        private FileManager.AppState _appState;                             // The current state of the app
        private string _title;                                              // The title text
        private Type _page;                                                 // The current page
        private string _searchQuery;                                        // The string entered into the search box
        private bool _allSoundsChanged;                                     // If there was made change to one or multiple sounds so that a reload of the sounds is required
        private Guid _selectedCategory;                                     // The index of the currently selected category in the category list
        private DavUser _user;                                              // The User object with username and avatar
        private bool _exporting;                                            // If true the soundboard is currently being exported
        private bool _exported;                                             // If true the soundboard was exported in the app session
        private bool _importing;                                            // If true a soundboard is currently being imported
        private bool _imported;                                             // If true a soundboard was import in the app session
        private string _exportMessage;                                      // The text describing the status of the export
        private string _importMessage;                                      // The text describing the status of the import
        private string _soundboardSize;                                     // The text shown on the settings page which describes the size of the soundboard
        #endregion

        #region Lists
        public List<Category> Categories { get; }                           // A list of all categories
        public ObservableCollection<Sound> AllSounds { get; }               // A list of all sounds, unsorted
        public ObservableCollection<Sound> Sounds { get; }                  // A list of the sounds which are displayed when the Sound pivot is selected, sorted by the selected sort option
        public ObservableCollection<Sound> FavouriteSounds { get; }         // A list of the favourite sound which are displayed when the Favourite pivot is selected, sorted by the selected sort option
        public ObservableCollection<Sound> SelectedSounds { get; }          // A list of the sounds which are selected
        public ObservableCollection<PlayingSound> PlayingSounds { get; }    // A list of the Playing Sounds which are displayed in the right menu
        #endregion

        #region Layout & Design
        private FileManager.AppTheme _currentTheme;                         // The current theme of the app; is either Light or Dark
        private bool _progressRingIsActive;                                 // Shows the Progress Ring if true
        private bool _loadingScreenVisible;                                 // If true, the large loading screen is visible
        private string _loadingScreenMessage;                               // The text that is shown in the loading screen
        private bool _backButtonEnabled;                                    // If the Back Button is enabled
        private bool _multiSelectionEnabled;                                // If true, the GridView has multi selection and the multi selection buttons are visible
        private bool _playAllButtonVisible;                                 // If true shows the Play button next to the title, only when a category or All Sounds is selected
        private bool _editButtonVisible;                                    // If true shows the edit button next to the title, only when a category is selected
        private bool _topButtonsCollapsed;                                  // If true the buttons at the top show only the icon, if false they show the icon and text
        private bool _searchAutoSuggestBoxVisible;                          // If true the search box is visible, if false multi selection is on or the search button shown
        private bool _searchButtonVisible;                                  // If true the search button at the top is visible
        private string _selectAllFlyoutText;                                // The text of the Select All flyout item in the Navigation View Header
        private SymbolIcon _selectAllFlyoutIcon;                            // The icon of the Select All flyout item in the Navigation View Header
        private double _soundTileWidth;                                     // The width of all sound tiles in the GridViews
        private AcrylicBrush _playingSoundsBarAcrylicBackgroundBrush;       // This represents the background of the PlayingSoundsBar
        private bool _exportAndImportButtonsEnabled;                        // If true shows the export and import buttons on the settings page
        #endregion
        #endregion

        #region Settings
        private FileManager.AppTheme _theme;                                // The design theme of the app
        private bool _playingSoundsListVisible;                             // If true shows the Playing Sounds list at the right
        private double _playingSoundsBarWidth;                              // The relative width of the PlayingSoundsBar in percent
        private bool _openMultipleSounds;                                   // If false, removes all PlayingSounds whenever the user opens a new one; if true, adds new opened sounds to existing PlayingSounds
        private bool _multiSoundPlayback;                                   // If true, can play multiple sounds at the same time; if false, stops the currently playing sound when playing another sound
        private bool _liveTileEnabled;                                      // If true, show the live tile
        private bool _showListView;                                         // If true, shows the sounds on the SoundPage in a ListView
        private bool _showCategoryIcon;                                     // If true shows the icon of the category on the sound tile
        private bool _showSoundsPivot;                                      // If true shows the pivot to select Sounds or Favourite sounds
        private bool _savePlayingSounds;                                    // If true saves the PlayingSounds and loads them when starting the app
        private int _volume;                                                // The volume of the entire app, between 0 and 100
        private bool _muted;                                                // If true, the volume is muted
        private bool _showAcrylicBackground;                                // If true the acrylic background is visible
        private FileManager.SoundOrder _soundOrder;                         // The selected sound order in the settings
        private bool _soundOrderReversed;                                   // If the sound order is descending (false) or ascending (true)
        #endregion

        #region Events
        public event EventHandler SoundsLoadedEvent;                                // Is triggered when the sounds at startup were loaded
        public event EventHandler PlayingSoundsLoadedEvent;                         // Is triggered when the playing sounds at startup were loaded
        public event EventHandler CategoriesUpdatedEvent;                           // Is triggered when all categories were loaded into the Categories ObservableCollection
        public event EventHandler<Guid> CategoryUpdatedEvent;                       // Is triggered when a category was updated
        public event EventHandler<Guid> CategoryRemovedEvent;                       // Is triggered when a category was removed
        public event EventHandler<RoutedEventArgs> SelectAllSoundsEvent;            // Trigger this event to select all sounds or deselect all sounds when all sounds are selected
        public event EventHandler<SizeChangedEventArgs> SoundTileSizeChangedEvent;  // This event is triggered when the size of the sound tiles in the GridViews has changed
        public event EventHandler PlayingSoundItemStartSoundsListAnimationEvent;                // Is triggered from the SoundPage for starting the appropriate animation
        public event EventHandler<Guid> PlayingSoundItemShowSoundsListAnimationStartedEvent;    // Is triggered when the animation of a PlayingSound item to show the sounds list started
        public event EventHandler<Guid> PlayingSoundItemShowSoundsListAnimationEndedEvent;      // Is triggered when the animation of a PlayingSound item to show the sounds list ended
        public event EventHandler<Guid> PlayingSoundItemHideSoundsListAnimationStartedEvent;    // Is triggered when the animation of a PlayingSound item to hide the sounds list started
        public event EventHandler<Guid> PlayingSoundItemHideSoundsListAnimationEndedEvent;      // Is triggered when the animation of a PlayingSound item to hide the sounds list ended
        public event EventHandler<Guid> ShowPlayingSoundItemStartedEvent;                       // Is triggered when the PlayingSound appearing animation has started
        public event EventHandler<Guid> RemovePlayingSoundItemEvent;                            // Is triggered when the user wants to remove a PlayingSound, to start the BottomPlayingSoundsBar animation
        #endregion

        #region Local variables
        readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        #endregion

        public ItemViewHolder()
        {
            ResourceLoader loader = new ResourceLoader();

            // Set the default values
            #region State
            _title = loader.GetString("AllSounds");
            _page = typeof(SoundPage);
            _searchQuery = "";
            _allSoundsChanged = true;
            _selectedCategory = Guid.Empty;
            _user = new DavUser();
            _exporting = false;
            _exported = false;
            _importing = false;
            _imported = false;
            _exportMessage = "";
            _importMessage = "";
            _soundboardSize = "";
            #endregion

            #region Lists
            Categories = new List<Category>();
            AllSounds = new ObservableCollection<Sound>();
            Sounds = new ObservableCollection<Sound>();
            FavouriteSounds = new ObservableCollection<Sound>();
            SelectedSounds = new ObservableCollection<Sound>();
            PlayingSounds = new ObservableCollection<PlayingSound>();
            #endregion

            #region Layout & Design
            _currentTheme = FileManager.AppTheme.Light;
            _progressRingIsActive = false;
            _loadingScreenVisible = false;
            _loadingScreenMessage = "";
            _backButtonEnabled = false;
            _playAllButtonVisible = false;
            _editButtonVisible = false;
            _topButtonsCollapsed = false;
            _searchAutoSuggestBoxVisible = true;
            _searchButtonVisible = false;
            _selectAllFlyoutText = loader.GetString("MoreButton_SelectAllFlyout-SelectAll");
            _selectAllFlyoutIcon = new SymbolIcon(Symbol.SelectAll);
            _soundTileWidth = 200;
            _playingSoundsBarAcrylicBackgroundBrush = new AcrylicBrush();
            _exportAndImportButtonsEnabled = true;
            #endregion

            #region Settings
            // theme
            if (localSettings.Values[themeKey] == null)
                _theme = themeDefault;
            else
            {
                switch ((string)localSettings.Values[themeKey])
                {
                    case "light":
                        _theme = FileManager.AppTheme.Light;
                        break;
                    case "dark":
                        _theme = FileManager.AppTheme.Dark;
                        break;
                    case "system":
                        _theme = FileManager.AppTheme.System;
                        break;
                }
            }

            // playingSoundsListVisible
            if (localSettings.Values[playingSoundsListVisibleKey] == null)
                _playingSoundsListVisible = playingSoundsListVisibleDefault;
            else
                _playingSoundsListVisible = (bool)localSettings.Values[playingSoundsListVisibleKey];

            // playingSoundsBarWidth
            if (localSettings.Values[playingSoundsBarWidthKey] == null)
                _playingSoundsBarWidth = playingSoundsBarWidthDefault;
            else
                _playingSoundsBarWidth = (double)localSettings.Values[playingSoundsBarWidthKey];

            // openMultipleSounds
            if (localSettings.Values[openMultipleSoundsKey] == null)
                _openMultipleSounds = openMultipleSoundsDefault;
            else
                _openMultipleSounds = (bool)localSettings.Values[openMultipleSoundsKey];

            // multiSoundPlayback
            if (localSettings.Values[multiSoundPlaybackKey] == null)
                _multiSoundPlayback = multiSoundPlaybackDefault;
            else
                _multiSoundPlayback = (bool)localSettings.Values[multiSoundPlaybackKey];

            // liveTile
            if (localSettings.Values[liveTileKey] == null)
                _liveTileEnabled = liveTileDefault;
            else
                _liveTileEnabled = (bool)localSettings.Values[liveTileKey];

            // showListView
            if (localSettings.Values[showListViewKey] == null)
                _showListView = showListViewDefault;
            else
                _showListView = (bool)localSettings.Values[showListViewKey];

            // showCategoryIcon
            if (localSettings.Values[showCategoryIconKey] == null)
                _showCategoryIcon = showCategoryIconDefault;
            else
                _showCategoryIcon = (bool)localSettings.Values[showCategoryIconKey];

            // showSoundsPivot
            if (localSettings.Values[showSoundsPivotKey] == null)
                _showSoundsPivot = showSoundsPivotDefault;
            else
                _showSoundsPivot = (bool)localSettings.Values[showSoundsPivotKey];

            // savePlayingSounds
            if (localSettings.Values[savePlayingSoundsKey] == null)
                _savePlayingSounds = savePlayingSoundsDefault;
            else
                _savePlayingSounds = (bool)localSettings.Values[savePlayingSoundsKey];

            // volume
            if (localSettings.Values[volumeKey] == null)
                _volume = volumeDefault;
            else
            {
                // Backwards compatibility for saving the volume as double
                try
                {
                    // Try to read the volume as int
                    _volume = (int)localSettings.Values[volumeKey];
                }
                catch
                {
                    // Overwrite the volume with the default
                    localSettings.Values[volumeKey] = volumeDefault;
                    _volume = volumeDefault;
                }
            }

            // muted
            if (localSettings.Values[mutedKey] == null)
                _muted = mutedDefault;
            else
                _muted = (bool)localSettings.Values[mutedKey];

            // showAcrylicBackground
            if (localSettings.Values[showAcrylicBackgroundKey] == null)
                _showAcrylicBackground = showAcrylicBackgroundDefault;
            else
                _showAcrylicBackground = (bool)localSettings.Values[showAcrylicBackgroundKey];

            // soundOrder
            if (localSettings.Values[soundOrderKey] == null)
                _soundOrder = soundOrderDefault;
            else
                _soundOrder = (FileManager.SoundOrder)localSettings.Values[soundOrderKey];

            // soundOrderReversed
            if (localSettings.Values[soundOrderReversedKey] == null)
                _soundOrderReversed = soundOrderReversedDefault;
            else
                _soundOrderReversed = (bool)localSettings.Values[soundOrderReversedKey];
            #endregion
        }

        #region Access Modifiers
        #region State
        public const string AppStateKey = "AppState";
        public FileManager.AppState AppState
        {
            get => _appState;
            set
            {
                if (_appState.Equals(value)) return;
                _appState = value;
                NotifyPropertyChanged(AppStateKey);
            }
        }

        public const string TitleKey = "Title";
        public string Title
        {
            get => _title;
            set
            {
                if (_title.Equals(value)) return;
                _title = value;
                NotifyPropertyChanged(TitleKey);
            }
        }

        public const string PageKey = "Page";
        public Type Page
        {
            get => _page;
            set
            {
                if (_page.Equals(value)) return;
                _page = value;
                NotifyPropertyChanged(PageKey);
            }
        }

        public const string SearchQueryKey = "SearchQuery";
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery.Equals(value)) return;
                _searchQuery = value;
                NotifyPropertyChanged(SearchQueryKey);
            }
        }

        public const string AllSoundsChangedKey = "AllSoundsChanged";
        public bool AllSoundsChanged
        {
            get => _allSoundsChanged;
            set
            {
                if (_allSoundsChanged.Equals(value)) return;
                _allSoundsChanged = value;
                NotifyPropertyChanged(AllSoundsChangedKey);
            }
        }

        public const string SelectedCategoryKey = "SelectedCategory";
        public Guid SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory.Equals(value)) return;
                _selectedCategory = value;
                NotifyPropertyChanged(SelectedCategoryKey);
            }
        }

        public const string UserKey = "User";
        public DavUser User
        {
            get => _user;
            set
            {
                if (_user.Equals(value)) return;
                _user = value;
                NotifyPropertyChanged(UserKey);
            }
        }

        public const string ExportingKey = "Exporting";
        public bool Exporting
        {
            get => _exporting;
            set
            {
                if (_exporting.Equals(value)) return;
                _exporting = value;
                NotifyPropertyChanged(ExportingKey);
            }
        }

        public const string ExportedKey = "Exported";
        public bool Exported
        {
            get => _exported;
            set
            {
                if (_exported.Equals(value)) return;
                _exported = value;
                NotifyPropertyChanged(ExportedKey);
            }
        }

        public const string ImportingKey = "Importing";
        public bool Importing
        {
            get => _importing;
            set
            {
                if (_importing.Equals(value)) return;
                _importing = value;
                NotifyPropertyChanged(ImportingKey);
            }
        }

        public const string ImportedKey = "Imported";
        public bool Imported
        {
            get => _imported;
            set
            {
                if (_imported.Equals(value)) return;
                _imported = value;
                NotifyPropertyChanged(ImportedKey);
            }
        }

        public const string ExportMessageKey = "ExportMessage";
        public string ExportMessage
        {
            get => _exportMessage;
            set
            {
                if (_exportMessage.Equals(value)) return;
                _exportMessage = value;
                NotifyPropertyChanged(ExportMessageKey);
            }
        }

        public const string ImportMessageKey = "ImportMessage";
        public string ImportMessage
        {
            get => _importMessage;
            set
            {
                if (_importMessage.Equals(value)) return;
                _importMessage = value;
                NotifyPropertyChanged(ImportMessageKey);
            }
        }

        public const string SoundboardSizeKey = "SoundboardSize";
        public string SoundboardSize
        {
            get => _soundboardSize;
            set
            {
                if (_soundboardSize.Equals(value)) return;
                _soundboardSize = value;
                NotifyPropertyChanged(SoundboardSizeKey);
            }
        }
        #endregion

        #region Layout & Design
        public const string CurrentThemeKey = "CurrentTheme";
        public FileManager.AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme.Equals(value)) return;
                _currentTheme = value;
                NotifyPropertyChanged(CurrentThemeKey);
            }
        }

        public const string ProgressRingIsActiveKey = "ProgressRingIsActive";
        public bool ProgressRingIsActive
        {
            get => _progressRingIsActive;
            set
            {
                if (_progressRingIsActive.Equals(value)) return;
                _progressRingIsActive = value;
                NotifyPropertyChanged(ProgressRingIsActiveKey);
            }
        }

        public const string LoadingScreenVisibleKey = "LoadingScreenVisible";
        public bool LoadingScreenVisible
        {
            get => _loadingScreenVisible;
            set
            {
                if (_loadingScreenVisible.Equals(value)) return;
                _loadingScreenVisible = value;
                NotifyPropertyChanged(LoadingScreenVisibleKey);
            }
        }

        public const string LoadingScreenMessageKey = "LoadingScreenMessage";
        public string LoadingScreenMessage
        {
            get => _loadingScreenMessage;
            set
            {
                if (_loadingScreenMessage.Equals(value)) return;
                _loadingScreenMessage = value;
                NotifyPropertyChanged(LoadingScreenMessageKey);
            }
        }

        public const string BackButtonEnabledKey = "BackButtonEnabled";
        public bool BackButtonEnabled
        {
            get => _backButtonEnabled;
            set
            {
                if (_backButtonEnabled.Equals(value)) return;
                _backButtonEnabled = value;
                NotifyPropertyChanged(BackButtonEnabledKey);
            }
        }

        public const string MultiSelectionEnabledKey = "MultiSelectionEnabled";
        public bool MultiSelectionEnabled
        {
            get => _multiSelectionEnabled;
            set
            {
                if (_multiSelectionEnabled.Equals(value)) return;
                _multiSelectionEnabled = value;
                NotifyPropertyChanged(MultiSelectionEnabledKey);
            }
        }

        public const string PlayAllButtonVisibleKey = "PlayAllButtonVisible";
        public bool PlayAllButtonVisible
        {
            get => _playAllButtonVisible;
            set
            {
                if (_playAllButtonVisible.Equals(value)) return;
                _playAllButtonVisible = value;
                NotifyPropertyChanged(PlayAllButtonVisibleKey);
            }
        }

        public const string EditButtonVisibleKey = "EditButtonVisible";
        public bool EditButtonVisible
        {
            get => _editButtonVisible;
            set
            {
                if (_editButtonVisible.Equals(value)) return;
                _editButtonVisible = value;
                NotifyPropertyChanged(EditButtonVisibleKey);
            }
        }

        public const string TopButtonCollapsedKey = "TopButtonsCollapsed";
        public bool TopButtonsCollapsed
        {
            get => _topButtonsCollapsed;
            set
            {
                if (_topButtonsCollapsed.Equals(value)) return;
                _topButtonsCollapsed = value;
                NotifyPropertyChanged(TopButtonCollapsedKey);
            }
        }

        public const string SearchAutoSuggestBoxVisibleKey = "SearchAutoSuggestBoxVisible";
        public bool SearchAutoSuggestBoxVisible
        {
            get => _searchAutoSuggestBoxVisible;
            set
            {
                if (_searchAutoSuggestBoxVisible.Equals(value)) return;
                _searchAutoSuggestBoxVisible = value;
                NotifyPropertyChanged(SearchAutoSuggestBoxVisibleKey);
            }
        }

        public const string SearchButtonVisibleKey = "SearchButtonVisible";
        public bool SearchButtonVisible
        {
            get => _searchButtonVisible;
            set
            {
                if (_searchButtonVisible.Equals(value)) return;
                _searchButtonVisible = value;
                NotifyPropertyChanged(SearchButtonVisibleKey);
            }
        }

        public const string SelectAllFlyoutTextKey = "SelectAllFlyoutText";
        public string SelectAllFlyoutText
        {
            get => _selectAllFlyoutText;
            set
            {
                if (_selectAllFlyoutText.Equals(value)) return;
                _selectAllFlyoutText = value;
                NotifyPropertyChanged(SelectAllFlyoutTextKey);
            }
        }

        public const string SelectAllFlyoutIconKey = "SelectAllFlyoutIcon";
        public SymbolIcon SelectAllFlyoutIcon
        {
            get => _selectAllFlyoutIcon;
            set
            {
                if (_selectAllFlyoutIcon.Equals(value)) return;
                _selectAllFlyoutIcon = value;
                NotifyPropertyChanged(SelectAllFlyoutIconKey);
            }
        }

        public const string SoundTileWidthKey = "SoundTileWidth";
        public double SoundTileWidth
        {
            get => _soundTileWidth;
            set
            {
                if (_soundTileWidth.Equals(value)) return;
                _soundTileWidth = value;
                NotifyPropertyChanged(SoundTileWidthKey);
            }
        }

        public const string PlayingSoundsBarAcrylicBackgroundBrushKey = "PlayingSoundsBarAcrylicBackgroundBrush";
        public AcrylicBrush PlayingSoundsBarAcrylicBackgroundBrush
        {
            get => _playingSoundsBarAcrylicBackgroundBrush;
            set
            {
                if (_playingSoundsBarAcrylicBackgroundBrush.Equals(value)) return;
                _playingSoundsBarAcrylicBackgroundBrush = value;
                NotifyPropertyChanged(PlayingSoundsBarAcrylicBackgroundBrushKey);
            }
        }

        public const string ExportAndImportButtonsEnabledKey = "ExportAndImportButtonsEnabled";
        public bool ExportAndImportButtonsEnabled
        {
            get => _exportAndImportButtonsEnabled;
            set
            {
                if (_exportAndImportButtonsEnabled.Equals(value)) return;
                _exportAndImportButtonsEnabled = value;
                NotifyPropertyChanged(ExportAndImportButtonsEnabledKey);
            }
        }
        #endregion
        #endregion

        #region Settings
        public const string ThemeKey = "Theme";
        public FileManager.AppTheme Theme
        {
            get => _theme;
            set
            {
                if (_theme.Equals(value)) return;
                string themeString = "system";
                switch (value)
                {
                    case FileManager.AppTheme.Light:
                        themeString = "light";
                        break;
                    case FileManager.AppTheme.Dark:
                        themeString = "dark";
                        break;
                }

                localSettings.Values[themeKey] = themeString;
                _theme = value;
                NotifyPropertyChanged(ThemeKey);
            }
        }

        public const string PlayingSoundsListVisibleKey = "PlayingSoundsListVisible";
        public bool PlayingSoundsListVisible
        {
            get => _playingSoundsListVisible;
            set
            {
                if (_playingSoundsListVisible.Equals(value)) return;
                localSettings.Values[playingSoundsListVisibleKey] = value;
                _playingSoundsListVisible = value;
                NotifyPropertyChanged(PlayingSoundsListVisibleKey);
            }
        }

        public const string PlayingSoundsBarWidthKey = "PlayingSoundsBarWidth";
        public double PlayingSoundsBarWidth
        {
            get => _playingSoundsBarWidth;
            set
            {
                if (_playingSoundsBarWidth.Equals(value)) return;
                localSettings.Values[playingSoundsBarWidthKey] = value;
                _playingSoundsBarWidth = value;
                NotifyPropertyChanged(PlayingSoundsBarWidthKey);
            }
        }

        public const string OpenMultipleSoundsKey = "OpenMultipleSounds";
        public bool OpenMultipleSounds
        {
            get => _openMultipleSounds;
            set
            {
                if (_openMultipleSounds.Equals(value)) return;
                localSettings.Values[openMultipleSoundsKey] = value;
                _openMultipleSounds = value;
                NotifyPropertyChanged(OpenMultipleSoundsKey);
            }
        }

        public const string MultiSoundPlaybackKey = "MultiSoundPlayback";
        public bool MultiSoundPlayback
        {
            get => _multiSoundPlayback;
            set
            {
                if (_multiSoundPlayback.Equals(value)) return;
                localSettings.Values[multiSoundPlaybackKey] = value;
                _multiSoundPlayback = value;
                NotifyPropertyChanged(MultiSoundPlaybackKey);
            }
        }

        public const string LiveTileEnabledKey = "LiveTileEnabled";
        public bool LiveTileEnabled
        {
            get => _liveTileEnabled;
            set
            {
                if (_liveTileEnabled.Equals(value)) return;
                localSettings.Values[liveTileKey] = value;
                _liveTileEnabled = value;
                NotifyPropertyChanged(LiveTileEnabledKey);
            }
        }

        public const string ShowListViewKey = "ShowListView";
        public bool ShowListView
        {
            get => _showListView;
            set
            {
                if (_showListView.Equals(value)) return;
                localSettings.Values[showListViewKey] = value;
                _showListView = value;
                NotifyPropertyChanged(ShowListViewKey);
            }
        }

        public const string ShowCategoryIconKey = "ShowCategoryIcon";
        public bool ShowCategoryIcon
        {
            get => _showCategoryIcon;
            set
            {
                if (_showCategoryIcon.Equals(value)) return;
                localSettings.Values[showCategoryIconKey] = value;
                _showCategoryIcon = value;
                NotifyPropertyChanged(ShowCategoryIconKey);
            }
        }

        public const string ShowSoundsPivotKey = "ShowSoundsPivot";
        public bool ShowSoundsPivot
        {
            get => _showSoundsPivot;
            set
            {
                if (_showSoundsPivot.Equals(value)) return;
                localSettings.Values[showSoundsPivotKey] = value;
                _showSoundsPivot = value;
                NotifyPropertyChanged(ShowSoundsPivotKey);
            }
        }

        public const string SavePlayingSoundsKey = "SavePlayingSounds";
        public bool SavePlayingSounds
        {
            get => _savePlayingSounds;
            set
            {
                if (_savePlayingSounds.Equals(value)) return;
                localSettings.Values[savePlayingSoundsKey] = value;
                _savePlayingSounds = value;
                NotifyPropertyChanged(SavePlayingSoundsKey);
            }
        }

        public const string VolumeKey = "Volume";
        public int Volume
        {
            get => _volume;
            set
            {
                if (_volume.Equals(value)) return;
                localSettings.Values[volumeKey] = value;
                _volume = value;
                NotifyPropertyChanged(VolumeKey);
            }
        }

        public const string MutedKey = "Muted";
        public bool Muted
        {
            get => _muted;
            set
            {
                if (_muted.Equals(value)) return;
                localSettings.Values[mutedKey] = value;
                _muted = value;
                NotifyPropertyChanged(MutedKey);
            }
        }

        public const string ShowAcrylicBackgroundKey = "ShowAcrylicBackground";
        public bool ShowAcrylicBackground
        {
            get => _showAcrylicBackground;
            set
            {
                if (_showAcrylicBackground.Equals(value)) return;
                localSettings.Values[showAcrylicBackgroundKey] = value;
                _showAcrylicBackground = value;
                NotifyPropertyChanged(ShowAcrylicBackgroundKey);
            }
        }

        public const string SoundOrderKey = "SoundOrder";
        public FileManager.SoundOrder SoundOrder
        {
            get => _soundOrder;
            set
            {
                if (_soundOrder.Equals(value)) return;
                localSettings.Values[soundOrderKey] = (int)value;
                _soundOrder = value;
                NotifyPropertyChanged(SoundOrderKey);
            }
        }

        public const string SoundOrderReversedKey = "SoundOrderReversed";
        public bool SoundOrderReversed
        {
            get => _soundOrderReversed;
            set
            {
                if (_soundOrderReversed.Equals(value)) return;
                localSettings.Values[soundOrderReversedKey] = value;
                _soundOrderReversed = value;
                NotifyPropertyChanged(SoundOrderReversedKey);
            }
        }
        #endregion

        #region Events
        public void TriggerSoundsLoadedEvent()
        {
            SoundsLoadedEvent?.Invoke(null, null);
        }

        public void TriggerPlayingSoundsLoadedEvent()
        {
            PlayingSoundsLoadedEvent?.Invoke(null, null);
        }

        public void TriggerCategoriesUpdatedEvent()
        {
            CategoriesUpdatedEvent?.Invoke(null, null);
        }

        public void TriggerCategoryUpdatedEvent(Guid uuid)
        {
            CategoryUpdatedEvent?.Invoke(null, uuid);
        }

        public void TriggerCategoryRemovedEvent(Guid uuid)
        {
            CategoryRemovedEvent?.Invoke(null, uuid);
        }

        public void TriggerSelectAllSoundsEvent(object sender, RoutedEventArgs e)
        {
            SelectAllSoundsEvent?.Invoke(sender, e);
        }

        public void TriggerSoundTileSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            SoundTileSizeChangedEvent?.Invoke(sender, e);
        }

        public void TriggerPlayingSoundItemStartSoundsListAnimationEvent()
        {
            PlayingSoundItemStartSoundsListAnimationEvent?.Invoke(null, null);
        }

        public void TriggerPlayingSoundItemShowSoundsListAnimationStartedEvent(object sender, Guid uuid)
        {
            PlayingSoundItemShowSoundsListAnimationStartedEvent?.Invoke(sender, uuid);
        }

        public void TriggerPlayingSoundItemShowSoundsListAnimationEndedEvent(object sender, Guid uuid)
        {
            PlayingSoundItemShowSoundsListAnimationEndedEvent?.Invoke(sender, uuid);
        }

        public void TriggerPlayingSoundItemHideSoundsListAnimationStartedEvent(object sender, Guid uuid)
        {
            PlayingSoundItemHideSoundsListAnimationStartedEvent?.Invoke(sender, uuid);
        }

        public void TriggerPlayingSoundItemHideSoundsListAnimationEndedEvent(object sender, Guid uuid)
        {
            PlayingSoundItemHideSoundsListAnimationEndedEvent?.Invoke(sender, uuid);
        }

        public void TriggerShowPlayingSoundItemStartedEvent(object sender, Guid uuid)
        {
            ShowPlayingSoundItemStartedEvent?.Invoke(sender, uuid);
        }

        public void TriggerRemovePlayingSoundItemEvent(object sender, Guid uuid)
        {
            RemovePlayingSoundItemEvent?.Invoke(sender, uuid);
        }
        #endregion

        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
