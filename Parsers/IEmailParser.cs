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
    ///     Путь к временной директории для хранения извлеченных вложений.
    /// </summary>
    string TempExtractedAttachmentsDir { get; set; }

    /// <summary>
    ///    Прогресс обработки файлов.
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
    ///     Обрабатывает EML-файл.
    /// </summary>
    /// <param name="sourcePath">Путь к EML-файлу.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseEmlFileAsync(string sourcePath);

    /// <summary>
    ///     Обрабатывает архив.
    /// </summary>
    /// <param name="sourcePath">Путь к архиву.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseArchiveAsync(string sourcePath);

    /// <summary>
    ///     Событие прогресса.
    /// </summary>
    public event EventHandler<int>? ProgressChanged;
}