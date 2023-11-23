using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace task2_ConsoleFileManager
{
    internal class FileManager
    {
        private string currentDir = "null";
        DirectoryInfo currentDirObj ;

        protected string menuText =  
            "1 – Устанавливает текущий диск/каталог\n" +
            "2 – Выводит список всех каталогов в текущем (пронумерованный)\n" +
            "3 – Выводит список всех файлов в текущем каталоге (пронумерованнный)\n" +
            "4 – Выводит на экран содержимое указанного файла (по номеру)\n" +
            "5 – Создает каталог в текущем\n" +
            "6 – Удаляет каталог по номеру, если он пустой\n" +
            "7 – Удаляет файлы с указанными номерами\n" +
            "8 – Выводит список всех файлов с указанной датой создания (ищет в текущем каталоге и подкаталогах)\n" +
            "9 – Выводит список всех текстовых файлов, в которых содержится указанный текст (ищет в текущем каталоге и подкаталогах)\n" +
            "0 – Выход\n";
        public FileManager() 
        {
            currentDirObj = new DirectoryInfo(Directory.GetCurrentDirectory());
            SetCurrentDir(Directory.GetCurrentDirectory());
            
        }
        /// <summary>
        /// Возвращает массив строк. Имена всех директорий содержащихся в директории переданной в аргументе.
        /// </summary>
        protected string[] GatAllDirs(DirectoryInfo dir)
        {
            DirectoryInfo[] allDirs = dir.GetDirectories();
            string[] allDirsStr = new string[allDirs.Length];

            if (allDirs.Length > 0)
            {
                for(int  i =0; i< allDirsStr.Length; i++)
                {
                    allDirsStr[i] = allDirs[i].Name;
                }
            }
            return allDirsStr;
        }
        /// <summary>
        /// Устанавливает директорию как текущую.
        /// </summary>
        protected void SetCurrentDir(string path )
        {
            if (GatAllDirs(currentDirObj).Contains(path)) // Если аргумент -  имя папки в текущей директории
            {
                currentDir = Path.Combine(currentDirObj.FullName, path);
                currentDirObj = new DirectoryInfo(currentDir);
            }
            else
            {
                if (Directory.Exists(path)) // Если аргумент - другая существующая директория
                {
                    currentDir = path;
                    currentDirObj = new DirectoryInfo(path);
                }
                else
                {
                    throw new DirectoryNotFoundException("Директории не существует");
                }
            }
        }
        protected void ShowMenu()
        {
            string text = $"\nТекущая директория: {currentDir ?? ""}\n";
            Console.WriteLine(text);
            Console.WriteLine("Выберите действие: \n" + menuText);
        }
        /// <summary>
        /// Запрос ввода каталога или директории.
        /// </summary>
        protected string RequestText(string? text = null)
        {
            if (text != null)
            {
                Console.WriteLine($"Введите {text}:  ");
            }
            else
            {
                Console.WriteLine("Введите имя:  ");
            }
            string? name = Console.ReadLine();
            if (name == null)
            {
                throw new ArgumentNullException("Пустой ввод");
            }
            return name;
        }
        /// <summary>
        /// Запрашивает у пользователя и считывает с консоли один номер или несколько номеров.
        /// </summary>
        /// <param name="quetMode">Тихий режим - считывает номера без отображения текста запроса</param>
        /// <param name="manyMode">Считывает одно число или несколько чисел введных через пробел</param>
        /// <returns>int[]</returns>
        protected int[] RequestNumber(bool quetMode = false, bool manyMode=false) 
        {
            int [] numbersInt;
            string? numberStr;
            string[] numbersStr;

            // Вывод  на коносль сообщения соотвествию флагов тихого режима и режима множественного выбора.
            if (!quetMode && manyMode)    
            { 
                Console.WriteLine("Введите номера через пробел: ");
            }
            if (!quetMode && !manyMode)
            {
                Console.WriteLine("Введите номер: ");
            }

            numberStr = Console.ReadLine();
            // Обработка пустого ввода номера.
            if (numberStr == null)
            {
                Console.WriteLine("Пустой ввод, попробуйте еще раз");
                return RequestNumber(true, manyMode);
            }
            else
            {
                bool correctFalg = true;
                // Обработка множественного ввода номеров.
                if (manyMode)
                {
                    numbersStr = numberStr.Split(' ');
                    numbersInt = new int[numbersStr.Length];           
                    for(int i = 0; i < numbersInt.Length; i++)
                    {
                       correctFalg = Int32.TryParse((string)numbersStr[i], out numbersInt[i]);
                       if (!correctFalg || numbersInt[i] < 0)
                       {
                            Console.WriteLine("Не верный ввод, попробуйте еще раз");
                            return RequestNumber(false, manyMode);
                       }
                    }
                }
                // Обработка одиночного ввода номера.
                else
                {    
                    numbersInt = new int[1];
                    correctFalg = Int32.TryParse(numberStr, out numbersInt[0]);
                    if (!correctFalg || numbersInt[0] < 0)
                    {
                        Console.WriteLine("Не верный ввод, попробуйте еще раз");
                        return RequestNumber(false, manyMode);
                    }
                }
            }
            return numbersInt;
        } 
        /// <summary>
        /// Ищет в массиве файлов указанный в аргументе текст
        /// </summary>
        /// <param name="files">Массив файлов</param>
        /// <param name="FilterText">Искомый текст</param>
        /// <returns>FileInfo[] - массив файлов в которых найден искомый текст.</returns>
        protected FileInfo[] FileTextFilter(FileInfo[] files, string FilterText)
        {
            FileInfo[] result = new FileInfo[0];
            
            foreach (FileInfo file in files)
            {
                using(FileStream f = file.OpenRead())
                {
                    byte[] dataArray = new byte[f.Length];
                    f.Read(dataArray, 0, dataArray.Length);
                    string data = Encoding.Default.GetString(dataArray);

                    if (data.Contains(FilterText))
                    {
                        result = result.Append(file).ToArray();
                    }
                }
            }
                
            return result;
        }
        /// <summary>
        /// Запускает менеджер.
        /// </summary>
        public void run()
        {
            int[] numberOption;
            while(true)
            {
                ShowMenu();
                numberOption = RequestNumber();                

                switch (numberOption[0])
                {
                    case 1:
                        string dir = RequestText("имя каталога или путь к директории");
                        try
                        {
                            SetCurrentDir(dir);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            Console.WriteLine($"{dir} не существует.");
                        }
                        
                        break;
                    case 2:
                        string[] allDirs = GatAllDirs(currentDirObj);
                        if(allDirs.Length> 0)
                        {
                            int count = 1;
                            foreach(string d in allDirs)
                            {
                                Console.WriteLine($"{count}. {d}");
                                count++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Директория пуста.");
                        }
                        break;
                    case 3:
                        FileInfo[] allFiles = currentDirObj.GetFiles();
                        if (allFiles.Length> 0)
                        {
                            int count = 1;
                            foreach(FileInfo file in allFiles)
                            {
                                Console.WriteLine($"{count}. {file.Name}");
                                count++;
                            }
                        }
                        else { Console.WriteLine("Нет файлов."); }
                        break;
                    case 4:
                        FileInfo[] CurrentAllFiles = currentDirObj.GetFiles();
                        Console.WriteLine("Введите номер файла");
                        int numberFile = RequestNumber(true, false)[0];
                        if (numberFile!=0 && numberFile-1 < CurrentAllFiles.Length)
                        {
                            using (FileStream file = CurrentAllFiles[numberFile-1].OpenRead())
                            {
                                byte[] dataArray = new byte[file.Length];
                                file.Read(dataArray, 0, dataArray.Length);
                                string textFromFile = Encoding.Default.GetString(dataArray);

                                string border = '\n' +  new string('-', 70) + '\n';
                                Console.WriteLine (border + textFromFile + border);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Файла под номером {numberFile} не существует"); 
                        }

                        break;
                    case 5:
                        try
                        {
                            string nameNewFolder = RequestText();
                            currentDirObj.CreateSubdirectory(nameNewFolder);
                        }
                        catch (ArgumentNullException) { Console.WriteLine("Пустой ввод."); }
                        break;
                    case 6:
                        DirectoryInfo[] alldirs = currentDirObj.GetDirectories();
                        int num = RequestNumber()[0];
                        if (alldirs.Length>0 && num!=0 && num <= alldirs.Length)
                        {
                            alldirs[num - 1].Delete();
                            Console.WriteLine("Каталог удален.");
                        }
                        else
                        {
                            Console.WriteLine($"Директории под номером {num} не существует.");
                        }
                        break;
                    case 7:
                        FileInfo[] files = currentDirObj.GetFiles();
                        int[] nums = RequestNumber(false, true);
                        if (files.Length > 0)
                        {
                           foreach(int n in nums)
                            {
                                try
                                {
                                    files[n - 1].Delete();
                                    Console.WriteLine($"{files[n - 1].Name} удален.");
                                }
                                catch(IndexOutOfRangeException){
                                    Console.WriteLine($"Файл {n} уже был удален");
                                }
                            }
                        }
                        break;
                    case 8:
                        FileInfo[] allFolderfiles = currentDirObj.GetFiles("*", SearchOption.AllDirectories);
                        if (allFolderfiles.Length > 0)
                        {
                            int count = 1;
                            foreach(FileInfo file in allFolderfiles)
                            {
                                DateOnly date = new DateOnly(file.CreationTime.Year, file.CreationTime.Month, file.CreationTime.Day);
                                Console.WriteLine($"{count}. {file.Name} Дата создания: {date.ToString(CultureInfo.CurrentCulture)}.");
                                count++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Файлов не найдено.");
                        }
                        break;
                    case 9:
                        string filterText = RequestText("искомый текст");
                        FileInfo[] allTxtfiles = currentDirObj.GetFiles("*.txt", SearchOption.AllDirectories);

                        if(allTxtfiles.Length > 0)
                        {
                            FileInfo[] resultFiles = FileTextFilter(allTxtfiles, filterText);
                            if (resultFiles.Length < 1)
                            {
                                Console.WriteLine($"Файлов с текстом '{filterText}' не найдено.");
                            }
                            else
                            {
                                Console.WriteLine("Искомый текст найден в следующих файлах: ");
                                int count = 1;
                                foreach(FileInfo file in resultFiles)
                                {
                                    Console.WriteLine($"{count}. {file.FullName}");
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Файлов с текстом '{filterText}' не найдено.");
                        }

                        break;
                    case 0:
                        Console.WriteLine("Приложение закрыто.");
                        System.Environment.Exit(0);
                        break;
                    default: 
                        Console.WriteLine("Не известная команда!");
                        break;

                }
            }
        }
    }
}
