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
    ///     Общий прогресс обработки файлов.
    /// </summary>
    public int TotalProgress { get; }

    /// <summary>
    ///     Флаг указывающий удалить ли исходный файл.
    /// </summary>
    public bool DeleteSourceFile { get; set; }

    /// <summary>
    ///     Загружает и обрабатывает EML-файл.
    /// </summary>
    /// <param name="sourcePath">Путь к EML-файлу.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseEmlFile(string sourcePath);

    /// <summary>
    ///     Загружает и обрабатывает архив.
    /// </summary>
    /// <param name="sourcePath">Путь к архиву.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task ParseArchive(string sourcePath);

    /// <summary>
    ///     Событие прогресса.
    /// </summary>
    public event EventHandler<int>? ProgressChanged;
}