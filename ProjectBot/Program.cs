﻿using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ProjectBot
{
    class Program
    {
        private DiscordSocketClient socketClient;
        /// <summary>
        /// Точка входа программы.
        /// </summary>
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            new Program().MainAsync().GetAwaiter().GetResult();
        }
        /// <summary>
        /// Основной метод запуска бота
        /// </summary>
        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                socketClient = services.GetRequiredService<DiscordSocketClient>();
                socketClient.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                var values = ReadJSON(".env");
                string token = values["token"];
                await socketClient.LoginAsync(TokenType.Bot, token);
                await socketClient.StartAsync();

                await services.GetRequiredService<CommandServiceHandler>().InitializeAsync();
                await Task.Delay(Timeout.Infinite);
            }
        }
        /// <summary>
        /// Метод для логирования клиента.
        /// </summary>
        /// <param name="logMsg">Сообщения логов.</param>
        private Task LogAsync(LogMessage logMsg)
        {
            Console.WriteLine(logMsg);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Метод настройки сервисов программы.
        /// </summary>
        /// <returns>Используемые программой сервисы.</returns>
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection().AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandServiceHandler>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
        /// <summary>
        /// Метод для считывания JSON-файла и получения значения. Используется для чтения токена.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ReadJSON(string path)
        {
            Dictionary<string, string> ret = null;
            StreamReader stream = new StreamReader(path);
            try
            {
                string text = stream.ReadToEnd();
                object t = JsonConvert.DeserializeObject(text, typeof(Dictionary<string, string>));
                ret = t as Dictionary<string, string>;
            }
            finally
            {
                stream.Close();
            }
            return ret;
        }
    }
}
