using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ParallelForRead
{
    class Program
    {
        static readonly string destFileName = "Stock.csv";

        static void Main(string[] args)
        {
            ProcessCSVFile(destFileName);
        }

        private static void ProcessCSVFile(string fileName)
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                int i = 0;

                Parallel.ForEach(File.ReadLines(fileName), (line, _, lineNumber) =>
                {
                    //Console.Write("\rLinea {0}", lineNumber);

                    string[] fields = line.Split(";");
                    int res;
                    if (int.TryParse(fields[3], out res))
                    {
                        Stock stock = new Stock
                        {
                            PointOfSale = fields[0],
                            Product = fields[1],
                            Date = DateTime.ParseExact(fields[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                            StockQty = int.Parse(fields[3])
                        };
                        i++;
                    }
                });

                stopWatch.Stop();

                Console.WriteLine();
                Console.WriteLine("Fichero completado.");

                Console.WriteLine("Total Lineas {0}", i);

                TimeSpan ts = stopWatch.Elapsed;

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Console.WriteLine("Tiempo total: " + elapsedTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
