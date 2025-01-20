using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V129.Runtime;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;

namespace GradientC_
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Proxy.CheckLocalIP();

            Console.Clear();
            Console.WriteLine(CenterText("====================================================="));
            Console.WriteLine(CenterText("                 Gradient Auto Bot                 "));
            Console.WriteLine(CenterText("-----------------------------------------------------"));
            Console.WriteLine(CenterText("          https://t.me/m_1_sh_a                    "));
            Console.WriteLine(CenterText("-----------------------------------------------------"));
            Console.WriteLine(CenterText("Solana: BJWo4wpiSVy6FdcpWyvfW6VXCJJstGq1N616bPdysfd5"));
            Console.WriteLine(CenterText("Etherium: 0x7b692583cc51220C86742C333D98714C07e1B2e1"));
            Console.WriteLine(CenterText("====================================================="));
            Console.WriteLine();
            Console.WriteLine(" Input: login/signup/exit");
            Console.WriteLine();

            string extensionUrl = "URL_завантаження_extension.crx";
            string savePath = "/шлях/до/розширення/extension.crx";
            bool isDownloaded = DownloadExtensionAsync(extensionUrl, savePath).Result;

            await Profile.RemoveAnotherProfiles();

            while (true)
            {
                string input = await Console.In.ReadLineAsync();

                switch (input.ToLower())
                {
                    case "login":
                        _ = LoginProcess();
                        break;
                    case "signup":
                        _ = SignupProcess();
                        break;
                    case "exit":
                        Console.WriteLine(" => Exit");
                        return;
                    default:
                        Console.WriteLine("Unknown command. Try again.");
                        break;
                }
            }

        }

        static async Task LoginProcess()
        {
            Console.WriteLine(" => Login...");

            var accountEngine = new AccountEngine("proxies.txt", "accounts.txt");
            await accountEngine.Initialize(false);
            await accountEngine.StartLogin();
        }

        static async Task SignupProcess()
        {
            Console.WriteLine(" => Sign up...");

            var accountEngine = new AccountEngine("proxies.txt", "accounts.txt", "referalLink.txt");
            await accountEngine.Initialize(true);
            await accountEngine.StartSignUp();
        }

        static string CenterText(string text)
        {
            int consoleWidth = Console.WindowWidth;
            int padding = (consoleWidth - text.Length) / 2;
            return text.PadLeft(text.Length + padding);
        }

        public static async Task<bool> DownloadExtensionAsync(string extensionUrl, string savePath)
        {
            if (File.Exists(savePath))
            {
                Console.WriteLine($"The extension already exists.");
                return true;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(extensionUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                 fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine("Extension successfully loaded.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading extension: {ex.Message}.");
                    return false;
                }
            }
        }
    }
}
