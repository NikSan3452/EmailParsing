namespace EmailParsing.Scanners;

/// <summary>
/// Сканер электронной почты, выполняющий поиск EML-файлов и файлов с метаданными в указанной директории и ее поддиректориях.
/// </summary>
internal class EmailScanner : IEmailScanner
{
    /// <summary>
    /// Сканирует указанную директорию на наличие файлов электронной почты (EML).
    /// </summary>
    /// <param name="directory">Путь к директории для сканирования.</param>
    /// <returns>Список путей к найденным EML-файлам.</returns>
    public List<string> ScanForEmailFiles(string? directory)
    {
        var emailFiles = new List<string>();
        
            if (directory != null)
            {
                emailFiles.AddRange(Directory.GetFiles(directory, "*.eml"));

                foreach (var subDirectory in Directory.GetDirectories(directory))
                    emailFiles.AddRange(ScanForEmailFiles(subDirectory));
            }
            
        return emailFiles;
    }

    /// <summary>
    /// Сканирует указанную директорию на наличие файлов с метаданными электронной почты (с расширением .eml.meta).
    /// </summary>
    /// <param name="directory">Путь к директории для сканирования.</param>
    /// <returns>Список путей к найденным файлам с метаданными.</returns>
    public List<string> ScanForEmailMetaFiles(string? directory)
    {
        var emailFiles = new List<string>();
        
            if (directory != null)
            {
                emailFiles.AddRange(Directory.GetFiles(directory, "*.eml.meta"));

                foreach (var subDirectory in Directory.GetDirectories(directory))
                    emailFiles.AddRange(ScanForEmailMetaFiles(subDirectory));
            }
 
        return emailFiles;
    }
}