using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileManager
{
    /// <summary>
    /// Главный исполняемый класс программы.
    /// </summary>
    class Program
    {
        private static DirectoryInfo _currentDirectory;
        
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
        /// Точка входа в программу; содержит главный цикл, обрабатывающий операции пользователя.
        /// </summary>
        /// <param name="args">---</param>
        static void Main(string[] args)
        {
            PrintOperations();
            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");

            _currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            bool programContinues = true;
            do
            {
                try
                {
                    var operation = InputCorrectOperation();
                    programContinues = HandleOperation(operation);
                }
                catch (Exception e)
                {
                    PrintError(e.Message);
                }
            } while (programContinues);
        }

        /// <summary>
        /// Запрашивает у пользователя операцию до тех пора, пока не будет введена корректная.
        /// </summary>
        /// <returns>Массив с названием операции и аргументами.</returns>
        static string[] InputCorrectOperation()
        {
            string[] operation = Array.Empty<string>();
            bool operationIsCorrect = false;
            while (!operationIsCorrect) 
            {
                Console.Write($"{_currentDirectory.FullName}: ");
                operation = Console.ReadLine()?.Split();
                
                if (operation == null || operation.Length == 0 || string.IsNullOrEmpty(operation[0]))
                    continue;

                var operationName = operation[0];
                if (!_availableOperations.Contains(operationName))
                {
                    PrintError($"Операция `{operationName}` не существует.");
                    continue;
                }
                
                if (Regex.Match(operationName, "drives|dir|dirrec|help|exit").Success)
                    break;

                if (Regex.Match(operationName, "cd|remove").Success && operation.Length != 2
                    || Regex.Match(operationName, "copy|^move").Success && operation.Length != 3 
                    || Regex.Match(operationName, "print|create").Success && operation.Length is not (>= 2 and <= 3) 
                    || Regex.Match(operationName, "concat").Success && operation.Length < 3)
                {
                    PrintError($"Неверное количество аргументов для операции `{operationName}`.");
                    continue;
                }
                
                if (CheckOperationArguments(operation))
                    operationIsCorrect = true;
            }
            return operation;
        }

        /// <summary>
        /// Проверяет введеные аргументы для каждой операции.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns>true, если аргументы корректны; false иначе.</returns>
        static bool CheckOperationArguments(string[] operation) => operation[0] switch
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
        static bool HasFilename(string path)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(path)))
                PrintError("В пути не найдено имя нового файла.");
            else
                return true;
            return false;
        }

        /// <summary>
        /// Проверяет, является ли указанный путь существующей директорией.
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если директория существует; false иначе.</returns>
        static bool IsDirectory(string path)
        {
            if (!Directory.Exists(path))
                PrintError($"Не найдена директория `{path}`.");
            else
                return true;
            return false;
        }
        
        /// <summary>
        /// Проверяет, является ли указанный путь существующим файлом.
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если файл существует; false иначе.</returns>
        static bool IsFile(string path)
        {
            if (!File.Exists(path))
                PrintError($"Не найден файл `{path}`.");
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
        static bool ArePathsEqual(string path1, string path2)
        {
            string absolutePath1 = Path.GetFullPath(path1);
            string absolutePath2 = Path.GetFullPath(path2);
            if (absolutePath1 == absolutePath2)
                PrintError("Пути ведут к одному и тому же файлу.");
            else
                return false;
            return true;
        }

        /// <summary>
        /// Проверяет, указана ли корректная поддерживаемая в программе кодировка.
        /// </summary>
        /// <param name="encoding">Указанная кодировка.</param>
        /// <returns>true, если кодировка корректна; false иначе.</returns>
        static bool IsCorrectEncoding(string encoding)
        {
            if (!_availableEncodings.Contains(encoding))
                PrintError($"Некорректная кодировка `{encoding}`.");
            else
                return true;
            return false;
        }

        /// <summary>
        /// Выводит на экран ошибку.
        /// </summary>
        /// <param name="message">Доп. сообщение для пользователя.</param>
        static void PrintError(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        /// <summary>
        /// Обрабатывает операцию: вызывает нужный метод и передает аргументы.
        /// </summary>
        /// <param name="operation">Массив, содержащий название операции и аргументы.</param>
        /// <returns>true, если программа продолжает работу после этой операции; false иначе.</returns>
        static bool HandleOperation(string[] operation)
        {
            switch (operation[0])
            {
                case "drives":
                    PrintDrives();
                    break;
                case "cd":
                    ChangeCurrentDirectory(operation[1]);
                    break;
                case "dir":
                    PrintDirectory(operation.Length == 2 ? operation[1] : "*");
                    break;
                case "dirrec":
                    RecursivePrintFiles(_currentDirectory, operation.Length == 2 ? operation[1] : "*");
                    break;
                case "print":
                    PrintFile(operation[1], operation.Length == 3 ? operation[2] : "utf-8");
                    break;
                case "copy":
                    CopyFile(operation[1], operation[2]);
                    break;
                case "move":
                    MoveFile(operation[1], operation[2]);
                    break;
                case "remove":
                    RemoveFile(operation[1]);
                    break;
                case "create":
                    CreateFile(operation[1], operation.Length == 3 ? operation[2] : "utf-8");
                    break;
                case "concat":
                    ConcatenateFiles(operation[1..]);
                    break;
                case "help":
                    PrintOperations();
                    break;
                case "exit":
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Выводит на экран список доступных дисков.
        /// </summary>
        static void PrintDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                Console.WriteLine(drive);
            }
        }

        /// <summary>
        /// Меняет текущую рабочую директорию.
        /// </summary>
        /// <param name="newDirectoryPath">Путь к новой директории.</param>
        static void ChangeCurrentDirectory(string newDirectoryPath)
        {
            _currentDirectory = new DirectoryInfo(newDirectoryPath);
            Directory.SetCurrentDirectory(_currentDirectory.FullName);
        }

        /// <summary>
        /// Печатает файлы и директории в текущей директории.
        /// </summary>
        /// <param name="mask">Маска для поиска файлов и директорий.</param>
        static void PrintDirectory(string mask)
        {
            foreach (var file in _currentDirectory.GetFiles(mask))
            {
                Console.WriteLine($"{file.Name} ({file.Length} b)");
            }

            foreach (var directory in _currentDirectory.GetDirectories(mask))
            {
                Console.WriteLine(directory.Name);
            }
        }

        /// <summary>
        /// Рекурсивно печатает все файлы в указанной директории и поддиректориях.
        /// </summary>
        /// <param name="directory">Директория, в которой необходимо вести поиск.</param>
        /// <param name="mask">Маска для поиска файлов.</param>
        static void RecursivePrintFiles(DirectoryInfo directory, string mask)
        {
            foreach (var file in directory.GetFiles(mask))
            {
                string relativePath = Path.GetRelativePath(_currentDirectory.FullName, directory.FullName);
                Console.WriteLine($"{relativePath}{Path.DirectorySeparatorChar}{file.Name} ({file.Length} b)");
            }
            
            foreach (var childDirectory in directory.GetDirectories())
            {
                RecursivePrintFiles(childDirectory, mask);
            }
        }

        /// <summary>
        /// Выводит на экран содержимое текстового файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="encoding">Кодировка, в которой нужно вывести содержимое файла.</param>
        static void PrintFile(string filePath, string encoding)
        {
            foreach (var line in File.ReadLines(filePath, Encoding.GetEncoding(encoding)))
            {
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// Создает директорию.
        /// </summary>
        /// <param name="directoryPath">Путь к директории или файлу, для которого нужно создать директорию.</param>
        static void CreateDirectory(string directoryPath)
        {
            string directoryName = Path.GetDirectoryName(directoryPath);
            if (String.IsNullOrEmpty(directoryName))
                return;
            Directory.CreateDirectory(directoryName);
        }

        /// <summary>
        /// Проверяет, существует ли файл или директория с указанным именем. 
        /// </summary>
        /// <param name="path">Путь, который необходимо проверить.</param>
        /// <returns>true, если имя занято; false иначе.</returns>
        static bool IsFileOrDirectoryExists(string path) => File.Exists(path) || Directory.Exists(path);

        /// <summary>
        /// Спрашивает у пользователя, заменить ли объект по существующему пути.
        /// </summary>
        /// <param name="path">Путь, о котором необходимо спросить.</param>
        /// <returns>true, если заменить; false иначе.</returns>
        static bool AskAboutReplace(string path)
        {
            Console.WriteLine($"Путь `{path}` уже занят файлом или папкой.\n" +
                              "Хотите заменить его? [y/n] (y - заменить, n - оставить без изменений)");
            ConsoleKeyInfo answer = Console.ReadKey();
            while (answer.Key is not (ConsoleKey.Y or ConsoleKey.N)) 
                answer = Console.ReadKey();
            Console.WriteLine();
            if (answer.Key == ConsoleKey.N)
                return false;

            if (Directory.Exists(path))
                Directory.Delete(path, true);
            
            return true;
        }
        
        /// <summary>
        /// Копирует файл.
        /// </summary>
        /// <param name="filePath">Путь файла, который необходимо скопировать.</param>
        /// <param name="newFilePath">Путь к новому файлу.</param>
        static void CopyFile(string filePath, string newFilePath)
        {
            bool replaceFile = false;
            if (IsFileOrDirectoryExists(newFilePath))
            {
                replaceFile = AskAboutReplace(newFilePath);
                if (!replaceFile) 
                    return;
            }
            else
                CreateDirectory(newFilePath);
            File.Copy(filePath, newFilePath, replaceFile);
        }

        /// <summary>
        /// Перемещает файл.
        /// </summary>
        /// <param name="sourceFilePath">Файл, который необходимо переместить.</param>
        /// <param name="destinationFilePath">Путь к новому файлу.</param>
        static void MoveFile(string sourceFilePath, string destinationFilePath)
        {
            bool replaceFile = false;
            if (IsFileOrDirectoryExists(destinationFilePath))
            {
                replaceFile = AskAboutReplace(destinationFilePath);
                if (!replaceFile) 
                    return;
            }
            else
                CreateDirectory(destinationFilePath);
            File.Move(sourceFilePath, destinationFilePath, replaceFile);
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="filePath">Путь к файлу, который необходимо удалить.</param>
        static void RemoveFile(string filePath)
        {
            File.Delete(filePath);
        }

        /// <summary>
        /// Создает файл в указанной кодировке.
        /// </summary>
        /// <param name="filePath">Путь к создаваемому файлу.</param>
        /// <param name="encoding">Кодировка, в которой нужно создать файл.</param>
        static void CreateFile(string filePath, string encoding)
        {
            Console.Write("Введите строку, которую необходимо записать в файл: ");
            string text = Console.ReadLine() ?? "";
            using StreamWriter streamWriter = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.GetEncoding(encoding));
            streamWriter.WriteLine(text);
        }

        /// <summary>
        /// Выводит по порядку содержимое переданных текстовых файлов.
        /// </summary>
        /// <param name="filesPath">Пути к файлам.</param>
        static void ConcatenateFiles(string[] filesPath)
        {
            StringBuilder result = new StringBuilder();
            foreach (var filePath in filesPath)
            {
                result.Append(File.ReadAllText(filePath, Encoding.UTF8));
            }
            Console.WriteLine(result);
        }

        /// <summary>
        /// Проверяет, имеет ли файл расширение `.txt`.
        /// </summary>
        /// <param name="filePath">Путь к файлу, который необходимо проверить.</param>
        /// <returns>true, если расширение `.txt`; false иначе.</returns>
        static bool CheckTxtExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension != ".txt")
                PrintError("Указанный файл не имеет расширение `.txt`.");
            else
                return true;
            
            return false;
        }
        
        /// <summary>
        /// Выводит на экран список операций и инструкцию пользования программой.
        /// </summary>
        static void PrintOperations()
        {
            Console.WriteLine("FileManager: вводите название команды и аргументы через пробел без каких-либо других символов.\n" +
                              "Обращайте внимание на выделенные капсом слова: где-то нужен путь к файлу, где-то к директории.\n" +
                              "Чтобы выбрать диск, используйте команду `cd` с аргументом `название диска` + двоеточие (c:, d:, ...)\n" +
                              "Список доступных операций:\n" +
                              "drives - вывести список доступных дисков.\n" +
                              "cd <path> - сменить текущую ДИРЕКТОРИЮ (.. - ур. вверх).\n" +
                              "dir <mask> - вывести список файлов и директорий в текущей директории по заданной маске (необяз.).\n" +
                              "dirrec <mask> - рекурсивно вывести список файлов в текущей директории и " +
                              "всех поддиректориях по заданной маске (необяз.)\n" +
                              "print <path> <enc: utf-8/utf-16/utf-32/us-ascii> - вывести содержимое " +
                              "текстового (обяз. `.txt`) ФАЙЛА (кодировка необязательна, utf-8 стандартное знач).\n" +
                              "copy <path1> <path2> - копирует указанный в `path1` ФАЙЛ в новый ФАЙЛ `path2`.\n" +
                              "move <path1> <path2> - перемещает указанный в `path1` ФАЙЛ в новый ФАЙЛ `path2`.\n" +
                              "remove <path> - удаляет указанный ФАЙЛ.\n" +
                              "create <path> <enc: utf-8/utf-16/utf-32/us-ascii>  - создать текстовый ФАЙЛ " + 
                              "(обяз. расширение `.txt`) в указанной кодировке (необяз., utf-8 стандартное знач).\n" +
                              "concat <path1> <path2> ... <path_n> - конкатенация двух или более текстовых ФАЙЛОВ (с расширением `.txt`).\n" +
                              "help - вывести список операций.\n" +
                              "exit - выйти из программы.\n");
        }
    }
}