using System;
using System.Security.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciWeb;
using NuciWeb.Steam;

using SteamProfileManager.Configuration;

namespace SteamProfileManager
{
    public sealed class Program
    {
        static BotSettings botSettings;
        static DebugSettings debugSettings;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();

            serviceProvider = CreateIOC();

            try
            {
                RunApplication();
            }
            catch (Exception ex)
            {
            }
        }

        static void RunApplication()
        {

        }
        
        static IConfiguration LoadConfiguration()
        {
            botSettings = new BotSettings();
            debugSettings = new DebugSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(DebugSettings), debugSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(debugSettings)
                .BuildServiceProvider();
        }
    }
}
