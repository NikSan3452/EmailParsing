namespace EmailParsing.Models;

/// <summary>
/// Модель вложения электронной почты.
/// </summary>
internal class Attachment
{
    /// <summary>
    /// Имя файла вложения.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Тип содержимого вложения (MIME-тип).
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Содержимое вложения в виде массива байтов.
    /// </summary>
    public byte[]? Content { get; set; }
}