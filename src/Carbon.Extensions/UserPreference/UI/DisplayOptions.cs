using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.UI;

internal static class DisplayOptions
{
    private static readonly Dictionary<string, string[]> _options = new();

    public static string[] Get(string optionLight, string optionDark)
    {
        var key = $"{optionLight}_{optionDark}";

        if (!_options.TryGetValue(key, out var options))
        {
            _options[key] = new[] { optionLight, optionDark };
        }

        return options;
    }
}
