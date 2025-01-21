using EmailParsing.Models;

namespace EmailParsing.Extractors;

/// <summary>
/// Интерфейс извлекателя содержимого электронной почты.
/// </summary>
internal interface IEmailExtractor
{
    /// <summary>
    /// Асинхронно извлекает содержимое электронной почты из файла EML.
    /// </summary>
    /// <param name="emailPath">Путь к файлу EML.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Результатом задачи является объект EmailContent, содержащий извлеченное содержимое.</returns>
    /// <exception cref="FileNotFoundException">Если указанный EML-файл не найден.</exception>
    Task<EmailContent> ExtractEmailContentAsync(string emailPath, CancellationToken cancellationToken = default);
}