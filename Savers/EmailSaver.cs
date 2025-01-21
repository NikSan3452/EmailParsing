using System.Text;
using EmailParsing.Helpers;
using EmailParsing.Models;

namespace EmailParsing.Savers;

/// <summary>
///     Сохраняет содержимое электронной почты в файловую систему.
/// </summary>
internal class EmailSaver : IEmailSaver
{
    private readonly IFileSystemHelper _helper;

    public EmailSaver(IFileSystemHelper helper)
    {
        _helper = helper;
    }
    
    /// <inheritdoc />
    public async Task SaveEmailContentAsync(EmailContent content, string outputDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(content.Subject)) return;

        var emailDirectory = Path.Combine(outputDirectory, content.Subject);
        if (OperatingSystem.IsWindows()) emailDirectory = _helper.ConvertToLongPath(emailDirectory);
        emailDirectory = _helper.CreateDirectory(emailDirectory);

        await SavePlainTextBody(content, emailDirectory, cancellationToken);
        await SaveHtmlBody(content, emailDirectory, cancellationToken);
        await SaveAttachments(content, emailDirectory, cancellationToken);
    }

    /// <summary>
    /// Асинхронно сохраняет текстовое содержимое письма в файл.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SavePlainTextBody(EmailContent content, string emailDirectory, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(content.PlainTextBody))
        {
            var filePath = Path.Combine(emailDirectory, $"{content.Subject}.txt");
            if (OperatingSystem.IsWindows()) filePath = _helper.ConvertToLongPath(filePath);
            await File.WriteAllTextAsync(filePath, content.PlainTextBody, Encoding.UTF8, cancellationToken);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет HTML-содержимое письма в файл.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SaveHtmlBody(EmailContent content, string emailDirectory, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(content.HtmlBody))
        {
            var filePath = Path.Combine(emailDirectory, $"{content.Subject}.html");
            if (OperatingSystem.IsWindows()) filePath = _helper.ConvertToLongPath(filePath);
            await File.WriteAllTextAsync(filePath, content.HtmlBody, Encoding.UTF8, cancellationToken);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет вложения письма в файлы.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SaveAttachments(EmailContent content, string emailDirectory, CancellationToken cancellationToken)
    {
        if (content.Attachments != null)
            foreach (var attachment in content.Attachments)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (attachment.FileName == null) continue;

                var filePath = Path.Combine(emailDirectory, attachment.FileName);
                if (OperatingSystem.IsWindows()) filePath = _helper.ConvertToLongPath(filePath);
                if (attachment.Content != null) await File.WriteAllBytesAsync(filePath, attachment.Content, cancellationToken);
            }
    }
}