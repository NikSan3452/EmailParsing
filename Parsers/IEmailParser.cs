namespace EmailParsing.Parsers;

/// <summary>
///     Интерфейс парсера электронной почты.
/// </summary>
public interface IEmailParser
{
    /// <summary>
    ///     Путь к выходному ZIP-архиву.
    /// </summary>
    string ZipFilePath { get; set; }

    /// <summary>
    ///     Путь к директории с временными файлами.
    /// </summary>
    public string TempDirectory { get; set; }

    /// <summary>
    ///     Путь к временной директории для хранения извлеченных вложений.
    /// </summary>
    string TempExtractedAttachmentsDir { get; set; }

    /// <summary>
    ///     Путь к временной директории для распаковки писем из архива.
    /// </summary>
    public string TempUnzippedEmlDir { get; set; }

    /// <summary>
    ///     Имя директории парсера электронной почты, которая будет создана во временной директории.
    ///     По умолчанию EmailParser.
    /// </summary>
    public string EmailParserDirectory { get; set; }

    /// <summary>
    ///     Прогресс обработки файлов.
    /// </summary>
    public int Progress { get; }

    /// <summary>
    ///     Флаг указывающий удалить ли исходный файл.
    /// </summary>
    public bool DeleteSourceFile { get; set; }

    /// <summary>
    ///     Текущая выполняемая операция.
    /// </summary>
    public OperationType CurrentOperation { get; }

    /// <summary>
    ///     Путь к исходному файлу.
    /// </summary>
    public string SourceFilePath { get; set; }

    /// <summary>
    ///     Обрабатывает EML-файл.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseEmlFileAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Обрабатывает архив.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseArchiveAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Очищает временные директории и удаляет исходный файл.
    /// </summary>
    Task CleanupAsync();

    /// <summary>
    ///     Событие прогресса.
    /// </summary>
    public event EventHandler<int>? ProgressChanged;
}