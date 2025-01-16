using System.Text;
using EmailParsing.Archivers;
using EmailParsing.Extractors;
using EmailParsing.Helpers;
using EmailParsing.Savers;
using EmailParsing.Scanners;

namespace EmailParsing.Parsers;

/// <summary>
///     Парсер электронной почты, обрабатывающий EML-файлы и архивы.
/// </summary>
public class EmailParser : IEmailParser
{
    private readonly IEmailArchiver _archiver;
    private readonly IEmailExtractor _extractor;
    private readonly IFileSystemHelper _helper;
    private readonly IEmailSaver _saver;
    private readonly IEmailScanner _scanner;

    /// <summary>
    ///     Конструктор класса EmailParser.
    /// </summary>
    public EmailParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _helper = new FileSystemHelper();
        _saver = new EmailSaver(_helper);
        _extractor = new EmailExtractor(_helper);
        _scanner = new EmailScanner();
        _archiver = new EmailArchiver();

        TempDirectory = Path.GetTempPath();
        EmailParserDirectory = "EmailParser";
        ZipFilePath = Path.Combine(_helper.GetDesktopPath(), $"{Guid.NewGuid()}.zip");
    }

    /// <summary>
    ///     Путь к временной директории.
    /// </summary>
    private string TempDirectory { get; }

    /// <summary>
    ///     Имя директории парсера электронной почты.
    /// </summary>
    private string EmailParserDirectory { get; }

    /// <summary>
    ///     Путь к временной директории для распаковки писем из архива.
    /// </summary>
    private string TempUnzippedEmlDir { get; set; } = string.Empty;

    /// <summary>
    ///     Путь к выходному ZIP-архиву.
    /// </summary>
    public string ZipFilePath { get; set; }

    /// <summary>
    ///     Путь к временной директории для хранения извлеченных вложений.
    /// </summary>
    public string TempExtractedAttachmentsDir { get; set; } = string.Empty;

    /// <summary>
    ///     Общий прогресс обработки файлов.
    /// </summary>
    public int TotalProgress { get; private set; }

    /// <summary>
    ///     Флаг указывающий удалить ли исходный файл.
    /// </summary>
    public bool DeleteSourceFile { get; set; } = false;

    /// <summary>
    ///     Событие прогресса.
    /// </summary>
    public event EventHandler<int>? ProgressChanged;

    /// <summary>
    ///     Обрабатывает архив.
    /// </summary>
    /// <param name="sourcePath">Путь к архиву.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ParseArchive(string sourcePath)
    {
        InitializeTempUnzippedEmlDir();
        InitializeExtractedAttachmentsDir();

        await UnpackArchive(sourcePath);
        var emailFiles = ScanForEmlFiles();

        if (emailFiles.Count == 0) return;
        await ExtractAttachments(emailFiles);

        await PrepareOutputArchive();
        Cleanup(sourcePath);
    }

    /// <summary>
    ///     Обрабатывает EML-файл.
    /// </summary>
    /// <param name="sourcePath">Путь к EML-файлу.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ParseEmlFile(string sourcePath)
    {
        InitializeExtractedAttachmentsDir();

        var content = await _extractor.ExtractEmailContentAsync(sourcePath);

        await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir);

        await _archiver.Zip(TempExtractedAttachmentsDir, ZipFilePath);

        Cleanup(sourcePath);
    }

    /// <summary>
    ///     Обработчик события прогресса.
    /// </summary>
    /// <param name="progress">Прогресс.</param>
    protected virtual void OnProgressChanged(int progress)
    {
        ProgressChanged?.Invoke(this, progress);
    }

    /// <summary>
    ///     Распаковывает архив.
    /// </summary>
    /// <param name="sourcePath">Путь к архиву.</param>
    private async Task UnpackArchive(string sourcePath)
    {
        await _archiver.UnZip(sourcePath, TempUnzippedEmlDir);
    }

    /// <summary>
    ///     Сканирует директорию на наличие EML-файлов.
    /// </summary>
    /// <returns>Список путей к EML-файлам.</returns>
    private List<string> ScanForEmlFiles()
    {
        var emailFiles = _scanner.ScanForEmailFiles(TempUnzippedEmlDir);
        return emailFiles;
    }

    /// <summary>
    ///     Извлекает вложения из EML-файлов.
    /// </summary>
    /// <param name="emailFiles">Список путей к EML-файлам.</param>
    private async Task ExtractAttachments(List<string> emailFiles)
    {
        TotalProgress = 0;
        var processedFiles = 0;
        var totalFiles = emailFiles.Count;

        var contentList = emailFiles.Select(email => _extractor.ExtractEmailContentAsync(email)).ToList();

        foreach (var contentTask in contentList)
        {
            var content = await contentTask;
            await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir);
            processedFiles++;
            TotalProgress = (int)((double)processedFiles / totalFiles * 100);
            OnProgressChanged(TotalProgress);
        }
    }

    /// <summary>
    ///     Подготавливает выходной архив.
    /// </summary>
    private async Task PrepareOutputArchive()
    {
        await _archiver.Zip(TempExtractedAttachmentsDir, ZipFilePath);
    }

    /// <summary>
    ///     Очищает временные директории и удаляет исходный файл.
    /// </summary>
    /// <param name="sourcePath">Путь к исходному файлу.</param>
    private void Cleanup(string sourcePath)
    {
        if (DeleteSourceFile) _helper.DeleteFiles(new List<string> { sourcePath });
        _helper.DeleteDirectory(TempUnzippedEmlDir);
        _helper.DeleteDirectory(TempExtractedAttachmentsDir);
    }

    /// <summary>
    ///     Инициализирует путь к временной директории для хранения извлеченных вложений.
    /// </summary>
    private void InitializeExtractedAttachmentsDir()
    {
        if (string.IsNullOrEmpty(TempExtractedAttachmentsDir))
            TempExtractedAttachmentsDir =
                _helper.CreateDirectory(Path.Combine(TempDirectory, EmailParserDirectory, Guid.NewGuid().ToString()));
    }

    /// <summary>
    ///     Инициализирует путь к временной директории для распаковки писем из архива.
    /// </summary>
    private void InitializeTempUnzippedEmlDir()
    {
        if (string.IsNullOrEmpty(TempUnzippedEmlDir))
            TempUnzippedEmlDir =
                _helper.CreateDirectory(Path.Combine(TempDirectory, EmailParserDirectory, Guid.NewGuid().ToString()));
    }
}