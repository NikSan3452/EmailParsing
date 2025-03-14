﻿using System.IO.Compression;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace EmailParsing.Archivers;

/// <summary>
///     Реализация архиватора для обработки email файлов с использованием ZIP.
/// </summary>
internal class EmailArchiver : IEmailArchiver
{
    /// <inheritdoc />
    public event Action<long, long>? ProgressChanged;

    /// <inheritdoc />
    public async Task Zip(string sourceDirectory, string zipFile, CancellationToken cancellationToken = default)
    {
        var totalBytes = GetDirectorySize(sourceDirectory);
        long processedBytes = 0;

        await Task.Run(() =>
        {
            using var zipArchive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            foreach (var filePath in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entryName = filePath.Substring(sourceDirectory.Length + 1);
                zipArchive.CreateEntryFromFile(filePath, entryName);

                // Обновление прогресса после добавления каждого файла
                var fileInfo = new FileInfo(filePath);
                processedBytes += fileInfo.Length;
                ProgressChanged?.Invoke(processedBytes, totalBytes);
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UnZip(string sourceArchive, string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

            using var stream = File.OpenRead(sourceArchive);
            using var reader = ReaderFactory.Open(stream);

            // Получаем общий размер архива для расчета прогресса
            var totalBytes = stream.Length;

            while (reader.MoveToNextEntry())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!reader.Entry.IsDirectory)
                {
                    // Отслеживаем прогресс распаковки на основе размера текущей записи
                    reader.WriteEntryToDirectory(destinationDirectory,
                        new ExtractionOptions { ExtractFullPath = true, Overwrite = true });

                    // Обновляем прогресс после распаковки каждой записи
                    var processedBytes = stream.Position;
                    ProgressChanged?.Invoke(processedBytes, totalBytes);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    ///     Вычисляет общий размер директории, включая все вложенные файлы.
    /// </summary>
    /// <param name="directoryPath">Путь к директории.</param>
    /// <returns>Размер директории в байтах.</returns>
    private static long GetDirectorySize(string directoryPath)
    {
        long size = 0;
        var directoryInfo = new DirectoryInfo(directoryPath);

        foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories)) size += fileInfo.Length;

        return size;
    }
}