using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciWeb;
using NuciWeb.Steam;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

using SteamProfileManager.Configuration;
using SteamProfileManager.Service;

namespace SteamProfileManager
{
    public sealed class Program
    {
        static BotSettings botSettings;
        static DebugSettings debugSettings;
        static NuciLoggerSettings loggerSettings;

        static IWebDriver webDriver;
        static ILogger logger;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
            SetupWebDriver();

            serviceProvider = CreateIOC();
            logger = serviceProvider.GetService<ILogger>();

            logger.Info(Operation.StartUp, "Application started");

            try
            {
                RunApplication();
            }
            catch (AggregateException ex)
            {
                LogInnerExceptions(ex);
                SaveCrashScreenshot();
            }
            catch (Exception ex)
            {
                logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
                SaveCrashScreenshot();
            }
            finally
            {
                if (!(webDriver is null))
                {
                    webDriver.Quit();
                }

                logger.Info(Operation.ShutDown, "Application stopped");
            }
        }

        static void RunApplication()
        {
            IProfileManager profile = serviceProvider.GetService<IProfileManager>();
            profile.LogIn();

            if (botSettings.RandomiseProfileName)
            {
                profile.SetRandomProfileName();
            }

            webDriver.Quit();
        }

        static IConfiguration LoadConfiguration()
        {
            botSettings = new BotSettings();
            debugSettings = new DebugSettings();
            loggerSettings = new NuciLoggerSettings();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(NuciLoggerSettings), loggerSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(loggerSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IWebDriver>(s => webDriver)
                .AddSingleton<IWebProcessor, WebProcessor>()
                .AddSingleton<ISteamProcessor, SteamProcessor>()
                .AddSingleton<IInfoGenerator, InfoGenerator>()
                .AddSingleton<IProfileManager, ProfileManager>()
                .BuildServiceProvider();
        }

        static void SetupWebDriver()
        {
            if (File.Exists("/usr/bin/geckodriver"))
            {
                webDriver = InitialiseFirefoxDriver();
            }
            else
            {
                webDriver = InitialiseChromeDriver();
            }
        }

        static IWebDriver InitialiseFirefoxDriver()
        {
            FirefoxOptions options = new FirefoxOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
			options.AddArgument("--disable-save-password-bubble");
            options.SetPreference("privacy.firstparty.isolate", false);

            if (debugSettings.IsHeadless)
            {
                options.AddArgument("--headless");
                options.SetPreference("permissions.default.image", 2);
            }

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            service.LogLevel = FirefoxDriverLogLevel.Error;

            IWebDriver driver = new FirefoxDriver(service, options, TimeSpan.FromSeconds(botSettings.PageLoadTimeout));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(botSettings.PageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }

        static IWebDriver InitialiseChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddExcludedArgument("--enable-logging");
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
			options.AddArgument("--disable-logging");

            if (debugSettings.IsHeadless)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1366,768");
                options.AddArgument("--start-maximized");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            ChromeDriver webDriver = new ChromeDriver(service, options, TimeSpan.FromSeconds(botSettings.PageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)webDriver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                webDriver.Quit();
                webDriver = new ChromeDriver(service, options);
            }

            webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(botSettings.PageLoadTimeout);
            webDriver.Manage().Window.Maximize();

            return webDriver;
        }

        static void LogInnerExceptions(AggregateException exception)
        {
            foreach (Exception innerException in exception.InnerExceptions)
            {
                AggregateException innerAggregateException = innerException as AggregateException;

                if (innerAggregateException is null)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                }
                else
                {
                    LogInnerExceptions(innerException as AggregateException);
                }
            }
        }

        static void SaveCrashScreenshot()
        {
            if (!debugSettings.IsCrashScreenshotEnabled)
            {
                return;
            }

            string directory = Path.GetDirectoryName(loggerSettings.LogFilePath);
            string filePath = Path.Combine(directory, debugSettings.CrashScreenshotFileName);

            ((ITakesScreenshot)webDriver)
                .GetScreenshot()
                .SaveAsFile(filePath);
        }
    }
}
