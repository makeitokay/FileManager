using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    public static class Validator
    {
        private static readonly List<string> _availableOperations = new()
        {
            "drives",
            "dir",
            "cd",
            "print",
            "copy",
            "move",
            "remove",
            "create",
            "concat",
            "help",
            "exit",
            "dirrec"
        };

        private static readonly List<string> _availableEncodings = new()
        {
            "utf-8",
            "utf-16",
            "utf-32",
            "us-ascii"
        };

        /// <summary>
        /// Проверяет, существует ли операция.
        /// </summary>
        /// <param name="operation">Проверяемая операция.</param>
        /// <returns>true, если существует; false иначе.</returns>
        public static bool OperationExists(string operation) => _availableOperations.Contains(operation);

        public static bool CheckArgumentsCount(string operation, int count) => operation switch
        {
            "drives" or "dir" or "dirrec" or "help" or "exit" => true,
            "copy" or "move" => count == 3,
            "print" or "create" => count is >= 2 and <= 3,
            "concat" => count >= 3,
            _ => true
        };

        /// <summary>
        /// Проверяет введеные аргументы для каждой операции.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns>true, если аргументы корректны; false иначе.</returns>
        public static bool CheckOperationArguments(string[] operation) => operation[0] switch
        {
            "cd" => IsDirectory(operation[1]),
            "print" => IsFile(operation[1]) 
                       && CheckTxtExtension(operation[1])
                       && (operation.Length != 3 || IsCorrectEncoding(operation[2])),
            "copy" => IsFile(operation[1]) && !ArePathsEqual(operation[1], operation[2]) && HasFilename(operation[2]),
            "move" => IsFile(operation[1]) && !ArePathsEqual(operation[1], operation[2]) && HasFilename(operation[2]),
            "remove" => IsFile(operation[1]),
            "create" => CheckTxtExtension(operation[1]) && (operation.Length != 3 || IsCorrectEncoding(operation[2])),
            "concat" => operation[1..].All(IsFile) && operation[1..].All(CheckTxtExtension),
            _ => true
        };

        /// <summary>
        /// Проверяет, содержится ли имя какого-то файла в указанном пути.
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если содержится; false иначе.</returns>
        public static bool HasFilename(string path)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(path)))
                Program.PrintError("В пути не найдено имя нового файла.");
            else
                return true;
            return false;
        }

        /// <summary>
        /// Проверяет, является ли указанный путь существующей директорией.
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если директория существует; false иначе.</returns>
        public static bool IsDirectory(string path)
        {
            if (!Directory.Exists(path))
                Program.PrintError($"Не найдена директория `{path}`.");
            else
                return true;
            return false;
        }
        
        /// <summary>
        /// Проверяет, является ли указанный путь существующим файлом.
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если файл существует; false иначе.</returns>
        public static bool IsFile(string path)
        {
            if (!File.Exists(path))
                Program.PrintError($"Не найден файл `{path}`.");
            else
                return true;
            return false;
        }

        /// <summary>
        /// Проверяет, эквиваленты ли указанные пути (ведут в одно и то же место/файл).
        /// </summary>
        /// <param name="path1">Путь 1.</param>
        /// <param name="path2">Путь 2.</param>
        /// <returns>true, если пути одинаковы; false иначе.</returns>
        public static bool ArePathsEqual(string path1, string path2)
        {
            string absolutePath1 = Path.GetFullPath(path1);
            string absolutePath2 = Path.GetFullPath(path2);
            if (absolutePath1 == absolutePath2)
                Program.PrintError("Пути ведут к одному и тому же файлу.");
            else
                return false;
            return true;
        }

        /// <summary>
        /// Проверяет, указана ли корректная поддерживаемая в программе кодировка.
        /// </summary>
        /// <param name="encoding">Указанная кодировка.</param>
        /// <returns>true, если кодировка корректна; false иначе.</returns>
        public static bool IsCorrectEncoding(string encoding)
        {
            if (!_availableEncodings.Contains(encoding))
                Program.PrintError($"Некорректная кодировка `{encoding}`.");
            else
                return true;
            return false;
        }

        /// <summary>
        /// Проверяет, существует ли файл или директория с указанным именем. 
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если имя занято; false иначе.</returns>
        public static bool IsFileOrDirectoryExists(string path) => File.Exists(path) || Directory.Exists(path);
        
        /// <summary>
        /// Проверяет, имеет ли файл расширение `.txt`.
        /// </summary>
        /// <param name="filePath">Путь к файлу, который необходимо проверить.</param>
        /// <returns>true, если расширение `.txt`; false иначе.</returns>
        public static bool CheckTxtExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension != ".txt")
                Program.PrintError("Указанный файл не имеет расширение `.txt`.");
            else
                return true;
            
            return false;
        }
    }
}