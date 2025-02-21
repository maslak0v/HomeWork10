using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

class Program
{
    static async Task Main(string[] args)
    {
        string rootPath = @"C:\Otus";
        string dir1Path = Path.Combine(rootPath, "TestDir1");
        string dir2Path = Path.Combine(rootPath, "TestDir2");

        // создание директорий
        CreateDirectory(dir1Path);
        CreateDirectory(dir2Path);

        // создание файлов
        CreateFiles(dir1Path);
        CreateFiles(dir2Path);

        // дополнение файлов текущей датой
        foreach (string file in Directory.GetFiles(dir1Path))
        {
            AppendCurrentDateSync(file); // синхронный вызов
        }

        foreach (string file in Directory.GetFiles(dir2Path))
        {
            await AppendCurrentDateAsync(file); // асинхронный вызов
        }

        // чтение и вывод содержимого файлов
        ReadAndDisplayFiles(dir1Path);
        ReadAndDisplayFiles(dir2Path);
    }

    static void CreateDirectory(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        if (!directory.Exists)
        {
            directory.Create();
            Console.WriteLine($"Создана директория: {path}");
        }
        else
        {
            Console.WriteLine($"Директория уже существует: {path}");
        }
    }

    static void CreateFiles(string directoryPath)
    {
        for (int i = 1; i <= 10; i++)
        {
            string fileName = $"File{i}.txt";
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, fileName, System.Text.Encoding.UTF8);
                    Console.WriteLine($"Файл создан: {filePath}");
                }
                else
                {
                    Console.WriteLine($"Файл уже существует: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании файла {filePath}: {ex.Message}");
            }
        }
    }
    static bool HasWriteAccess(string filePath)
    {
        try
        {
            // создаем объект FileInfo для указанного файла
            FileInfo fileInfo = new FileInfo(filePath);

            // получаем объект FileSecurity для файла
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            // получаем текущего пользователя
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            AuthorizationRuleCollection rules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

            // проверяем правила доступа
            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == identity.Name)
                {
                    if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write &&
                        rule.AccessControlType == AccessControlType.Allow)
                    {
                        return true; // право на запись есть
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке прав доступа: {ex.Message}");
        }

        return false; // Если правило не найдено или нет права на запись
    }

    static void AppendCurrentDateSync(string filePath)
    {
        if (!HasWriteAccess(filePath))
        {
            Console.WriteLine($"Недостаточно прав для записи в файл: {filePath}");
            return;
        }
        try
        {
            string currentDate = $"\n{DateTime.Now}";
            File.AppendAllText(filePath, currentDate, System.Text.Encoding.UTF8);
            Console.WriteLine($"Текущая дата добавлена в файл: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи в файл {filePath}: {ex.Message}");
        }
    }

    static async Task AppendCurrentDateAsync(string filePath)
    {
        if (!HasWriteAccess(filePath))
        {
            Console.WriteLine($"Недостаточно прав для записи в файл: {filePath}");
            return;
        }
        try
        {
            string currentDate = $"\n{DateTime.Now}";
            await File.AppendAllTextAsync(filePath, currentDate, System.Text.Encoding.UTF8);
            Console.WriteLine($"Текущая дата добавлена в файл (асинхронно): {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при асинхронной записи в файл {filePath}: {ex.Message}");
        }
    }

    static void ReadAndDisplayFiles(string directoryPath)
    {
        foreach (string file in Directory.GetFiles(directoryPath))
        {
            try
            {
                string content = File.ReadAllText(file, System.Text.Encoding.UTF8);
                Console.WriteLine($"{Path.GetFileName(file)}: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла {file}: {ex.Message}");
            }
        }
    }
}