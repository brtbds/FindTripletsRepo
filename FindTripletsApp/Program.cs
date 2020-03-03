using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace FindTripletsApp
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("File name:");
            string fileName = Console.ReadLine();

            Stopwatch sw = Stopwatch.StartNew();
            ParallelCount(fileName);
            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }


        static void ParallelCount(string filename)
        {
            using (StreamReader streamReader = new StreamReader(filename))
            {
                // <триплет, число повторений>
                ConcurrentDictionary<string, int> tripletDictionary = new ConcurrentDictionary<string, int>();
                string fileString = streamReader.ReadToEnd();

                if (fileString.Length < 4)
                {
                    Console.WriteLine("File length must be >= 4");
                    return;
                }

                using (CancellationTokenSource cancelToken = new CancellationTokenSource())
                {
                    // останавливаем при нажатии клавиши
                    Task.Factory.StartNew(() =>
                    {
                        Console.ReadKey();
                        Console.WriteLine();
                        cancelToken.Cancel();
                    });

                    try
                    {
                        Parallel.For(0, fileString.Length - 3, new ParallelOptions { CancellationToken = cancelToken.Token }, i =>
                        {
                            string triplet = fileString.Substring(i, 3);
                            if (Regex.IsMatch(triplet, @"\w{3}"))
                            {
                                tripletDictionary.AddOrUpdate(triplet, 1, (triplet, x) => x + 1);
                            }
                        });
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine("Operation canceled");
                    }

                    // вывод топ 10
                    var top = (from t in tripletDictionary
                               orderby t.Value descending
                               select t).Take(10);
                    foreach (var each in top)
                        Console.WriteLine(each);
                }
            }
        }
    }
}
