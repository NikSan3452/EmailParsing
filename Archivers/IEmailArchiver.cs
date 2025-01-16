namespace EmailParsing.Archivers;

/// <summary>
/// Интерфейс архиватора для обработки email файлов.
/// </summary>
internal interface IEmailArchiver
{
    /// <summary>
    /// Архивирует указанную директорию в ZIP-архив.
    /// </summary>
    /// <param name="sourceDirectoryName">Путь к директории, которую необходимо архивировать.</param>
    /// <param name="zipFile">Путь к создаваемому ZIP-архиву.</param>
    /// <returns>Задача, представляющая асинхронную операцию архивирования.</returns>
    Task Zip(string sourceDirectoryName, string zipFile);

    /// <summary>
    /// Распаковывает ZIP-архив в указанную директорию.
    /// </summary>
    /// <param name="zipFile">Путь к ZIP-архиву.</param>
    /// <param name="destinationDirectory">Путь к директории, в которую необходимо распаковать архив.</param>
    /// <returns>Задача, представляющая асинхронную операцию распаковки.</returns>
    Task UnZip(string zipFile, string destinationDirectory);
}