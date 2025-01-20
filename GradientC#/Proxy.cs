using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using System.Net;

namespace GradientC_
{
    internal static class Proxy
    {
        static string localIP = String.Empty;
        static public async Task<bool> CheckLocalIP()
        {

            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            options.AddArguments("headless");
            options.AddArguments("disable-gpu");
            options.AddArguments("disable-logging");
            options.AddArgument("--start-maximized");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

            try
            {
                IWebDriver driver = new ChromeDriver(service, options);

                await Task.Run(() => driver.Navigate().GoToUrl("https://api.ipify.org"));

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                IWebElement pageElement = await Task.Run(() => driver.FindElement(By.TagName("body")));

                string pageContent = pageElement.Text;
                localIP = pageContent;

                driver.Quit();

                return !string.IsNullOrEmpty(localIP);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        static public async Task<bool> ProxyStatus(ProxyCredentials credentials)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.Proxy = new WebProxy
                {
                    Address = new Uri($"http://{credentials.Host}:{credentials.Port}"),
                    Credentials = new System.Net.NetworkCredential(credentials.Username, credentials.Password)
                };

                using (var client = new HttpClient(handler))
                {
                    try
                    {
                        var response = await client.GetAsync("https://api.ipify.org");
                        response.EnsureSuccessStatusCode();
                        string result = await response.Content.ReadAsStringAsync();
                        if (result == localIP)
                        {
                            return false;
                        }
                        else
                        {
                            credentials.IsWorking = true;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
        }

        /*static public async Task<bool> ProxyStatus(ProxyCredentials credentials)
        {
            if (credentials == null)
                return false;

            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            options.AddHttpProxy(credentials.Host, credentials.Port, credentials.Username, credentials.Password);

            options.AddArguments("headless");
            options.AddArguments("disable-gpu");
            options.AddArguments("disable-logging");
            options.AddArgument("--start-maximized");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

            using (IWebDriver driver = new ChromeDriver(service, options))
            {
                try
                {
                    driver.Navigate().GoToUrl("https://api.ipify.org");

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    string currentIp = driver.FindElement(By.TagName("body")).Text.Trim();

                    if (string.IsNullOrEmpty(currentIp) || !IPAddress.TryParse(currentIp, out _))
                    {
                        return false;
                    }

                    if (currentIp == localIP)
                    {
                        return false;
                    }
                    else
                    {
                        credentials.IsWorking = true;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }*/
    }
}
