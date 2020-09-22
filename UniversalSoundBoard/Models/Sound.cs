﻿using davClassLibrary.Models;
using System;
using System.Collections.Generic;
using UniversalSoundBoard.DataAccess;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using static davClassLibrary.Models.TableObject;

namespace UniversalSoundBoard.Models
{
    public class Sound{
        private const string DefaultLightSoundImageUri = "ms-appx:///Assets/Images/default.png";
        private const string DefaultDarkSoundImageUri = "ms-appx:///Assets/Images/default-dark.png";

        public Guid Uuid { get; }
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
        public bool Favourite { get; set; }
        public int DefaultVolume { get; set; }
        public bool DefaultMuted { get; set; }
        public BitmapImage Image { get; set; }
        public TableObject AudioFileTableObject { get; set; }
        public StorageFile AudioFile { get; set; }
        public TableObject ImageFileTableObject { get; set; }
        public StorageFile ImageFile { get; set; }

        public Sound(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
            Categories = new List<Category>();

            FileManager.itemViewHolder.TableObjectFileDownloadCompleted += ItemViewHolder_TableObjectFileDownloadCompleted;
        }

        private async void ItemViewHolder_TableObjectFileDownloadCompleted(object sender, UniversalSoundboard.Common.TableObjectFileDownloadCompletedEventArgs e)
        {
            if (e.Uuid.Equals(Guid.Empty) || e.File == null) return;

            if (AudioFileTableObject != null && e.Uuid.Equals(AudioFileTableObject.Uuid))
            {
                // Set the new audio file
                try
                {
                    AudioFile = await StorageFile.GetFileFromPathAsync(e.File.FullName);
                }
                catch { }
            }
            else if (ImageFileTableObject != null && e.Uuid.Equals(ImageFileTableObject.Uuid))
            {
                // Set the new image file
                try
                {
                    ImageFile = await StorageFile.GetFileFromPathAsync(e.File.FullName);
                }
                catch { }
            }
        }

        public string GetAudioFileExtension()
        {
            if (AudioFileTableObject == null) return null;
            return AudioFileTableObject.GetPropertyValue(FileManager.TableObjectExtPropertyName);
        }

        public void ScheduleAudioFileDownload(Progress<(Guid, int)> progress)
        {
            if (AudioFileTableObject == null) return;
            AudioFileTableObject.ScheduleFileDownload(progress);
        }

        public TableObjectFileDownloadStatus GetAudioFileDownloadStatus()
        {
            if (AudioFileTableObject == null) return TableObjectFileDownloadStatus.NoFileOrNotLoggedIn;
            return AudioFileTableObject.FileDownloadStatus;
        }

        public string GetImageFileExtension()
        {
            if (ImageFileTableObject == null) return null;
            return ImageFileTableObject.GetPropertyValue(FileManager.TableObjectExtPropertyName);
        }

        public static Uri GetDefaultImageUri()
        {
            Uri defaultImageUri;
            if (FileManager.itemViewHolder.CurrentTheme == FileManager.AppTheme.Dark)
                defaultImageUri = new Uri(DefaultDarkSoundImageUri, UriKind.Absolute);
            else
                defaultImageUri = new Uri(DefaultLightSoundImageUri, UriKind.Absolute);
            return defaultImageUri;
        }
    }
}
