namespace EmailParsing.Models;

/// <summary>
/// Модель содержимого электронной почты.
/// </summary>
internal class EmailContent
{
    /// <summary>
    /// Тема письма.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Текстовое содержимое письма.
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// HTML-содержимое письма.
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Список вложений.
    /// </summary>
    public List<Attachment>? Attachments { get; set; }
}