﻿using davClassLibrary.Common;
using davClassLibrary.Models;
using System;
using UniversalSoundBoard.DataAccess;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UniversalSoundboard.Common
{
    public class TriggerAction : ITriggerAction
    {
        public async void UpdateAllOfTable(int tableId)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (tableId == FileManager.SoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.AddAllSounds());
            else if (tableId == FileManager.CategoryTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.CreateCategoriesListAsync());
            else if (tableId == FileManager.PlayingSoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.CreatePlayingSoundsListAsync());

            if (FileManager.itemViewHolder.AppState == FileManager.AppState.InitialSync)
                FileManager.itemViewHolder.AppState = FileManager.AppState.Normal;
        }

        public async void UpdateTableObject(TableObject tableObject, bool fileDownloaded)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (tableObject.TableId == FileManager.SoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.ReloadSound(tableObject.Uuid));
            else if(tableObject.TableId == FileManager.CategoryTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.ReloadCategory(tableObject.Uuid));
            else if(tableObject.TableId == FileManager.PlayingSoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await FileManager.UpdatePlayingSoundListItemAsync(tableObject.Uuid));
        }

        public async void DeleteTableObject(TableObject tableObject)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (tableObject.TableId == FileManager.SoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () => FileManager.RemoveSound(tableObject.Uuid));
            else if (tableObject.TableId == FileManager.CategoryTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () => FileManager.RemoveCategory(tableObject.Uuid));
            else if (tableObject.TableId == FileManager.PlayingSoundTableId)
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () => FileManager.RemovePlayingSound(tableObject.Uuid));
        }

        public void SyncFinished()
        {
            FileManager.syncFinished = true;
        }
    }
}
