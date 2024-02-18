using System;
using System.IO;

namespace HashMethods
{
    class Program
    {
        static String globalId;            // глобальный идентификатор
        static HashTableSimple tableSimple;
        static HashTablePsevdo tablePsevdo;

        static string[] stringsFromFile;    // поля файла индентификаторов
        static DateTime dateStartSimple, dateEndSimple,
                        dateStartPsevdo, dateEndPsevdo;

        static void Main(string[] args)
        {
            /* Вариант 4
             * Первый метод организации коллизий – Рехэширование с использованием псевдослучайных чисел.
             * Второй метод организации коллизий – Простое рехэширование
             */

            String sourcepath = "";

            Console.WriteLine("Производится загрузка данных из файла...");      // кт 1

            if (args.Length == 2)   // допустимо указать путь к файлу в аргументах командной строки
            {
                if (args[0].Trim().CompareTo("--file") == 0)
                {
                    sourcepath = args[1];
                }
            }
            
            if (String.IsNullOrEmpty(sourcepath))
            {
                sourcepath = @"source.txt";
            }

            if (!File.Exists(sourcepath))
            {
                Console.WriteLine("Файл не найден :{0}!", sourcepath);
                return;
            }

            stringsFromFile = File.ReadAllLines(sourcepath);
            Console.WriteLine("Загрузка данных из файла завершена...");      // кт 2

            showMenu();
            while (true)
            {
                ReadChoiceAndHandle();
            }            
        }

        static void showMenu()
        {
            Console.WriteLine("\r\n*** \t\t Меню \t\t ***");
            Console.WriteLine("\t 0. Разместить исх. файл в хеш-таблице \t\t");
            Console.WriteLine("\t 1. Ввод идентификатора \t\t");
            Console.WriteLine("\t 2. Поиск в хеш-таблице \t\t");
            Console.WriteLine("\t 3. Показать меню \t\t");
            Console.WriteLine("\t 4. Выход \t\t\r\n");
        }

        static void ReadChoiceAndHandle()    // принимает только 4 значения
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            switch (keyInfo.KeyChar)
            {
                case '0':
                    {
                        dateStartSimple = DateTime.Now;
                        tableSimple = new HashTableSimple(stringsFromFile.Length);
                        tableSimple.IterCount = 1;
                        foreach (string line in stringsFromFile)
                        {
                            tableSimple.addNewItem(line);
                        }
                        dateEndSimple = DateTime.Now;

                        dateStartPsevdo = DateTime.Now;
                        tablePsevdo = new HashTablePsevdo(stringsFromFile.Length);
                        tablePsevdo.IterCount = 1;
                        foreach (string line in stringsFromFile)
                        {
                            tablePsevdo.addNewItem(line);
                        }
                        dateEndPsevdo = DateTime.Now;

                        Console.WriteLine("Рехэширование с использованием псевдослучайных чисел: ");
                        tablePsevdo.Show();
                        TimeSpan ts = dateEndPsevdo - dateStartPsevdo;
                        Console.WriteLine("Время выполнения: {0} мс {1} сравнений", ts.TotalMilliseconds, tablePsevdo.IterCount);

                        Console.WriteLine("\r\n");
                        Console.WriteLine("Простое рехэширование: ");
                        tableSimple.Show();
                        ts = dateEndSimple - dateStartSimple;
                        Console.WriteLine("Время выполнения: {0} мс {1} сравнений", ts.TotalMilliseconds, tableSimple.IterCount);
                        break;
                    };
                case '1': 
                    {
                        Console.WriteLine("Введите значение идентификатора: ");
                        globalId = Console.ReadLine();
                        break;
                    };
                case '2':
                    {
                        dateStartSimple = DateTime.Now;
                        HashItem item = tableSimple.searchItem(globalId);
                        dateEndSimple = DateTime.Now;
                        dateStartPsevdo = DateTime.Now;
                        HashItem itemS = tablePsevdo.searchItem(globalId);
                        dateEndPsevdo = DateTime.Now;
                        if (item != null && itemS != null)
                        {
                            Console.WriteLine("Идентификатор найден");
                            Console.WriteLine("{0} (Рехэширование с использованием псевдослучайных чисел)", itemS.ShowKey);
                            Console.WriteLine("{0} (Простое рехэширование)", item.ShowKey);
                            TimeSpan ts = dateEndPsevdo - dateStartPsevdo;
                            Console.WriteLine("Количество сравнений для рехэширования с использованием псевдослучайных чисел: {0} время {1} мс", tablePsevdo.IterCount, ts.TotalMilliseconds);
                            ts = dateEndSimple - dateStartSimple;
                            Console.WriteLine("Количество сравнений для Простого рехэширования: {0} время {1} мс", tableSimple.IterCount, ts.TotalMilliseconds);
                        }
                        else
                        {
                            Console.WriteLine("Идентификатор не найден");
                        }
                                                
                        break;
                    };
                case '3':
                    {
                        showMenu();
                        break;
                    };
                case '4':
                    {
                        Console.WriteLine("Завершение работы программы...");
                        Environment.Exit(0);
                        break;
                    };
                default:
                    {
                        Console.WriteLine("Неизвестная команда, попробуйте ввести заново...");
                        break;
                    };
            }
        }
    }
}
