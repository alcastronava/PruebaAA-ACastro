using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using PruebaAA_ACastro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PruebaAA_ACastro
{
    class Program
    {
        static readonly string url = "https://drive.google.com/uc?export=download&id=1odSfITqFQjOODr29dz-soWFlvHk4LL2H"; //aprox 10 MB
        //static readonly string url = "https://storage10082020.blob.core.windows.net/y9ne9ilzmfld/Stock.CSV"; //aprox 637 MB
        static readonly string destFileName = "Stock.csv";
        static string sTotal = string.Empty;

        static void Main(string[] args)
        {

            Console.WriteLine("Descargando fichero CSV - {0}", url);

            try
            {
                //Descarga del fichero usando webclient
                using (var client = new WebClient())
                {
                    Uri fileAddress = new Uri(url);

                    //Eventos para progress y descarga completada
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFinishedCallback);

                    //Inicio de la descarga
                    client.DownloadFileAsync(fileAddress, destFileName);

                    //Mientras se este haciendo la descarga
                    while (client.IsBusy) { }

                    ProcessCSVFile(destFileName);
                }

                //bool res = ProcessCSVFile(destFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Recibe una longitud en bytes y devuelve un string para mostrar el tamaño de facil lectura
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        private static string GetBytesSizeToString(long len)
        {
            if (len <= 0)
            {
                return "0 (unknown)";
            }
            double dLen = len;

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (dLen >= 1024 && order < sizes.Length - 1)
            {
                order++;
                dLen /= 1024;
            }

            return String.Format("{0:0.##} {1}", dLen, sizes[order]);
        }

        /// <summary>
        /// Evento de finalización de descarga
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DownloadFinishedCallback(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("Descarga de fichero CSV terminada.");

            //ProcessCSVFile(destFileName);
        }

        /// <summary>
        /// Evento de progreso de descarga
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            string sReceived = GetBytesSizeToString(e.BytesReceived).PadRight(10);

            if (string.IsNullOrEmpty(sTotal))
                sTotal = GetBytesSizeToString(e.TotalBytesToReceive).PadRight(10);

            string sPercent = e.ProgressPercentage > 100 ? "100" : e.ProgressPercentage.ToString();

            Console.Write("\rDescargando {0} de {1} bytes. {2} %", sReceived, sTotal, sPercent);
        }

        /// <summary>
        /// Vacia la tabla Stocks
        /// </summary>
        /// <returns></returns>
        private static async Task TruncateTable()
        {
            using (PruebaAAContext context = new PruebaAAContext())
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "TRUNCATE TABLE Stocks";
                        var result = await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Lee cada una de las lineas del archivo, separa los campos del fichero CSV, crea el modelo y lo almacena en BD
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static void ProcessCSVFile(string fileName)
        {
           
            //Verificar si existe el fichero
            if (!File.Exists(fileName))
            {
                Console.WriteLine("No existe el fichero CSV.");
                return;
            }

            Console.WriteLine("Procesando fichero CSV.");

            try
            {
                //Determinar la cantidad de tiempo para procesar el archivo
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                var lineCount = File.ReadAllLines(fileName).Length;
                Console.WriteLine("Total de lineas {0}.", lineCount);

                //Se crea el TextFieldParser para leer cada linea del fichero CSV
                using (TextFieldParser parser = new TextFieldParser(fileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");
                    parser.ReadLine(); //Descartar la primera linea

                    PruebaAAContext context = new PruebaAAContext();

                    //Se vacia la tabla con data anterior
                    TruncateTable().Wait();

                    Console.WriteLine("Leyendo fichero y guardando registros...");

                    int batchSize = 100; //Define la cantidad de lineas a procesar para guardar en BD
                    int i = 0;

                    List<Stock> list = new List<Stock>();

                    //while (i < 10000)
                    while (!parser.EndOfData)
                    {
                        int percent = (int)Math.Round((double)(100 * i) / lineCount);
                        Console.Write("\rLinea {0} - {1} %", i+1, percent);

                        if (i != 0 && i % batchSize == 0) //Cada vez que se llega al tamaño del lineas a procesar se guardan los cambios y se crea un nuevo DBContext
                        {
                            context.Stocks.AddRange(list);
                            context.SaveChanges();
                            context = new PruebaAAContext();
                            context.ChangeTracker.DetectChanges();
                            list.Clear();
                        }

                        //Se lee una linea de fichero
                        string[] fields = parser.ReadFields();

                        if (fields != null && fields.Length == 4)
                        {
                            //Se crea el modelo con los campos de la linea
                            Stock stock = new Stock
                            {
                                PointOfSale = fields[0],
                                Product = fields[1],
                                Date = fields[2], //DateTime.ParseExact(fields[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                StockQty = int.Parse(fields[3])
                            };

                            //Se agrega a la lista que va al AddRange
                            list.Add(stock);
                        }

                        i++;
                    }

                    context.Stocks.AddRange(list);

                    context.ChangeTracker.DetectChanges();
                        
                    context.SaveChanges();

                    list.Clear();
                }

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;

                Console.WriteLine();
                Console.WriteLine("Fichero completado.");

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
