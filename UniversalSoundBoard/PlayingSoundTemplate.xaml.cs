﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UniversalSoundBoard.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UniversalSoundBoard
{
    public sealed partial class PlayingSoundTemplate : UserControl
    {
        public PlayingSound PlayingSound { get; set; }

        CoreDispatcher dispatcher;

        public PlayingSoundTemplate()
        {
            this.InitializeComponent();
            Loaded += PlayingSoundTemplate_Loaded;

            setDataContext();
            DataContextChanged += PlayingSoundTemplate_DataContextChanged;

            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private void PlayingSoundTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if(this.DataContext != null)
            {
                this.PlayingSound = this.DataContext as PlayingSound;

                initializePlayingSound();
            }
        }

        private void PlayingSoundTemplate_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            initializePlayingSound();
            setMediaPlayerElementIsCompact();
        }

        private void setDataContext()
        {
            ContentRoot.DataContext = (App.Current as App)._itemViewHolder;
        }

        private void setMediaPlayerElementIsCompact()
        {
            if(Window.Current.Bounds.Width < 1000 && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                MediaPlayerElement.TransportControls.IsCompact = false;
            }else
            {
                MediaPlayerElement.TransportControls.IsCompact = true;
            }
        }

        private void repeatSound(int repetitions)
        {
            this.PlayingSound.repetitions = repetitions;
        }

        private void initializePlayingSound()
        {
            if (this.PlayingSound.MediaPlayer != null)
            {
                MediaPlayerElement.SetMediaPlayer(this.PlayingSound.MediaPlayer);
                MediaPlayerElement.MediaPlayer.MediaEnded -= Player_MediaEnded;
                MediaPlayerElement.MediaPlayer.MediaEnded += Player_MediaEnded;
                ((MediaPlaybackList)PlayingSound.MediaPlayer.Source).CurrentItemChanged -= PlayingSoundTemplate_CurrentItemChanged;
                ((MediaPlaybackList)PlayingSound.MediaPlayer.Source).CurrentItemChanged += PlayingSoundTemplate_CurrentItemChanged;
                PlayingSoundName.Text = this.PlayingSound.CurrentSound.Name;
                if(this.PlayingSound.repetitions >= 0)
                {
                    MediaPlayerElement.MediaPlayer.Play();
                }
            }
        }

        private void removePlayingSound()
        {
            if (MediaPlayerElement.MediaPlayer != null)
            {
                this.PlayingSound.MediaPlayer.Pause();
                //this.PlayingSound.MediaPlayer.IsMuted = true;
                //this.PlayingSound.MediaPlayer.PlaybackSession.Position = this.PlayingSound.MediaPlayer.PlaybackSession.NaturalDuration;
                MediaPlayerElement.MediaPlayer.MediaEnded -= Player_MediaEnded;
                ((MediaPlaybackList)PlayingSound.MediaPlayer.Source).CurrentItemChanged -= PlayingSoundTemplate_CurrentItemChanged;
                MediaPlayerElement.SetMediaPlayer(null);
                PlayingSoundName.Text = "";
                this.PlayingSound.MediaPlayer = null;
            }
            SoundPage.RemovePlayingSound(this.PlayingSound);
        }

        private async void Player_MediaEnded(MediaPlayer sender, object args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.PlayingSound.repetitions--;
                if (this.PlayingSound.repetitions <= 0)
                {
                    removePlayingSound();
                }
                
                if(this.PlayingSound.repetitions >= 0 && this.PlayingSound.MediaPlayer != null)
                {
                    if (this.PlayingSound.Sounds.Count > 1)
                    {
                        // Multiple Sounds in the list
                        ((MediaPlaybackList)this.PlayingSound.MediaPlayer.Source).MoveTo(0);
                    }
                    this.PlayingSound.MediaPlayer.Play();
                }
            });
        }

        private async void PlayingSoundTemplate_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if(this.PlayingSound.Sounds.Count > 1)
                {
                    if (this.PlayingSound.Sounds.Count > 1 && sender.CurrentItemIndex < this.PlayingSound.Sounds.Count)
                    {
                        this.PlayingSound.CurrentSound = this.PlayingSound.Sounds.ElementAt((int)sender.CurrentItemIndex);
                        PlayingSoundName.Text = this.PlayingSound.CurrentSound.Name;
                    }
                }
            });
        }

        private void CustomMediaTransportControls_Removed(object sender, EventArgs e)
        {
            removePlayingSound();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setMediaPlayerElementIsCompact();
        }

        private void CustomMediaTransportControls_Repeat_1x_Clicked(object sender, EventArgs e)
        {
            repeatSound(1);
        }

        private void CustomMediaTransportControls_Repeat_2x_Clicked(object sender, EventArgs e)
        {
            repeatSound(2);
        }

        private void CustomMediaTransportControls_Repeat_5x_Clicked(object sender, EventArgs e)
        {
            repeatSound(5);
        }

        private void CustomMediaTransportControls_Repeat_10x_Clicked(object sender, EventArgs e)
        {
            repeatSound(10);
        }

        private void CustomMediaTransportControls_Repeat_endless_Clicked(object sender, EventArgs e)
        {
            repeatSound(int.MaxValue);
        }
    }
}