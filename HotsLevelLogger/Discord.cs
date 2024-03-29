﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Manages Discord logging.
    /// </summary>
    internal static class Discord
    {
        private static bool ready = false;
        private static bool initialized = false;
        private static ulong channelId;
        private static SocketTextChannel channel;
        private static DiscordSocketClient client;

#if DEBUG
        private const string rickroll = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        private const string icon = "https://cdn.discordapp.com/attachments/538805766719668284/847167411400343562/Master512.png";
        private const string horizontalLine = "\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_";
#endif

        /// <summary>
        /// Initializes Discord logger.
        /// </summary>
        /// <param name="token">Discord bot token.</param>
        /// <param name="channelId">ID of the channel where messages will be sent to.</param>
        internal static async Task Init(string token, ulong channelId)
        {
            // Logger can only be initialized once
            if (initialized)
            {
                return;
            }
            initialized = true;

            if (channelId == 0 || token == null || token == "")
            {
                initialized = false;
                return;
            }
            Discord.channelId = channelId;

            // Start bot
            DiscordSocketConfig socketConfig = new DiscordSocketConfig()
            {
                AlwaysDownloadDefaultStickers = false,
                AlwaysDownloadUsers = false,
                AlwaysResolveStickers = false,

                LogGatewayIntentWarnings = false
                // Alternatively, the following line works too instead of ignoring the warnings, but then the bot takes longer from startup until ready.
                //GatewayIntents = GatewayIntents.None
            };
            client = new DiscordSocketClient(socketConfig);
            client.Log += DiscordLog;
            await client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);

            // Prevent application from being closed
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets called when the bot is ready.
        /// Caches Discord channel.
        /// </summary>
        private static void OnReady()
        {
            channel = client.GetChannel(channelId) as SocketTextChannel;
            _ = client.SetGameAsync($"Loggers of the Storm");
            ready = true;
        }

        /// <summary>
        /// Determines whether a connection to Discord has been established and logging can be used.
        /// </summary>
        internal static bool IsReady()
        {
            return ready;
        }

        /// <summary>
        /// Sends a message to the discord channel.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        internal static void Log(string message)
        {
            _ = channel.SendMessageAsync(message);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Sends a message with an attached file to the discord channel.
        /// </summary>
        /// <param name="filepath">Path of the file to be sent.</param>
        /// <param name="message">Message to be sent.</param>
        internal static void LogFile(string filepath, string message = "")
        {
            _ = channel.SendFileAsync(filepath, message);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Sends a message with the attached files to the discord channel.
        /// </summary>
        /// <param name="attachments">List of the Files to be sent.</param>
        /// <param name="message">Message to be sent.</param>
        internal static void LogFiles(IEnumerable<FileAttachment> attachments, string message = "")
        {
            _ = channel.SendFilesAsync(attachments, message);
        }

#if DEBUG
        /// <summary>
        /// Sends a message with an attached embed to the discord channel. (Not implemented properly)
        /// </summary>
        internal static void LogEmbed()
        {
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Hots Level Logger",
                Url = rickroll,
                IconUrl = null
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"Amazing Footer",
                IconUrl = null
            };
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            foreach (int i in new int[] { 1, 2, 4, 8, 13 })
            {
                EmbedFieldBuilder accountField = new EmbedFieldBuilder()
                {
                    Name = $"Name {i}",
                    IsInline = true,
                    Value = $"Value {i}"
                };
                fields.Add(accountField);
            }
            EmbedFieldBuilder footerfield = new EmbedFieldBuilder()
            {
                Name = horizontalLine,
                IsInline = false,
                Value = $"Amazing Footer Field"
            };
            fields.Add(footerfield);
            Embed embed = new EmbedBuilder()
            {
                Author = author,
                ThumbnailUrl = icon,
                Url = rickroll,
                Title = "Embed!",
                Description = $"This is an amazing embed.",
                Fields = fields,
                ImageUrl = null,
                Footer = footer,
                Timestamp = DateTime.Now,
                Color = Color.Magenta
            }.Build();
            _ = channel.SendMessageAsync("Embed Message", false, embed);
        }
#endif

        /// <summary>
        /// Handles logs initialized by the Discord API.
        /// </summary>
        /// <param name="message">Discord API log message.</param>
        private static Task DiscordLog(LogMessage message)
        {
            if (!(message.Severity == LogSeverity.Verbose || message.Severity == LogSeverity.Debug))
            {
                ready = false;
            }
            if (message.Severity == LogSeverity.Info && message.Source.ToLower() == "gateway")
            {
                if (message.Message.ToLower() == "ready") // TODO: Eventuell ist message.Message == "connected" auch möglich.
                {
                    ready = true;
                    if (channel == null)
                    {
                        OnReady();
                    }
                }
            }

            string messageString = $"\r\nDiscord Message:\r\n{message.Message}";
            lock (Program.consoleLockObject)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(messageString);
                Console.ForegroundColor = previousColor;
            }
            return Task.CompletedTask;
        }
    }
}
