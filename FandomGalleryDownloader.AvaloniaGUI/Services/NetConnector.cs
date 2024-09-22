using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FandomGalleryDownloader.AvaloniaGUI.Services
{
    internal static class NetConnector
    {

        public static async Task<string> GetHTMLSourceCode(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(url);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "\n" + url);
                }

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();
                    return html;
                }
                else
                {
                    throw new Exception("Cannot connect to the following address:\n" + url);
                }
            }
        }


        public static async Task DownloadImagesAsync(List<string> urls, string path, int threads, IProgress<int> progress, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using HttpClient client = new HttpClient();
            using SemaphoreSlim semaphore = new SemaphoreSlim(threads);
            int successfulDownloads = 0;

            List<Task> tasks = new List<Task>();

            foreach (string url in urls)
            {
                if (token.IsCancellationRequested)
                    break;

                var task = Task.Run(async () =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();

                        await semaphore.WaitAsync(token);
                        try
                        {
                            await DownloadImageAsync(url, client, path, token);
                            Interlocked.Increment(ref successfulDownloads);
                            progress?.Report(successfulDownloads);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception)
                    {

                    }
                }, token);

                tasks.Add(task);

                // Limiting the number of tasks to the number of threads
                if (tasks.Count >= threads)
                {
                    Task finishedTask = await Task.WhenAny(tasks);
                    tasks.Remove(finishedTask);
                }
            }

            await Task.WhenAll(tasks);
        }




        private static async Task DownloadImageAsync(string url, HttpClient client, string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                string fileName = Path.GetFileName(new Uri(url).LocalPath);
                fileName = StringEditor.FixInvalidFilename(fileName);
                byte[] imageBytes = await client.GetByteArrayAsync(url);
                fileName = Path.Combine(path, fileName);
                await File.WriteAllBytesAsync(fileName, imageBytes);
            }
            catch (Exception)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(path, "errors.txt"), append: true))
                {
                    sw.WriteLine(url);
                }
            }
        }

        public static async Task Download(string url, string path, int threads, IProgress<int> progress, IProgress<int> progressMAX, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            token.ThrowIfCancellationRequested();

            string sourceCode = await NetConnector.GetHTMLSourceCode(url);

            if (!string.IsNullOrEmpty(sourceCode))
            {
                List<string> pics = StringEditor.getPicsList(sourceCode, true, path);

                progressMAX.Report(pics.Count);

                if (pics.Count > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await NetConnector.DownloadImagesAsync(pics, path, threads, progress, token);
                }
                else
                {
                    throw new Exception("There are no images to download at this address:\n" + url);
                }
            }
            else
            {
                throw new Exception("Cannot connect to the following address:\n" + url);
            }
        }
    }
}
