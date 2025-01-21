using System.Text;
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
    
    /// <inheritdoc />
    public Task<EmailContent> ExtractEmailContentAsync(string emailPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(emailPath) || !File.Exists(emailPath))
                throw new FileNotFoundException("Указанный EML-файл не найден.");

            var emailMessage = Message.Load(new FileInfo(emailPath));

            cancellationToken.ThrowIfCancellationRequested();

            var subject = _helper.GetSubjectOrDefault(_helper.SanitizeString(emailMessage.Headers.Subject));
            var plainText = GetPlainTextBody(emailMessage, cancellationToken);
            var htmlText = GetHtmlBody(emailMessage, cancellationToken);
            var attachments = ExtractAttachments(emailMessage, cancellationToken);

            return new EmailContent
            {
                Subject = subject,
                PlainTextBody = plainText,
                HtmlBody = htmlText,
                Attachments = attachments
            };
        }, cancellationToken);
    }

    /// <summary>
    /// Извлекает текстовое содержимое сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Текстовое содержимое сообщения или пустую строку, если оно отсутствует.</returns>
    private static string GetPlainTextBody(Message message, CancellationToken cancellationToken)
    {
        var plainTextPart = message.FindFirstPlainTextVersion();
        cancellationToken.ThrowIfCancellationRequested();
        return plainTextPart?.GetBodyAsText() ?? string.Empty;
    }

    /// <summary>
    /// Извлекает HTML-содержимое сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>HTML-содержимое сообщения или пустую строку, если оно отсутствует.</returns>
    private static string GetHtmlBody(Message message, CancellationToken cancellationToken)
    {
        var htmlPart = message.FindFirstHtmlVersion();
        cancellationToken.ThrowIfCancellationRequested();
        return htmlPart?.GetBodyAsText() ?? string.Empty;
    }

    /// <summary>
    /// Извлекает вложения из сообщения.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список вложений.</returns>
    private List<Attachment> ExtractAttachments(Message message, CancellationToken cancellationToken)
    {
        var attachments = new List<Attachment>();

        foreach (var part in message.FindAllAttachments())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (part.FileName == null || part.Body == null) continue;

            attachments.Add(new Attachment
            {
                FileName = _helper.SanitizeString(part.FileName),
                ContentType = part.ContentType.MediaType,
                Content = part.Body
            });
        }

        return attachments;
    }
}