using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Themes.Fluent;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Markup.Xaml;
using FandomGalleryDownloader.AvaloniaGUI.Services;
using System.Threading;

namespace FandomGalleryDownloader.AvaloniaGUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Height = 310;
            Width = 500;

            if (Environment.ProcessorCount > 6)
                MyThreads.Value = 6;
            else
                MyThreads.Value = Environment.ProcessorCount;
        }

        private async void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var folderResult = await this.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select a Folder",
            });

            if (folderResult != null && folderResult.Any())
            {
                var selectedFolder = folderResult[0];
                SavePathTextBox.Text = selectedFolder.Path.LocalPath;
            }

        }



        private async void OnDownloadButtonClick(object sender, RoutedEventArgs e)
        {

            string inputData = InputTextBox.Text ?? "";
            string savePath = SavePathTextBox.Text ?? "";
            int threads = 1;
            try
            {
                threads = Convert.ToInt32(MyThreads.Value);
            }
            catch (Exception)
            {
                threads = 1;
            }



            if (!string.IsNullOrEmpty(inputData) && !string.IsNullOrEmpty(savePath))
            {
                List<string> links = inputData
                                    .Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(link => link.Trim())
                                    .ToList();

                var progressDialog = new ProgressDialog(links, savePath, threads);
                await progressDialog.ShowDialog(this);


            }
            else
            {
                await MessageBoxManager.GetMessageBoxCustom(
                     new MessageBoxCustomParams
                     {
                         ButtonDefinitions = new List<ButtonDefinition>
                         {
                            new ButtonDefinition { Name = "OK", },
                         },
                         ContentTitle = "Warning",
                         ContentMessage = "Please enter correct data",
                         Icon = MsBox.Avalonia.Enums.Icon.Error,
                         WindowStartupLocation = WindowStartupLocation.CenterOwner,
                         CanResize = false,
                         MaxWidth = 500,
                         MaxHeight = 800,
                         SizeToContent = SizeToContent.WidthAndHeight,
                         ShowInCenter = true,
                         Topmost = true,
                     }).ShowWindowDialogAsync(this); 
            }
        }
    }
}