namespace EmailParsing.Helpers;

/// <summary>
/// Интерфейс вспомогательного класса для работы с файловой системой.
/// </summary>
internal interface IFileSystemHelper
{
    /// <summary>
    /// Очищает строку от недопустимых символов и обрезает ее до максимальной длины.
    /// </summary>
    /// <param name="text">Исходная строка.</param>
    /// <returns>Очищенная строка.</returns>
    string SanitizeString(string text);

    /// <summary>
    /// Возвращает тему письма или строку "Без темы" с уникальным идентификатором, если тема пуста.
    /// </summary>
    /// <param name="subject">Тема письма.</param>
    /// <returns>Тема письма или строка "Без темы" с уникальным идентификатором.</returns>
    string GetSubjectOrDefault(string subject);

    /// <summary>
    /// Преобразует путь к файлу в длинный путь (более 255 символов).
    /// </summary>
    /// <param name="path">Исходный путь к файлу.</param>
    /// <returns>Длинный путь к файлу.</returns>
    string ConvertToLongPath(string path);

    /// <summary>
    /// Возвращает путь к рабочему столу текущего пользователя.
    /// </summary>
    /// <returns>Путь к рабочему столу.</returns>
    string GetDesktopPath();

    /// <summary>
    /// Удаляет указанные файлы.
    /// </summary>
    /// <param name="files">Список путей к файлам, которые необходимо удалить.</param>
    void DeleteFiles(List<string> files);

    /// <summary>
    /// Удаляет указанную директорию рекурсивно.
    /// </summary>
    /// <param name="directory">Путь к директории, которую необходимо удалить.</param>
    void DeleteDirectory(string directory);

    /// <summary>
    /// Создает директорию. Если директория существует, к ней добавляется уникальный идентификатор.
    /// </summary>
    /// <param name="directory">Путь к создаваемой директории.</param>
    /// <returns>Путь к созданной директории.</returns>
    string CreateDirectory(string directory);
}