using System.Collections.Generic;

namespace HizenLabs.Extensions.UserPreference.UI;

/// <summary>
/// Holds labels for moda options instead of creating hundreds of string[] { "option 1", "option 2" }.
/// </summary>
internal static class ModalOptions
{
    private static readonly Dictionary<string, string[]> _options = new();

    public static string[] Get(string option1, string option2)
    {
        var key = $"2|o1:{option1}|o2:{option2}";

        if (!_options.TryGetValue(key, out var options))
        {
            _options[key] = new[] { option1, option2 };
        }

        return options;
    }

    public static string[] Get(string option1, string option2, string option3)
    {
        var key = $"3|o1:{option1}|o2:{option2}|o3:{option3}";

        if (!_options.TryGetValue(key, out var options))
        {
            _options[key] = new[] { option1, option2, option3 };
        }

        return options;
    }
}
