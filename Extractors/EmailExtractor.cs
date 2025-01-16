using EmailParsing.Helpers;
using EmailParsing.Models;
using OpenPop.Mime;

namespace EmailParsing.Extractors;

/// <summary>
///     Извлекатель содержимого электронной почты из файлов EML.
/// </summary>
internal class EmailExtractor : IEmailExtractor
{
    private readonly IFileSystemHelper _helper;

    public EmailExtractor(IFileSystemHelper helper)
    {
        _helper = helper;
    }
    /// <summary>
    /// Асинхронно извлекает содержимое электронной почты из файла EML.
    /// </summary>
    /// <param name="emailPath">Путь к файлу EML.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Результатом задачи является объект EmailContent, содержащий извлеченное содержимое.</returns>
    /// <exception cref="FileNotFoundException">Если указанный EML-файл не найден.</exception>
    public Task<EmailContent> ExtractEmailContentAsync(string emailPath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(emailPath) || !File.Exists(emailPath))
                throw new FileNotFoundException("Указанный EML-файл не найден.");

            var emailMessage = Message.Load(new FileInfo(emailPath));

            var subject =
                _helper.GetSubjectOrDefault(_helper.SanitizeString(emailMessage.Headers.Subject));
            var plainText = GetPlainTextBody(emailMessage);
            var htmlText = GetHtmlBody(emailMessage);
            var attachments = ExtractAttachments(emailMessage);

            return new EmailContent
            {
                Subject = subject,
                PlainTextBody = plainText,
                HtmlBody = htmlText,
                Attachments = attachments
            };
        });
    }

    /// <summary>
    /// Извлекает текстовое содержимое сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <returns>Текстовое содержимое сообщения или пустую строку, если оно отсутствует.</returns>
    private static string GetPlainTextBody(Message message)
    {
        var plainTextPart = message.FindFirstPlainTextVersion();
        return plainTextPart?.GetBodyAsText() ?? string.Empty;
    }

    /// <summary>
    /// Извлекает HTML-содержимое сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <returns>HTML-содержимое сообщения или пустую строку, если оно отсутствует.</returns>
    private static string GetHtmlBody(Message message)
    {
        var htmlPart = message.FindFirstHtmlVersion();
        return htmlPart?.GetBodyAsText() ?? string.Empty;
    }

    /// <summary>
    /// Извлекает вложения из сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <returns>Список вложений.</returns>
    private static List<Attachment> ExtractAttachments(Message message)
    {
        var attachments = new List<Attachment>();

        foreach (var part in message.FindAllAttachments())
        {
            if (part.FileName == null || part.Body == null) continue;

            attachments.Add(new Attachment
            {
                FileName = part.FileName,
                ContentType = part.ContentType.MediaType,
                Content = part.Body
            });
        }

        return attachments;
    }
}