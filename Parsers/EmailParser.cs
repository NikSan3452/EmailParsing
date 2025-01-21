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
    private CancellationTokenSource _cts;

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

    /// <inheritdoc />
    public string TempDirectory { get; set; }

    /// <inheritdoc />
    public string EmailParserDirectory { get; set; } = "EmailParser";

    /// <inheritdoc />
    public string TempUnzippedEmlDir { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ZipFilePath { get; set; }

    /// <inheritdoc />
    public string TempExtractedAttachmentsDir { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Progress { get; private set; }

    /// <inheritdoc />
    public bool DeleteSourceFile { get; set; } = false;

    /// <inheritdoc />
    public event EventHandler<int>? ProgressChanged;

    /// <inheritdoc />
    public OperationType CurrentOperation { get; private set; } = OperationType.None;

    /// <inheritdoc />
    public string SourceFilePath { get; set; } = string.Empty;

    /// <inheritdoc />
    public async Task ParseArchiveAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        InitializeTempUnzippedEmlDir();
        InitializeExtractedAttachmentsDir();

        CurrentOperation = OperationType.Unpacking;
        Progress = 0;
        OnProgressChanged(Progress);
        try
        {
            await UnpackArchiveAsync(_cts.Token);

            var emailFiles = await ScanForEmlFiles();

            if (emailFiles.Count == 0)
            {
                await CleanupAsync();
                return;
            }

            CurrentOperation = OperationType.Extraction;
            Progress = 0;
            OnProgressChanged(Progress);
            await ExtractAttachmentsAsync(emailFiles, _cts.Token);

            CurrentOperation = OperationType.Packing;
            Progress = 0;
            OnProgressChanged(Progress);
            await PrepareOutputArchiveAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Операция была отменена в ParseArchiveAsync");
        }
        finally
        {
            await CleanupAsync();
        }
    }

    /// <inheritdoc />
    public async Task ParseEmlFileAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        InitializeExtractedAttachmentsDir();

        try
        {
            var content = await _extractor.ExtractEmailContentAsync(SourceFilePath, cancellationToken);
            await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir, cancellationToken);

            CurrentOperation = OperationType.Packing;
            Progress = 0;
            OnProgressChanged(Progress);
            await PrepareOutputArchiveAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Операция была отменена в ParseEmlFileAsync.");
        }
        finally
        {
            CurrentOperation = OperationType.Cleanup;
            await CleanupAsync();
            CurrentOperation = OperationType.Complete;
            OnProgressChanged(Progress);
        }
    }

    /// <inheritdoc />
    public async Task CleanupAsync()
    {
        _cts?.Cancel();

        if (DeleteSourceFile)
            _helper.DeleteFiles(new List<string> { SourceFilePath });

        await Task.Run(() => _helper.DeleteDirectory(TempUnzippedEmlDir));
        await Task.Run(() => _helper.DeleteDirectory(TempExtractedAttachmentsDir));
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
    ///     <param name="cancellationToken">Токен отмены</param>
    /// </summary>
    private async Task UnpackArchiveAsync(CancellationToken cancellationToken)
    {
        await _archiver.UnZip(SourceFilePath, TempUnzippedEmlDir, cancellationToken);
    }

    /// <summary>
    ///     Сканирует директорию на наличие EML-файлов.
    /// </summary>
    /// <returns>Список путей к EML-файлам.</returns>
    private async Task<List<string>> ScanForEmlFiles()
    {
        return await Task.Run(() => _scanner.ScanForEmailFiles(TempUnzippedEmlDir));
    }

    /// <summary>
    ///     Извлекает вложения из EML-файлов.
    /// </summary>
    /// <param name="emailFiles">Список путей к EML-файлам.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task ExtractAttachmentsAsync(List<string> emailFiles, CancellationToken cancellationToken)
    {
        var processedFiles = 0;
        var totalFiles = emailFiles.Count;

        foreach (var emailFile in emailFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await _extractor.ExtractEmailContentAsync(emailFile, cancellationToken);
            await _saver.SaveEmailContentAsync(content, TempExtractedAttachmentsDir, cancellationToken);
            processedFiles++;
            Progress = (int)((double)processedFiles / totalFiles * 100);
            OnProgressChanged(Progress);
        }
    }

    /// <summary>
    ///     Подготавливает выходной архив.
    ///     <param name="cancellationToken">Токен отмены.</param>
    /// </summary>
    private async Task PrepareOutputArchiveAsync(CancellationToken cancellationToken)
    {
        await _archiver.Zip(TempExtractedAttachmentsDir, ZipFilePath, cancellationToken);
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