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
            webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(debugSettings.IsDebugMode, botSettings.PageLoadTimeout);

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

            if (botSettings.RandomiseProfileIdentifier)
            {
                profile.SetRandomProfileIdentifier();
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
