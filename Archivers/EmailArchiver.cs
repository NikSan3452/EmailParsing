using System.IO.Compression;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace EmailParsing.Archivers;

/// <summary>
///     Реализация архиватора для обработки email файлов с использованием ZIP.
/// </summary>
internal class EmailArchiver : IEmailArchiver
{
    /// <summary>
    ///     Архивирует указанную директорию в ZIP-архив.
    /// </summary>
    /// <param name="sourceDirectory">Путь к директории, которую необходимо архивировать.</param>
    /// <param name="zipFile">Путь к создаваемому ZIP-архиву.</param>
    /// <returns>Задача, представляющая асинхронную операцию архивирования.</returns>
    public async Task Zip(string sourceDirectory, string zipFile)
    {
        await Task.Run(() => ZipFile.CreateFromDirectory(sourceDirectory, zipFile));
    }

    /// <summary>
    ///     Распаковывает ZIP-архив в указанную директорию.
    /// </summary>
    /// <param name="sourceArchive">Путь к ZIP-архиву.</param>
    /// <param name="destinationDirectory">Путь к директории, в которую необходимо распаковать архив.</param>
    /// <returns>Задача, представляющая асинхронную операцию распаковки.</returns>
    public async Task UnZip(string sourceArchive, string destinationDirectory)
    {
        await Task.Run(() =>
        {
            if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

            using var stream = File.OpenRead(sourceArchive);
            var reader = ReaderFactory.Open(stream);

            while (reader.MoveToNextEntry())
                if (!reader.Entry.IsDirectory)
                    reader.WriteEntryToDirectory(destinationDirectory,
                        new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
        });
    }
}