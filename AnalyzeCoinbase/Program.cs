using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace AnalyzeCoinbase
{
    class Program
    {
        private const string IngestDirectory = @"C:\Users\Sunny\Downloads\Coinbase";
        private const string IngestFileMask = "*.csv";

        static void Main(string[] args)
        {
            try
            {
                var coinbaseData = new IngestCoinbaseFilesCommand().Execute(IngestDirectory, IngestFileMask);
                var analyzeTransactions = new AnalyzeTransactionsCommand();
                WriteOutputFile("btc.csv",analyzeTransactions.Execute(coinbaseData, "BTC"));
                WriteOutputFile("eth.csv",analyzeTransactions.Execute(coinbaseData, "ETH"));
                WriteOutputFile("ltc.csv",analyzeTransactions.Execute(coinbaseData, "LTC"));
                WriteOutputFile("bch.csv", analyzeTransactions.Execute(coinbaseData, "BCH"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Done");
            }
        }

        private static void WriteOutputFile(string outputFile, List<AnalyzeTransactionsCommand.MyOutputRecord> outputRecords)
        {
            using (var outputStreamWriter = new StreamWriter(outputFile, false))
            using (var csvOutputWriter = new CsvWriter(outputStreamWriter))
            {
                csvOutputWriter.WriteRecords(outputRecords);
            }
        }

    }
}
