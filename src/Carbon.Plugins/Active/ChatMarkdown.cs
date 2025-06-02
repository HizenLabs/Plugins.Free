using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Facepunch;

namespace Carbon.Plugins;

[Info("Chat Markdown", "hizenxyz", "25.6.1968")]
[Description("Allows custom markdown formatting in chat messages, enhancing readability and style.")]
public class ChatMarkdown : CarbonPlugin
{
    #region Fields

    private static ChatMarkdown _instance;

    private const string chatPrefix = "chat.";
    private const string sayCommand = "say";
    private const string teamCommand = "team";
    private const char quote = '"';
    private const char escape = '\\';
    private const string escapeQuote = "\\\"";

    private static ArrayPool<char> _chatBuffer;

    #endregion

    #region Hooks

    private void Init()
    {
        _instance = this;
    }

    private void Unload()
    {
        _chatBuffer = null;
    }

    private object OnClientCommand(Network.Connection connection, string command)
    {
        if (connection.player is not BasePlayer)
        {
            return null;
        }

        var buffer = _chatBuffer.Rent(Settings.ChatBufferLength);
        try
        {
            if (!TryGetChatMessage(command, buffer, out var commandName, out var message))
            {
                return null;
            }

            Puts(commandName.Length);
            Puts(message.Length);

            message = Escape(message, buffer);

            var final = BuildCommand(commandName, message, buffer);

            Puts(final);

            var option = ConsoleSystem.Option.Server.FromConnection(connection).Quiet();
            ConsoleSystem.Run(option, final);

            return false;
        }
        finally
        {
            _chatBuffer.Return(buffer);
        }
    }

    #endregion

    #region Parsing

