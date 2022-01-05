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
        static void Main(string[] args)
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
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/PoliAuth/PoliAuthenticator/releases/download/Release/PoliAuthenticator.exe", destFileAuth);
                        client.DownloadFile("https://github.com/PoliAuth/PoliAuthJumper/blob/main/distWin-x64/PoliAuth_Jumper.exe?raw=true", destFileJump);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while downloading files. Please retry running this installer");
                    return;
                }
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
                var KeyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
                RegistryKey key = KeyTest.CreateSubKey("poliauth");
                key.SetValue("URL Protocol", "poliauth");
                key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + destFileJump + "\" \"%1\"");
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
    }
}
