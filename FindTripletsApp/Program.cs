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
                        Parallel.For(0, fileString.Length - 3, new ParallelOptions { CancellationToken = cancelToken.Token }, i => {

                            string triplet = fileString.Substring(i, 3);
                            // только алфавитно-цифровые символы в триплете
                            if (Regex.IsMatch(triplet, @"\w{3}")) 
                            {
                                if (fileString.IndexOf(triplet, i + 1) != -1)
                                {
                                    tripletDictionary.AddOrUpdate(triplet, 2, (triplet, x) => x + 1);
                                }
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


        //######################################################################################################
        //######################################################################################################
        //######################################################################################################

        static void nonParallelRegex(string filename)
        {
            StreamReader fs = new StreamReader(filename);
            Regex regex = new Regex(@"(\w{3})(?=.*?\1)", RegexOptions.Singleline | RegexOptions.Compiled);
            string str = fs.ReadToEnd();
            Dictionary<string, int> myDict = new Dictionary<string, int>();

            // Ищем
            Match matchObj = regex.Match(str);
            while (matchObj.Success)
            {

                if (myDict.ContainsKey(matchObj.Value))
                    myDict[matchObj.Value]++;
                else
                    myDict[matchObj.Value] = 2;
                matchObj = regex.Match(str, matchObj.Index + 1);
            }

            // Выводим
            var top = (from t in myDict
                     orderby t.Value descending
                     select t).Take(10);

            foreach (var gg in top)
                Console.WriteLine(gg);

            fs.Close();
        }


        static void nonParallelCount(string filename)
        {
            StreamReader streamReader = new StreamReader(filename);
            string fileString = streamReader.ReadToEnd();
            Dictionary<string, int> myDic = new Dictionary<string, int>();

            for (int i = 0; i < fileString.Length - 3; i++)
            {
                string triplet = fileString.Substring(i, 3);
                if (Regex.IsMatch(triplet, @"\w{3}"))
                {
                    if (fileString.IndexOf(triplet, i + 1) != -1)
                    {
                        if (myDic.ContainsKey(triplet))
                            myDic[triplet]++;
                        else
                            myDic[triplet] = 2; ;
                    }
                }
            }

            var top = (from t in myDic
                     orderby t.Value descending
                     select t).Take(10);

            foreach (var gg in top)
                Console.WriteLine(gg);

            streamReader.Close();
        }

    }
}