    /// <summary>
    /// Tries to extract the command name and message from the given command.
    /// </summary>
    /// <param name="command">The command to parse.</param>
    /// <param name="buffer">The buffer, which will be populated with the message if successful.</param>
    /// <param name="commandName">The extracted command name if successful.</param>
    /// <param name="message">The extracted message span if successful.</param>
    /// <returns>True if the command and message were successfully parsed, otherwise false.</returns>
    private static bool TryGetChatMessage(string command, char[] buffer, out ReadOnlySpan<char> commandName, out Span<char> message)
    {
        var spanCommand = command.AsSpan();

        message = Span<char>.Empty;
        if (!TryGetCommandName(spanCommand, out commandName))
            return false;

        // Validate start/end quotes
        int startIndex = command.IndexOf(quote);
        if (startIndex == -1) return false;
        int endIndex = command.LastIndexOf(quote);
        if (endIndex == -1) return false;

        // Validate existence of message between quotes
        if (endIndex <= startIndex || endIndex == startIndex + 1) return false;

        // Extract the message between quotes
        var rawMessage = spanCommand.Slice(startIndex + 1, endIndex - startIndex - 1);
        rawMessage.CopyTo(buffer);

        // Unescape quotes within the message
        message = buffer.AsSpan(0, rawMessage.Length);
        message = Unescape(message);

        // Validate that the message isn't just whitespace
        for (int i = 0; i < message.Length; i++)
        {
            if (!char.IsWhiteSpace(message[i]))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to extract the command name from the given command.
    /// </summary>
    /// /// <param name="command">The command to parse.</param>
    /// /// <param name="commandName">The extracted command name if successful.</param>
    /// /// <returns>True if the command name was successfully extracted, otherwise false.</returns>
    private static bool TryGetCommandName(ReadOnlySpan<char> command, out ReadOnlySpan<char> commandName)
    {
        commandName = ReadOnlySpan<char>.Empty;
        if (command.IsEmpty) return false;

        if (!command.StartsWith(chatPrefix.AsSpan(), StringComparison.OrdinalIgnoreCase)) return false;

        if (command[chatPrefix.Length..].StartsWith(sayCommand.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            commandName = command[..(chatPrefix.Length + sayCommand.Length)];
            return true;
        }
        else if (command[chatPrefix.Length..].StartsWith(teamCommand.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            commandName = command[..(chatPrefix.Length + teamCommand.Length)];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Escapes a message by adding escape characters before quotes.
    /// </summary>
    /// <param name="message">The message to escape.</param>
    /// param name="buffer">The underlying buffer that the message is wrapping. This is used to create a new Span.</param>
    /// <returns>A span containing the escaped message.</returns>
    // File: ChatEscape.cs
    internal static Span<char> Escape(Span<char> message, char[] buffer)
    {
        int escapeBufferLength = message.Length;
        int firstIndex = -1;
        for (int i = message.Length - 1; i >= 0; i--)
        {
            if (message[i] == quote || message[i] == escape)
            {
                escapeBufferLength++;
                firstIndex = i;
            }
        }

        // No quotes to escape, return original message
        if (firstIndex < 0)
        {
            return message;
        }

        if (escapeBufferLength > buffer.Length)
        {
            throw new IndexOutOfRangeException($"Required buffer length {escapeBufferLength} exceeds buffer length {buffer.Length}. Consider increasing the chat buffer length in the config.");
        }

        var escaped = buffer.AsSpan(0, escapeBufferLength);
        for (int i = message.Length - 1, offset = escaped.Length - message.Length; i >= 0 && i >= firstIndex - 1; i--)
        {
            if (message[i] == quote)
            {
                escaped[i + offset--] = quote;
                escaped[i + offset] = escape;
            }
            else if (message[i] == escape)
            {
                escaped[i + offset--] = escape;
                escaped[i + offset] = escape;
            }
            else
            {
                escaped[i + offset] = message[i];
            }
        }

        return escaped;
    }

    /// <summary>
    /// Unescapes a message by removing escape characters before quotes.
    /// </summary>
    /// <param name="message">The message to unescape.</param>
    /// <returns>A span containing the unescaped message.</returns>
    internal static Span<char> Unescape(Span<char> message)
    {
        int pos = 0;

        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == escape && i + 1 < message.Length && message[i + 1] == quote)
            {
                message[pos++] = quote;
                i++;
            }
            else if (message[i] == escape && i + 1 < message.Length && message[i + 1] == escape)
            {
                message[pos++] = escape;
                i++;
            }
            else
            {
                message[pos++] = message[i];
            }
        }

        return message[..pos];
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Builds a command string from the command name and message.
    /// </summary>
    /// /// <param name="commandName">The command name to use.</param>
    /// /// <param name="message">The message to include in the command.</param>
    /// /// <param name="buffer">The buffer to use for building the command string.</param>
    /// /// <returns>A string representing the command with the message included.</returns>
    private static string BuildCommand(ReadOnlySpan<char> commandName, Span<char> message, char[] buffer)
    {
        int totalSize = commandName.Length  + message.Length + 3; // 3 for 2 quotes + 1 space
        var buildSpan = buffer.AsSpan(0, totalSize);

        // build backwards since message is using the buffer
        buildSpan[^1] = quote;
        message.CopyTo(buildSpan[(commandName.Length + 2)..]);
        buildSpan[commandName.Length + 1] = quote;
        buildSpan[commandName.Length] = ' ';
        commandName.CopyTo(buildSpan);

        return buildSpan.ToString();
    }

    #endregion

    #region Settings

    #region Config Overrides

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    protected override void LoadConfig()
    {
        base.LoadConfig();

        Settings.Init(this);
    }

    /// <summary>
    /// Loads the default configuration.
    /// </summary>
    protected override void LoadDefaultConfig()
    {
        base.LoadDefaultConfig();

        Settings.InitDefault(this);
    }

    /// <summary>
    /// Saves the configuration.
    /// </summary>
    protected override void SaveConfig()
    {
        base.SaveConfig();

        Settings.Save(this);

        if (Settings.ChatBufferLength < SettingDefaults.ChatBufferLength)
        {
            Puts($"Warning! Chat buffer length is set to {Settings.ChatBufferLength}, which is less than the default value of {SettingDefaults.ChatBufferLength}. This may cause exceptions with longer messages.");
        }
        else if (Settings.ChatBufferLength > SettingDefaults.ChatBufferLength * 20)
        {
            Puts($"Warning! Chat buffer length is set to {Settings.ChatBufferLength}, which is significantly higher than the default value of {SettingDefaults.ChatBufferLength}. Extremely high values may cause perf issues or crashes.");
        }

        _chatBuffer = new(Settings.ChatBufferLength);
    }

    #endregion

    /// <summary>
    /// Handles the settings and defaults.
    /// </summary>
    /// <summary>
    /// Handles the settings and defaults.
    /// </summary>
    private static class Settings
    {
        private static ChatMarkdownConfig _config;

        public static int ChatBufferLength => _config.ChatBufferLength;

        public static List<ChatMarkdownConfig.DefaultMarkdown> Defaults => _config.Defaults;

        public static List<ChatMarkdownConfig.CustomMarkdown> CustomMarkdowns => _config.CustomMarkdowns;

        /// <summary>
        /// Initializes the settings by reading the configuration from the file or creating a default one if it fails.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Init(ChatMarkdown plugin)
        {
            _config = ReadConfigOrCreateDefault(plugin);

            plugin.SaveConfig();
        }

        /// <summary>
        /// Initializes the settings with default values and saves them to the file.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void InitDefault(ChatMarkdown plugin)
        {
            _config = CreateDefault();

            plugin.SaveConfig();
        }

        /// <summary>
        /// Reads the configuration from the file or creates a default one if it fails.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        /// <returns>The configuration object.</returns>
        private static ChatMarkdownConfig ReadConfigOrCreateDefault(ChatMarkdown plugin)
        {
            try
            {
                return plugin.Config.ReadObject<ChatMarkdownConfig>()
                    ?? throw new Exception("Config is null");
            }
            catch
            {
                return CreateDefault();
            }
        }

        /// <summary>
        /// Creates a default instance.
        /// </summary>
        /// <returns>The default instance.</returns>
        private static ChatMarkdownConfig CreateDefault()
        {
            return new();
        }

        /// <summary>
        /// Saves the configuration to the file.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        public static void Save(ChatMarkdown plugin)
        {
            plugin.Config.WriteObject(_config, true);
        }
    }

    /// <summary>
    /// The default settings for the plugin.
    /// </summary>
    private static class SettingDefaults
    {
        public const int ChatBufferLength = 256;

        public static class CustomMarkdown
        {
            public static readonly string TagRegex = "";
            public static readonly string Replacement = "";
            public static readonly string Permission = "chatmarkdown.custom";
            public static readonly bool Enabled = true;
        }
    }

    /// <summary>
    /// Represents the configuration.
    /// </summary>
    private class ChatMarkdownConfig
    {
        [JsonProperty("Chat Buffer Length (Note: This should only be increased if you have a longer chat length setting)")]
        public int ChatBufferLength { get; set; } = SettingDefaults.ChatBufferLength;

        public List<DefaultMarkdown> Defaults { get; set; } = new()
        {
            new("[b]$1[/b]", "<b>$1</b>"),
            new("[color=$1]$2[/color]", "<color=\"#$1\">$2</color>")
        };

        public List<CustomMarkdown> CustomMarkdowns { get; set; } = new()
        {
        };

        public class DefaultMarkdown : MarkdownBase
        {
            public string TagRegex { get; }

            public string Replacement { get; }

            public DefaultMarkdown(string tagRegex, string replacement)
            {
                TagRegex = tagRegex;
                Replacement = replacement;
            }
        }

        public class CustomMarkdown : MarkdownBase
        {
            public string TagRegex { get; set; } = SettingDefaults.CustomMarkdown.TagRegex;

            public string Replacement { get; set; } = SettingDefaults.CustomMarkdown.Replacement;
        }

        public class MarkdownBase
        {
            public string Permission { get; set; } = SettingDefaults.CustomMarkdown.Permission;

            public bool Enabled { get; set; } = SettingDefaults.CustomMarkdown.Enabled;
        }
    }

    #endregion
}
