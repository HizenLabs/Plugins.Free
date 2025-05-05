namespace HizenLabs.Extensions.ObjectSerializer.Enums;

/// <summary>
/// Represents the format of the data.
/// </summary>
public enum DataFormat
{
    /// <summary>
    /// Binary format.
    /// </summary>
    Binary,
    /// <summary>
    /// Binary format with Gzip compression.
    /// </summary>
    Gzip,
    /// <summary>
    /// JSON format.
    /// </summary>
    Json,
    /// <summary>
    /// JSON format with indentation.
    /// </summary>
    JsonIndented,
}
