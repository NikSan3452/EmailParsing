namespace EmailParsing.Helpers;

/// <summary>
/// Вспомогательный класс для работы с файловой системой.
/// </summary>
internal class FileSystemHelper : IFileSystemHelper
{
    /// <summary>
    /// Очищает строку от недопустимых символов и обрезает ее до 250 символов.
    /// </summary>
    /// <param name="text">Исходная строка.</param>
    /// <returns>Очищенная строка.</returns>
    public string SanitizeString(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            text = text.Trim();
            if (text.Length > 250) text = text[..250];
            var invalidChars = Path.GetInvalidFileNameChars();
        
            text = new string(text.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());

            text = text.TrimEnd('.'); 
        }

        return text;
    }

    /// <summary>
    /// Возвращает тему письма или строку "Без темы" с уникальным идентификатором, если тема пуста.
    /// </summary>
    /// <param name="subject">Тема письма.</param>
    /// <returns>Тема письма или строка "Без темы" с уникальным идентификатором.</returns>
    public string GetSubjectOrDefault(string subject)
    {
        return string.IsNullOrEmpty(subject)
            ? $"Без темы {Guid.NewGuid()}"
            : SanitizeString(subject);
    }

    /// <summary>
    /// Преобразует путь к файлу в длинный путь (более 255 символов), добавляя префикс \\?\
    /// </summary>
    /// <param name="path">Исходный путь к файлу.</param>
    /// <returns>Длинный путь к файлу.</returns>
    public string ConvertToLongPath(string path)
    {
        if (path.StartsWith(@"\\?\"))
            return path;

        if (Path.IsPathRooted(path)) return @"\\?\" + path;

        var fullPath = Path.GetFullPath(path);
        return @"\\?\" + fullPath;
    }

    /// <summary>
    /// Возвращает путь к рабочему столу текущего пользователя.
    /// </summary>
    /// <returns>Путь к рабочему столу.</returns>
    public string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    /// <summary>
    /// Удаляет указанные файлы.
    /// </summary>
    /// <param name="files">Список путей к файлам, которые необходимо удалить.</param>
    public void DeleteFiles(List<string> files)
    {
        foreach (var file in files.Where(File.Exists)) File.Delete(file);
    }

    /// <summary>
    /// Удаляет указанную директорию рекурсивно.
    /// </summary>
    /// <param name="directory">Путь к директории, которую необходимо удалить.</param>
    public void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    /// <summary>
    /// Создает директорию. Если директория существует, к ней добавляется уникальный идентификатор.
    /// </summary>
    /// <param name="directory">Путь к создаваемой директории.</param>
    /// <returns>Путь к созданной директории.</returns>
    public string CreateDirectory(string directory)
    {
        if (Directory.Exists(directory)) directory = $"{directory}_{Guid.NewGuid()}";
        Directory.CreateDirectory(directory);
        return directory;
    }
}