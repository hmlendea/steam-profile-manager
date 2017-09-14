﻿using System;
using System.Threading.Tasks;

namespace SteamProfileManager
{
    class Program
    {
        static SteamClient client;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("At least two arguments must be sent to this application (Username and Password)");
            }

            string username = args[0];
            string password = args[1];
            string authCode = null;
            string webSteamLogin = null;
            string webSessionId = null;

            if (args.Length == 3)
            {
                authCode = args[2];
            }

            if (args.Length == 4)
            {
                webSteamLogin = args[2];
                webSessionId = args[3];
            }
            else if (args.Length == 5)
            {
                webSteamLogin = args[3];
                webSessionId = args[4];
            }

            client = new SteamClient();

            RegisterEvents();

            Console.WriteLine("Connecting to Steam");

            if (!string.IsNullOrEmpty(authCode) && string.IsNullOrEmpty(webSteamLogin))
            {
                client.LogIn(username, password, authCode);
            }
            else if (string.IsNullOrEmpty(authCode) && !string.IsNullOrEmpty(webSteamLogin))
            {
                client.LogIn(username, password, webSteamLogin, webSessionId);
            }
            else if (!string.IsNullOrEmpty(authCode) && !string.IsNullOrEmpty(webSteamLogin))
            {
                client.LogIn(username, password, authCode, webSteamLogin, webSessionId);
            }
            else
            {
                client.LogIn(username, password);
            }

            Console.ReadKey();
        }

        static void RegisterEvents()
        {
            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
            client.LoggedIn += OnClientLoggedIn;
            client.LoggedOut += OnClientLoggedOut;
        }

        static void OnClientConnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Connected to Steam");
        }

        static void OnClientDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from Steam");
        }

        static void OnClientLoggedIn(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged in to Steam. (SteamID: {client.CurrentUser.AccountId})");
        }

        static void OnClientLoggedOut(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged out of Steam: {client.CurrentUser.AccountId}");
        }
    }
}