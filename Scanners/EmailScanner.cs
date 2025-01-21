namespace EmailParsing.Scanners;

/// <summary>
/// Сканер электронной почты, выполняющий поиск EML-файлов и файлов с метаданными в указанной директории и ее поддиректориях.
/// </summary>
internal class EmailScanner : IEmailScanner
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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