using System.Text;
using EmailParsing.Helpers;
using EmailParsing.Models;

namespace EmailParsing.Savers;

/// <summary>
/// Сохраняет содержимое электронной почты в файловую систему.
/// </summary>
/// <param name="helper">Вспомогательный класс для работы с файловой системой.</param>
internal class EmailSaver(IFileSystemHelper helper) : IEmailSaver
{
    /// <summary>
    /// Асинхронно сохраняет содержимое электронной почты в указанную директорию.
    /// </summary>
    /// <param name="content">Содержимое электронной почты для сохранения.</param>
    /// <param name="outputDirectory">Путь к директории, в которую необходимо сохранить содержимое.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    public async Task SaveEmailContentAsync(EmailContent content, string outputDirectory)
    {
        if (string.IsNullOrEmpty(content.Subject)) return;

        var emailDirectory = Path.Combine(outputDirectory, content.Subject);
        if (OperatingSystem.IsWindows()) emailDirectory = helper.ConvertToLongPath(emailDirectory);
        emailDirectory = helper.CreateDirectory(emailDirectory);

        await SavePlainTextBody(content, emailDirectory);
        await SaveHtmlBody(content, emailDirectory);
        await SaveAttachments(content, emailDirectory);
    }

    /// <summary>
    /// Асинхронно сохраняет текстовое содержимое письма в файл.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SavePlainTextBody(EmailContent content, string emailDirectory)
    {
        if (!string.IsNullOrEmpty(content.PlainTextBody))
        {
            var filePath = Path.Combine(emailDirectory, $"{content.Subject}.txt");
            if (OperatingSystem.IsWindows()) filePath = helper.ConvertToLongPath(filePath);
            await File.WriteAllTextAsync(filePath, content.PlainTextBody, Encoding.UTF8);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет HTML-содержимое письма в файл.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SaveHtmlBody(EmailContent content, string emailDirectory)
    {
        if (!string.IsNullOrEmpty(content.HtmlBody))
        {
            var filePath = Path.Combine(emailDirectory, $"{content.Subject}.html");
            if (OperatingSystem.IsWindows()) filePath = helper.ConvertToLongPath(filePath);
            await File.WriteAllTextAsync(filePath, content.HtmlBody, Encoding.UTF8);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет вложения письма в файлы.
    /// </summary>
    /// <param name="content">Содержимое электронной почты.</param>
    /// <param name="emailDirectory">Путь к директории письма.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    private async Task SaveAttachments(EmailContent content, string emailDirectory)
    {
        if (content.Attachments != null)
            foreach (var attachment in content.Attachments)
            {
                if (attachment.FileName == null) continue;

                var filePath = Path.Combine(emailDirectory, attachment.FileName);
                if (OperatingSystem.IsWindows()) filePath = helper.ConvertToLongPath(filePath);
                if (attachment.Content != null) await File.WriteAllBytesAsync(filePath, attachment.Content);
            }
    }
}