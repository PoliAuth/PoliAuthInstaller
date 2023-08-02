using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Threading;

namespace PoliAuth_Installer
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("-- Creating PoliAuth folder --");
                var destFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                destFolder = Path.Combine(destFolder, "poliauth");
                string fileNameAuth = "PoliAuthenticator.exe";
                string fileNameJump = "PoliAuthJumper.exe";
                Directory.CreateDirectory(destFolder);
                var sourceFileAuth = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileNameAuth);
                var destFileAuth = System.IO.Path.Combine(destFolder, fileNameAuth);
                var sourceFileJump = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileNameJump);
                var destFileJump = System.IO.Path.Combine(destFolder, fileNameJump);

                Console.WriteLine("Downloading files, please wait...");

                Console.WriteLine("Downloading PoliAuthenticator.exe...");
                await DownloadFileAsync("https://github.com/PoliAuth/PoliAuthenticator/releases/latest/download/PoliAuthenticator.exe", destFileAuth);

                Console.WriteLine("\nDownloading PoliAuth_Jumper.exe...");
                await DownloadFileAsync("https://github.com/PoliAuth/PoliAuthJumper/blob/main/distWin-x64/PoliAuth_Jumper.exe?raw=true", destFileJump);

                Console.WriteLine("\nDownload completed!");

                Console.WriteLine("-- File downloaded in " + destFileAuth + " --");
                //Start and init db
                Console.WriteLine("-- Initializing DB --");
                ProcessStartInfo info = new ProcessStartInfo(destFileAuth);
                info.WorkingDirectory = destFolder;
                info.UseShellExecute = false;
                info.Arguments = "-init";
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return;
                }
                info.LoadUserProfile = true;
                Process.Start(info);
                //Create Registry
                Console.WriteLine("-- Setting Registry Key --");
                var KeyTest = Registry.CurrentUser.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);
                RegistryKey? key = KeyTest?.CreateSubKey("poliauth");
                key?.SetValue("URL Protocol", "poliauth");
                key?.CreateSubKey(@"shell\open\command").SetValue("", "\"" + destFileJump + "\" \"%1\"");
                Console.WriteLine("-- All Done! --");
                Console.WriteLine("To run PoliAuthenticator, open the folder " + destFolder + " and launch PoliAuthenticator.exe");
                Console.ReadKey();

            }
            catch (Exception e)
            {
                Console.WriteLine("-- There was an error in execution, please send this log to poliauth@gmail.com --");
                Console.WriteLine("");
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var totalBytes = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
                        var totalReadBytes = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        do
                        {
                            var readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (readBytes == 0)
                            {
                                isMoreToRead = false;
                                TriggerProgressChanged(totalBytes, totalBytes); // Trigger 100% progress
                                continue;
                            }

                            await fileStream.WriteAsync(buffer, 0, readBytes);

                            totalReadBytes += readBytes;
                            TriggerProgressChanged(totalReadBytes, totalBytes);
                        } while (isMoreToRead);
                    }
                }
            }
        }

        private static void TriggerProgressChanged(long totalReadBytes, long totalBytes)
        {
            double percentage = (double)totalReadBytes / totalBytes * 100;
            int totalBars = 50;
            int filledBars = (int)(totalBars * percentage / 100);

            Console.Write("\r[");
            Console.Write(new string('=', filledBars));
            Console.Write(new string(' ', totalBars - filledBars));
            Console.Write($"] {percentage:0.00}%");
        }
    }

}
