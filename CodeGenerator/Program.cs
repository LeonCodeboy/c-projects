using System;
using System.IO;

namespace CodeGenerator
{
    class Program
    {
        static string[] stringsFromFile;    // строки входного файла
        static Syntaxer analyzer;           // объект лексического анализатора
        static void Main(string[] args)
        {
            /* Вариант 4
             * Грамматика 
             * S -> F;
             * F -> for (T) do F | a := a
             * T -> F;E;F | ;E;F | F;E; | ;E;
             * E -> a < a | a > a | a = a
             * десятичные числа с плавающей запятой, комментарии {}
             */

            String sourcepath = "";


            Console.WriteLine("Производится загрузка данных из файла...");

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

            analyzer = new Syntaxer();
            analyzer.generateLexems(stringsFromFile);       // запуск лексического анализатора
            analyzer.SyntaxTree();                          // запуск синтаксического анализатора
            analyzer.buildTriads();                         // запуск генератора объектного кода
            showMenu();
            while (true)
            {
                ReadChoiceAndHandle();
            }
        }

        static void showMenu()
        {
            Console.WriteLine("\r\n*** \t\t Меню \t\t ***");
            Console.WriteLine("\t 0. Вывести входной текст \t\t");
            Console.WriteLine("\t 1. Вывести перечень лексем \t\t");
            Console.WriteLine("\t 2. Вывести перечень ошибок \t\t");
            Console.WriteLine("\t 3. Вывести дерево вывода \t\t");
            Console.WriteLine("\t 4. Вывести синтаксические ошибки \t\t");
            Console.WriteLine("\t 5. Вывести список триад \t\t");
            Console.WriteLine("\t 6. Вывести свертку \t\t");
            Console.WriteLine("\t 7. Вывести оптимизированный код\t\t");
            Console.WriteLine("\t 8. Показать меню \t\t");
            Console.WriteLine("\t 9. Выход \t\t\r\n");
        }

        static void ReadChoiceAndHandle()    // принимает только 5 значений
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            switch (keyInfo.KeyChar)
            {
                case '0':
                    {
                        Console.WriteLine();
                        foreach (string line in stringsFromFile)
                        {
                            Console.WriteLine(line);
                        }
                        break;
                    };
                case '1':
                    {
                        analyzer.ShowCorrectTokens();
                        break;
                    };
                case '2':
                    {
                        analyzer.ShowErrorTokens();
                        break;
                    };
                case '3':
                    {
                        analyzer.ShowSyntaxTree();
                        break;
                    };
                case '4':
                    {
                        analyzer.ShowSyntaxError();
                        break;
                    };
                case '5':
                    {
                        analyzer.showTriads();
                        break;
                    };
                case '6':
                    {
                        Console.WriteLine("Для выбранной грамматики свертка недоступна (отсутствуют мат. операции)!");
                        break;
                    };
                case '7':
                    {
                        analyzer.showTriadsOptimize();
                        break;
                    };
                case '8':
                    {
                        showMenu();
                        break;
                    };
                case '9':
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
