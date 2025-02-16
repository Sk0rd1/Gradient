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


            Console.Clear();
            Console.WriteLine(LineText("="));
            Console.WriteLine(CenterText("                 Gradient Auto Bot                 "));
            Console.WriteLine(LineText("-"));
            Console.WriteLine(CenterText("          https://t.me/m_1_sh_a                    "));
            Console.WriteLine(LineText("-"));
            Console.WriteLine(CenterText("Solana: BJWo4wpiSVy6FdcpWyvfW6VXCJJstGq1N616bPdysfd5"));
            Console.WriteLine(CenterText("Etherium: 0x7b692583cc51220C86742C333D98714C07e1B2e1"));
            Console.WriteLine(LineText("="));
            Console.WriteLine("  Version: 2.0");
            Console.WriteLine(LineText("="));
            Console.WriteLine();

            //string extensionId = "caacbgbklghmpodbdafajbgdnegacfmo";
            //string crxUrl = $"https://clients2.google.com/service/update2/crx?response=redirect&prodversion=98.0.4758.102&acceptformat=crx2,crx3&x=id%3D{extensionId}%26uc&nacl_arch=x86-64";
            //string extensionFilename = "app.crx";
            
            //await DownloadExtension(crxUrl, extensionFilename);

            await Proxy.CheckLocalIP();

            int max_proxies_per_account = 5;

            Console.WriteLine(" Input number proxy nodes per account: ");
            max_proxies_per_account = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine(" Delete previous accounts?(Y/n): ");

            if(Console.ReadLine() == "Y")
                await Profile.RemoveAnotherProfiles();

            Console.WriteLine(" Input: login/signup/exit");

            while (true)
            {
                string input = await Console.In.ReadLineAsync();

                switch (input.ToLower())
                {
                    case "login":
                        _ = LoginProcess(max_proxies_per_account);
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

        private static async Task DownloadExtension(string url, string filename)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(filename, content);
                Console.WriteLine("Extension downloaded successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to download extension: {response.StatusCode}");
                throw new Exception("Failed to download extension.");
            }
        }
        static async Task LoginProcess(int max_proxies_per_account)
        {
            Console.WriteLine(" => Login...");

            var accountEngine = new AccountEngine("proxies.txt", "accounts.txt", max_proxies_per_account);
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

        static string LineText(string symbol)
        {
            int consoleWidth = Console.WindowWidth;

            string result = String.Empty;

            for (int i = 0; i < consoleWidth; i++)
            {
                result += symbol;
            }

            return result;
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
