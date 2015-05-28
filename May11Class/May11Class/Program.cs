using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace May11Class
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> results = ShowResults(16);
            results.ForEach(x => Console.WriteLine(x));
            Console.WriteLine("-----------------------------------------");
            List<Tuple<int, int>> even = GetEven();
            even.ForEach(x => Console.WriteLine(x.ToString()));
            Console.ReadLine();

            flatten();
        }

        public static List<string> ShowResults(int n)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < n; i++)
            {
                temp.Add(i);
            }
            var results = temp.Select(x => x * x)
                .OrderByDescending(x => x)
                .Select(x => x.ToString()).ToList();

            return results;
        }

        public static List<Tuple<int, int>> GetEven()
        {
            var temp = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2, 4, 6, 8, 12, 2, 4, 6 };

            var evenNumberHistogram = temp
                .Where(x => x % 2 == 0)
                .GroupBy(x => x)
                .Select(x => new Tuple<int, int>(x.Key, x.ToList().Count))
                .OrderByDescending(x => x.Item1)
                .ToList();

            return evenNumberHistogram;
        }

        public static List<Tuple<int, int>> GetPrimes()
        {
            var temp = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2, 4, 6, 8, 12, 2, 4, 6 };

            var evenNumberHistogram = temp
                .Where(x => isPrime(x))
                .GroupBy(x => x)
                .Select(x => new Tuple<int, int>(x.Key, x.ToList().Count))
                .OrderByDescending(x => x.Item1)
                .ToList();

            return evenNumberHistogram;
        }

        public static bool isPrime(int n)
        {
            return true;
        }

        public static void flatten()
        {
            var input = new string[] { "hello", "info" };

            var result = input.SelectMany(x => x);
        }

        public static void skip()
        {
            var numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var result = numbers.Skip(5).Take(3);
        }

        public static void obama()
        {
            var file = File.ReadAllLines(@"C:\Users\Bryant Taylor\Desktop");

            var count = file
                .Where(x => x.ToLower().Split(new char[]{ '.', '?', '!', ',', ' ', '\n' }))
                .SelectMany(x => x)
                .Contains("obama");


        }

    }
}
