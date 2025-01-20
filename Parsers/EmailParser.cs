using EmailParsing.Archivers;
using EmailParsing.Extractors;
using EmailParsing.Helpers;
using EmailParsing.Savers;
using EmailParsing.Scanners;
using System.Text;

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
        _archiver.ProgressChanged += ArchiverProgressChanged;

        TempDirectory = Path.GetTempPath();
        ZipFilePath = Path.Combine(_helper.GetDesktopPath(), $"{Guid.NewGuid()}.zip");
    }

    /// <summary>
    ///     Путь к системной директории с временными файлами.
    /// </summary>
    private string TempDirectory { get; }

    /// <summary>
    ///     Имя директории парсера электронной почты, которая будет создана во временной директории.
    ///     По умолчанию EmailParser.
    /// </summary>
    private string EmailParserDirectory { get; } = "EmailParser";

    /// <summary>
    ///     Путь к временной директории для распаковки писем из архива.
    /// </summary>
    private string TempUnzippedEmlDir { get; set; } = string.Empty;

    /// <inheritdoc>
    public string ZipFilePath { get; set; }

    /// <inheritdoc>
    public string TempExtractedAttachmentsDir { get; set; } = string.Empty;

    /// <inheritdoc>
    public int Progress { get; private set; }

    /// <inheritdoc>
    public bool DeleteSourceFile { get; set; } = false;

    /// <inheritdoc>
    public event EventHandler<int>? ProgressChanged;

    /// <inheritdoc>
    public OperationType CurrentOperation { get; set; } = OperationType.None;

    /// <inheritdoc>
    public async Task ParseArchiveAsync(string sourcePath)
    {
        InitializeTempUnzippedEmlDir();
        InitializeExtractedAttachmentsDir();

        CurrentOperation = OperationType.Unpacking;
        Progress = 0;
        OnProgressChanged(Progress);
        await UnpackArchiveAsync(sourcePath);

        var emailFiles = ScanForEmlFiles();

        if (emailFiles.Count == 0)
        {
            await CleanupAsync(sourcePath);
            return;
        }

        CurrentOperation = OperationType.Extraction;
        Progress = 0;
        OnProgressChanged(Progress);
        await ExtractAttachmentsAsync(emailFiles);

        CurrentOperation = OperationType.Packing;
        Progress = 0;
        OnProgressChanged(Progress);
        await PrepareOutputArchiveAsync();

        await CleanupAsync(sourcePath);
    }

    /// <inheritdoc>
    public async Task ParseEmlFileAsync(string sourcePath)
    {
        InitializeExtractedAttachmentsDir();

        var content = await _extractor.ExtractEmailContentAsync(sourcePath);

        await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir);

        CurrentOperation = OperationType.Packing;
        Progress = 0;
        OnProgressChanged(Progress);
        await PrepareOutputArchiveAsync();

        CurrentOperation = OperationType.Cleanup;
        await CleanupAsync(sourcePath);
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
    private async Task UnpackArchiveAsync(string sourcePath)
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
    private async Task ExtractAttachmentsAsync(List<string> emailFiles)
    {
        var processedFiles = 0;
        var totalFiles = emailFiles.Count;

        foreach (var emailFile in emailFiles)
        {
            var content = await _extractor.ExtractEmailContentAsync(emailFile);
            await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir);
            processedFiles++;
            Progress = (int)((double)processedFiles / totalFiles * 100);
            OnProgressChanged(Progress);
        }
    }

    /// <summary>
    ///     Подготавливает выходной архив.
    /// </summary>
    private async Task PrepareOutputArchiveAsync()
    {
        await _archiver.Zip(TempExtractedAttachmentsDir, ZipFilePath);
    }

    /// <summary>
    ///     Очищает временные директории и удаляет исходный файл.
    /// </summary>
    /// <param name="sourcePath">Путь к исходному файлу.</param>
    private async Task CleanupAsync(string sourcePath)
    {
        if (DeleteSourceFile)
            _helper.DeleteFiles(new List<string> { sourcePath });

        await Task.Run(() => _helper.DeleteDirectory(TempUnzippedEmlDir));
        await Task.Run(() => _helper.DeleteDirectory(TempExtractedAttachmentsDir));

        CurrentOperation = OperationType.Complete;
        OnProgressChanged(Progress);
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

    /// <summary>
    ///     Обработчик события изменения прогресса архиватора.
    /// </summary>
    /// <param name="processedBytes">Обработано байт.</param>
    /// <param name="totalBytes">Всего байт.</param>
    private void ArchiverProgressChanged(long processedBytes, long totalBytes)
    {
        Progress = totalBytes > 0 ? (int)((double)processedBytes / totalBytes * 100) : 0;
        OnProgressChanged(Progress);
    }
}

/// <summary>
///     Типы операций, выполняемых парсером.
/// </summary>
public enum OperationType
{
    /// <summary>
    ///     Операция не определена.
    /// </summary>
    None,

    /// <summary>
    ///     Распаковка архива.
    /// </summary>
    Unpacking,

    /// <summary>
    ///     Извлечение вложений.
    /// </summary>
    Extraction,

    /// <summary>
    ///     Упаковка.
    /// </summary>
    Packing,

    /// <summary>
    ///     Очистка.
    /// </summary>
    Cleanup,

    /// <summary>
    ///     Выполнено.
    /// </summary>
    Complete
}