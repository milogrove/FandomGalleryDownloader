using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FandomGalleryDownloader.AvaloniaGUI.Services;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.IO;

namespace FandomGalleryDownloader.AvaloniaGUI;

public partial class ProgressDialog : Window
{
    private CancellationTokenSource cancellationTokenSource;


    public ProgressDialog(List<string> _links, string _savePath, int _threads)
    {
        InitializeComponent();
        Height = 300;
        Width = 300;

        cancellationTokenSource = new CancellationTokenSource();
        StartProgress(_links, _savePath, _threads, cancellationTokenSource);
    }

    private async void StartProgress(List<string> links, string savePath, int threads, CancellationTokenSource cancellationTokenSource)
    {

        var progress = new Progress<int>(value =>
        {
            ProgressBar.Value = value;
            ProgressText.Text = $"Images: {ProgressBar.Value}/{ProgressBar.Maximum}";
        });

        var progressMAX = new Progress<int>(value =>
        {
            ProgressBar.Maximum = value;
            ProgressText.Text = $"Images: {ProgressBar.Value}/{ProgressBar.Maximum}";
        });

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < links.Count; i++)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }

            string link = links[i];
            ProgressLinksText.Text = $"Links: {i + 1}/{links.Count}";
            try
            {
                await NetConnector.Download(link, savePath, threads, progress, progressMAX, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                TextInfo.Text = "Download canceled";
                break;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await MessageBoxManager.GetMessageBoxStandard("ERROR", "ERROR: " + ex.Message).ShowAsync();
            }
        }

        stopwatch.Stop();

        if (!string.IsNullOrEmpty(TextInfo.Text))
        {
            if (TextInfo.Text.StartsWith("Please wait,"))
                TextInfo.Text = "Download completed";
        }




        TimeSpan elapsed = stopwatch.Elapsed;
        string formattedTime = string.Format("{0:mm\\:ss\\.fff}", elapsed);
        TimeText.Text = "Download time: " + formattedTime;
        TimeText.IsVisible = true;

        CancelButton.IsVisible = false;
        ExitButton.IsVisible = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        cancellationTokenSource.Cancel();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}