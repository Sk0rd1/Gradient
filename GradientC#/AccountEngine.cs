using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GradientC_
{
    public class ProxyCredentials
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsWorking { get; set; } = false;
        public string OriginalString { get; set; }

        public static ProxyCredentials ParseFromString(string proxyString)
        {
            try
            {
                var match = Regex.Match(proxyString, @"http://([^:]+):([^@]+)@([^:]+):(\d+)");
                if (!match.Success)
                {
                    throw new FormatException("Неправильний формат проксi");
                }

                return new ProxyCredentials
                {
                    Username = match.Groups[1].Value,
                    Password = match.Groups[2].Value,
                    Host = match.Groups[3].Value,
                    Port = int.Parse(match.Groups[4].Value),
                    OriginalString = proxyString
                };
            }
            catch (Exception ex)
            {
                throw new FormatException($"Помилка парсингу проксi: {ex.Message}");
            }
        }
    }

    public class AccountCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public static AccountCredentials ParseFromString(string accountString)
        {
            try
            {
                var parts = accountString.Split(':');
                if (parts.Length != 2)
                {
                    throw new FormatException("Неправильний формат облікового запису");
                }

                return new AccountCredentials
                {
                    Email = parts[0].Trim(),
                    Password = parts[1].Trim()
                };
            }
            catch (Exception ex)
            {
                throw new FormatException($"Помилка парсингу облікового запису: {ex.Message}");
            }
        }
    }

    internal class AccountEngine
    {
        //значення для кількості proxy виділених на один акаунт
        private const int MAX_PROXIES_PER_ACCOUNT = 5;
        private string proxyFilePath;
        private string accountsFilePath;
        private string referalLinkPath;
        private string referalLink = String.Empty;
        private List<ProxyCredentials> listProxies;
        private List<AccountCredentials> listAccounts;
        private readonly System.Timers.Timer checkTimer;

        public AccountEngine(string proxyFilePath, string accountsFilePath)
        {
            this.proxyFilePath = proxyFilePath;
            this.accountsFilePath = accountsFilePath;
            listProxies = new List<ProxyCredentials>();
            listAccounts = new List<AccountCredentials>();
        }

        public AccountEngine(string proxyFilePath, string accountsFilePath, string referalLinkPath)
        {
            this.proxyFilePath = proxyFilePath;
            this.accountsFilePath = accountsFilePath;
            this.referalLinkPath = referalLinkPath;

            listProxies = new List<ProxyCredentials>();
            listAccounts = new List<AccountCredentials>();
        }

        public async Task Initialize(bool isSignUp)
        {

            await LoadProxiesFromFile();
            await LoadAccountsFromFile();

            if(isSignUp)
                await LoadReferalLinkFromFile();
        }

        private async Task LoadReferalLinkFromFile()
        {
            try
            {
                string linkFromFile = File.ReadLines(referalLinkPath).First();

                string pattern = @"https:\/\/app\.gradient\.network\/signup";

                if(Regex.IsMatch(linkFromFile, pattern))
                {
                    referalLink = linkFromFile;
                    Console.WriteLine(" => Referal link: " + referalLink);
                }
                else
                {
                    Console.WriteLine(" => Incorrect referal link !!!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task LoadProxiesFromFile()
        {
            try
            {
                string[] lines = await File.ReadAllLinesAsync(proxyFilePath);
                listProxies = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => ProxyCredentials.ParseFromString(line))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження проксі: {ex.Message}");
                listProxies = new List<ProxyCredentials>();
            }

            if (listProxies.Count == 0)
            {
                Console.WriteLine("Proxy not loaded.");
                return;
            }

            //перевірка проксі
            int proxyFromFile = listProxies.Count;
            int proxyThatWorks = 0;
            
            var tasks = listProxies.Select(async proxy =>
            {
                if (await Proxy.ProxyStatus(proxy))
                {   
                    Interlocked.Increment(ref proxyThatWorks);
                }
                else
                {
                    listProxies.Remove(proxy);
                }

            }).ToList();

            await Task.WhenAll(tasks);

            Console.WriteLine("Loaded proxy: " + proxyThatWorks + "/" + proxyFromFile);
        }

        private async Task LoadAccountsFromFile()
        {
            try
            {
                string[] lines = await File.ReadAllLinesAsync(accountsFilePath);
                listAccounts = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => AccountCredentials.ParseFromString(line))
                    .ToList();

                Console.WriteLine($"Loaded {listAccounts.Count} accounts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}");
                listAccounts = new List<AccountCredentials>();
            }
        }

        public async Task DeleteFailedProxy()
        {
            for (int i = 0; i < listProxies.Count; i++)
            {
                if (!listProxies[i].IsWorking)
                {
                    listProxies.RemoveAt(i);
                    i--;
                }
            }
        }

        public async Task StartLogin()
        {
            Login login = new Login();

            int numOfExtensions = Math.Min(MAX_PROXIES_PER_ACCOUNT * listAccounts.Count, listProxies.Count);

            List<Task<bool>> tasks = new List<Task<bool>>();
            List<bool> results = new List<bool>();

            //запускаємо одночасно вхід в акаунти
            for (int i = 0; i < numOfExtensions; i++)
            {
                if (i == 0)
                {
                    tasks.Add(Task.Run(async () => await login.Start(listProxies[0], listAccounts[0])));
                }
                else
                {
                    int proxyIndex = i;
                    int accountIndex = i / MAX_PROXIES_PER_ACCOUNT;
                    tasks.Add(Task.Run(async () => await login.Start(listProxies[proxyIndex], listAccounts[accountIndex])));
                }
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                results.Add(task.Result);
            }

            int countTrue = results.Count(result => result);
            Console.WriteLine(" #Result: " + countTrue + "/" + results.Count);
        }

        public async Task StartSignUp()
        {
            Signup signup = new Signup();

            int numOfExtensions = Math.Min(listAccounts.Count, listProxies.Count);

            Console.WriteLine("numOfExtensions" + numOfExtensions);

            List<Task<bool>> tasks = new List<Task<bool>>();
            List<bool> results = new List<bool>();

            //запускаємо одночасно вхід в акаунти
            for (int i = 0; i < numOfExtensions; i++)
            {
                if (i == 0)
                {
                    tasks.Add(Task.Run(async () => await signup.Start(listProxies[0], listAccounts[0], referalLink)));
                }
                else
                {
                    int proxyIndex = i;
                    int accountIndex = i / MAX_PROXIES_PER_ACCOUNT;
                    tasks.Add(Task.Run(async () => await signup.Start(listProxies[proxyIndex], listAccounts[accountIndex], referalLink)));
                }
            }

            await Task.WhenAll(tasks);

        }
    }
}
