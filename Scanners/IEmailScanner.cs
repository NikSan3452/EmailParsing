namespace EmailParsing.Scanners;

/// <summary>
/// Интерфейс сканера электронной почты.
/// </summary>
internal interface IEmailScanner
{
    /// <summary>
    /// Сканирует указанную директорию на наличие файлов электронной почты (EML).
    /// </summary>
    /// <param name="directory">Путь к директории для сканирования.</param>
    /// <returns>Список путей к найденным EML-файлам.</returns>
    List<string> ScanForEmailFiles(string? directory);

    /// <summary>
    /// Сканирует указанную директорию на наличие файлов с метаданными электронной почты (например, MSG).
    /// </summary>
    /// <param name="directory">Путь к директории для сканирования.</param>
    /// <returns>Список путей к найденным файлам с метаданными.</returns>
    List<string> ScanForEmailMetaFiles(string? directory);
}