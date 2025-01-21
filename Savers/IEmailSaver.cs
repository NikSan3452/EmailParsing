using EmailParsing.Models;

namespace EmailParsing.Savers;

/// <summary>
/// Интерфейс для сохранения содержимого электронной почты.
/// </summary>
internal interface IEmailSaver
{
    /// <summary>
    /// Асинхронно сохраняет содержимое электронной почты в указанную директорию.
    /// </summary>
    /// <param name="content">Содержимое электронной почты для сохранения.</param>
    /// <param name="outputDirectory">Путь к директории, в которую необходимо сохранить содержимое.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения.</returns>
    Task SaveEmailContentAsync(EmailContent content, string outputDirectory, CancellationToken cancellationToken = default);
}