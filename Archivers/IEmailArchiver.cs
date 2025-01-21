namespace EmailParsing.Archivers;

/// <summary>
///     Интерфейс архиватора для обработки email файлов.
/// </summary>
internal interface IEmailArchiver
{
    /// <summary>
    ///     Архивирует указанную директорию в ZIP-архив.
    /// </summary>
    /// <param name="sourceDirectoryName">Путь к директории, которую необходимо архивировать.</param>
    /// <param name="zipFile">Путь к создаваемому ZIP-архиву.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию архивирования.</returns>
    Task Zip(string sourceDirectoryName, string zipFile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Распаковывает ZIP-архив в указанную директорию.
    /// </summary>
    /// <param name="sourceArchive">Путь к ZIP-архиву.</param>
    /// <param name="destinationDirectory">Путь к директории, в которую необходимо распаковать архив.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию распаковки.</returns>
    Task UnZip(string sourceArchive, string destinationDirectory, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Событие, возникающее при изменении прогресса операции архивирования/разархивирования.
    /// </summary>
    public event Action<long, long>? ProgressChanged;
}