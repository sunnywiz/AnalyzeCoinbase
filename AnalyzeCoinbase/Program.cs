using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace AnalyzeCoinbase
{
    class Program
    {
        private const string TransactionsFile = "Coinbase-54c962face4355ecd0000004-Transactions-Report-2017-12-21-00_57_59.csv";
        private const string OutputFile = "AnalyzeCoinbaseOutput.csv";

        static void Main(string[] args)
        {
            try
            {
                using (var streamReader = new StreamReader(TransactionsFile))
                using (var streamWriter = new StreamWriter(OutputFile, false))
                using (var csvReader = new CsvReader(streamReader))
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    csvReader.Configuration.RegisterClassMap<CoinbaseTransactionFileRecordMap>();
                    ReadUntilHeader(csvReader);
                    var coinbaseRecords = csvReader.GetRecords<CoinbaseTransactionFileRecord>().OrderBy(x=>x.Timestamp).ToList();
                    if (coinbaseRecords.Count == 0) throw new NotSupportedException("must be at least one transaction");
                    var outputRecords = CookRecords(coinbaseRecords);
                    csvWriter.WriteRecords(outputRecords); 
                }
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

        private static List<MyOutputRecord> CookRecords(List<CoinbaseTransactionFileRecord> coinbaseRecords)
        {
            var current = new MyOutputRecord()
            {
                Timestamp = coinbaseRecords.First().Timestamp,
                BankBalance = 0m,
                CryptoBalance = 0m,
                GoodsBalance = 0m
            };
            var outputRecords = new List<MyOutputRecord>();
            foreach (var cr in coinbaseRecords)
            {
                var r = new MyOutputRecord() {Timestamp = cr.Timestamp};

                var sign = Math.Sign(cr.Amount??0m);
                // + = bought crypto
                // - = sold crypto; got bank

                r.ToCrypto = cr.Amount??0m;

                if (cr.TransferTotal.HasValue && cr.TransferFee.HasValue)
                {
                    if (sign == 1)
                    {
                        // bought crypto -- lost from bank that got converted
                        r.ToBank = -sign * (cr.TransferTotal.Value - cr.TransferFee.Value);
                        // but the fees just rack up
                        r.ToFee = cr.TransferFee.Value; 
                    } else if (sign == -1)
                    {
                        // sold crypto -- something got to bank
                        r.ToBank = cr.TransferTotal.Value;
                        r.ToFee = cr.TransferFee.Value; 
                    }
                }

                r.CryptoBalance = current.CryptoBalance + r.ToCrypto;
                r.BankBalance = current.BankBalance + r.ToBank;
                r.FeeBalance = current.FeeBalance + r.ToFee;
                r.GoodsBalance = current.GoodsBalance + r.ToGoods;

                outputRecords.Add(r);
                current = r;
            }
            return outputRecords;
        }

        private static void ReadUntilHeader(CsvReader csv)
        {
            while (csv.Read())
            {
                if (csv.GetField<string>(0) == "Timestamp")
                {
                    if (!csv.ReadHeader()) throw new NotSupportedException("??");
                    return;
                }
            }
            throw new NotSupportedException("Could not find header");
        }
    }
}
