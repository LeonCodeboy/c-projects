using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashMethods
{
    class HashItem
    {
        public int Key { get; set; }
        public string Value { get; set; }

        public int ShowKey { get; set; } // для отображения
    }

    class HashTableSimple       // хеш таблица с простым рехешированием
    {
        private int realSize;
        private int Nm;         // максимальное значение из обл. значения хэш-функции

        private HashItem[] table;
        public int IterCount { get; set; } // количество сравнений

        public HashTableSimple(int fileSize) 
        {
            this.Nm = fileSize;
            this.table = new HashItem[this.Nm];
            this.realSize = 0;
        }

        public void Show()
        {
            foreach (HashItem item in table)
            {
                if (item != null)
                    Console.WriteLine("{0} {1}", item.ShowKey, item.Value);
            }
        }

        public void addNewItem(string newItem)
        {
            int i = 1;  // шаг в простом рехешировании
            int hashCode = this.hashFunction(newItem);
            int startHashCode = hashCode;
            while (this.table[hashCode] != null && this.table[hashCode].Key == hashCode)    // элемент существует
            {
                hashCode = (startHashCode + i) % this.Nm;    // простое рехеширование
                IterCount += 1;
                i++;
            }

            int keyCode = (hashCode == 0) ? 1 : hashCode + 1;
            this.table[hashCode] = new HashItem() { Key = hashCode, Value = newItem, ShowKey = keyCode };
            this.realSize += 1;
        }

        public HashItem searchItem(string srcItem)
        {
            IterCount = 1;
            int i = 1;  // шаг в простом рехешировании
            int hashCode = this.hashFunction(srcItem);
            int startHashCode = hashCode;
            while ( (this.table[hashCode] != null &&  
                    this.table[hashCode].Value.CompareTo(srcItem) != 0)
                    || this.table[hashCode] == null)    // элемент не найден
            {
                hashCode = (startHashCode + i) % this.Nm;    // простое рехеширование
                i++;
                IterCount += 1;
                if (hashCode == startHashCode) // невозможно найти
                {
                    return null;
                }
            }

            return this.table[hashCode];
        }

        private int hashFunction(string inputValue)
        {
            // базовая хеш функция - длина строки * (код первого + код последнего символа)
            int first = (int) inputValue[0];
            int last = (int) inputValue[inputValue.Length - 1];
            return (first + last) % this.Nm;
        }
    }

    class HashTablePsevdo       // хеш таблица с псевдослучайным рехешированием
    {
        private int realSize;
        private int Nm;         // максимальное значение из обл. значения хэш-функции

        private HashItem[] table;
        public int IterCount { get; set; } // количество сравнений

        public HashTablePsevdo(int fileSize)  
        {
            this.Nm = fileSize;
            this.table = new HashItem[this.Nm];
            this.realSize = 0;
        }

        public void Show()
        {
            foreach (HashItem item in table)
            {
                if (item != null)
                    Console.WriteLine("{0} {1}", item.ShowKey, item.Value);
            }
        }

        public void addNewItem(string newItem)
        {
            Random r = new Random();
            int hashCode = this.hashFunction(newItem);
            int startHashCode = hashCode;
            while (this.table[hashCode] != null && this.table[hashCode].Key == hashCode)    // элемент существует
            {
                int rInt = r.Next(0, this.Nm);
                hashCode = (startHashCode + rInt) % this.Nm;    // псевдослучайное рехеширование
                if (hashCode == 0)
                {
                    int x = 0;
                    x++;
                }
                IterCount += 1;
            }

            int keyCode = (hashCode == 0) ? 1 : hashCode + 1;
            this.table[hashCode] = new HashItem() { Key = hashCode, Value = newItem, ShowKey = keyCode };
            this.realSize += 1;
        }

        public HashItem searchItem(string srcItem)
        {
            IterCount = 1;
            Random r = new Random();
            int hashCode = this.hashFunction(srcItem);
            int startHashCode = hashCode;
            int i = 0;
            while ((this.table[hashCode] != null &&
                    this.table[hashCode].Value.CompareTo(srcItem) != 0)
                    || this.table[hashCode] == null)    // элемент не найден
            {
                int rInt = r.Next(1, this.Nm);
                hashCode = (startHashCode + rInt) % this.Nm;
                IterCount += 1;
                if (hashCode == startHashCode || i >= 10000) // невозможно найти
                {
                    return null;
                }
                i++;
            }

            return this.table[hashCode];
        }

        private int hashFunction(string inputValue)
        {
            // базовая хеш функция - длина строки * (код первого + код последнего символа)
            int first = (int)inputValue[0];
            int last = (int)inputValue[inputValue.Length - 1];
            return (first + last) % this.Nm;
        }
    }
}
