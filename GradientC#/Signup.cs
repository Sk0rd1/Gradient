using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;

namespace GradientC_
{
    internal class Signup
    {
        private string userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data");

        public async Task WaitForPageLoad(IWebDriver driver, int timeoutSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").Equals("complete"));

            await Task.Delay(3000);
        }

        public async Task<bool> Start(ProxyCredentials proxyCredentials, AccountCredentials accountCredentials, string referalLink)
        {
            Console.WriteLine("SignUp: " + accountCredentials.Email);

            // Отримуємо шлях до профілю Chrome поточного користувача
            string defaultProfilePath = Path.Combine(userDataPath, "Default");

            // Створюємо унікальний шлях для нового профілю
            string newProfileName = "ProfileChrome_" + Guid.NewGuid().ToString();
            string newProfilePath = Path.Combine(userDataPath, newProfileName);

            // Створюємо директорію для нового профілю
            Directory.CreateDirectory(newProfilePath);

            // Копіюємо розширення з профілю за замовчуванням
            string defaultExtensionsPath = Path.Combine(userDataPath, "Default\\Extensions");
            string newExtensionsPath = Path.Combine(newProfilePath, "Extensions");

            /*if (Directory.Exists(defaultExtensionsPath))
            {
                try
                {
                    // Створюємо директорію для розширень в новому профілі
                    Directory.CreateDirectory(newExtensionsPath);

                    // Копіюємо всі розширення
                    foreach (string extensionDir in Directory.GetDirectories(defaultExtensionsPath))
                    {
                        string extensionName = Path.GetFileName(extensionDir);
                        string targetPath = Path.Combine(newExtensionsPath, extensionName);
                        CopyDirectory(extensionDir, targetPath);
                    }
                }
                catch (Exception ex)
                {

                }
            }*/

            var options = new ChromeOptions();

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            options.AddHttpProxy(proxyCredentials.Host, proxyCredentials.Port, proxyCredentials.Username, proxyCredentials.Password);

            options.AddArgument($"user-data-dir={newProfilePath}");
            options.AddArguments("--enable-extensions");
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-insecure-localhost");
            options.AddArguments("--load-extension=C:\\Users\\ipz21\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\caacbgbklghmpodbdafajbgdnegacfmo\\1.0.21_0");

            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-3d-apis");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-background-networking");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-breakpad");
            options.AddArgument("--disable-client-side-phishing-detection");
            options.AddArgument("--disable-default-apps");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-sync");

            //options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.fonts", 2);
            options.AddUserProfilePreference("profile.default_content_settings.webgl", 2);


            //options.AddArguments("headless");

            IWebDriver driver = new ChromeDriver(service, options);

            try
            {
                Random random = new Random();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                driver.Navigate().GoToUrl(referalLink);

                await Task.Delay(1000);

                var allWindows = driver.WindowHandles;

                bool isFind = false;

                foreach (var window in allWindows)
                {
                    driver.SwitchTo().Window(window);

                    string currentUrl = driver.Url;

                    if (currentUrl == referalLink && !isFind)
                    {
                        isFind = true;
                    }
                    else
                    {
                        driver.Close();
                    }
                }

                allWindows = driver.WindowHandles;
                driver.SwitchTo().Window(allWindows[0]);
                await WaitForPageLoad(driver);

                await Task.Delay(1000);

                /*
                //пошта
                IWebElement input = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[2]/div[1]/input"));

                input.SendKeys(accountCredentials.Email);

                IWebElement continueButton = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[2]/button[1]"));

                await Task.Delay(1000);

                continueButton.Click();

                await WaitForPageLoad(driver);

                //реферальний код
                continueButton = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/button"));

                await Task.Delay(1000);

                continueButton.Click();

                await WaitForPageLoad(driver);

                //пароль
                input = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[3]/div[1]/span/input"));

                input.SendKeys(accountCredentials.Password);

                input = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[3]/div[2]/span/input"));

                input.SendKeys(accountCredentials.Password);

                await Task.Run(() => Console.In.ReadLine()); */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return false;
            }

            return true;

        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            // Копіюємо всі файли
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
            }

            // Рекурсивно копіюємо всі піддиректорії
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destDir = Path.Combine(targetDir, dirName);
                CopyDirectory(subDir, destDir);
            }
        }

    }
}
