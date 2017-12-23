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
        private const string TaxesFile = "Coinbase-54c962face4355ecd0000004-Taxes-Report-2017-12-18-16_16_37.csv";
        private const string OutputFile = "AnalyzeCoinbaseOutput.csv";

        static void Main(string[] args)
        {
            try
            {
                var transactions = ReadCoinbaseTransactions(TransactionsFile);

                if (transactions.Count == 0)
                    throw new NotSupportedException("must be at least one transaction");

                var taxes = ReadCoinbaseTaxFile(TaxesFile);

                var outputRecords = CookRecords(transactions);

                WriteOutputFile(outputRecords);
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

        private static void WriteOutputFile(List<MyOutputRecord> outputRecords)
        {
            using (var outputStreamWriter = new StreamWriter(OutputFile, false))
            using (var csvOutputWriter = new CsvWriter(outputStreamWriter))
            {
                csvOutputWriter.WriteRecords(outputRecords);
            }
        }

        private static List<CoinbaseTransactionFileRecord> ReadCoinbaseTransactions(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            using (var csv = new CsvReader(sr))
            {
                csv.Configuration.RegisterClassMap<CoinbaseTransactionFileRecordMap>();
                while (csv.Read())
                {
                    if (csv.GetField<string>(0) == "Timestamp")
                    {
                        if (!csv.ReadHeader()) throw new NotSupportedException("??");
                        var records = csv.GetRecords<CoinbaseTransactionFileRecord>()
                            .OrderBy(x => x.Timestamp).ToList();
                        return records;
                    }
                }
                throw new NotSupportedException("Could not find header");
            }
        }

        private static List<CoinbaseTaxFileRecord> ReadCoinbaseTaxFile(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            using (var csv = new CsvReader(sr))
            {
                csv.Configuration.RegisterClassMap<CoinbaseTaxFileRecordMap>();
                while (csv.Read())
                {
                    if (csv.GetField<string>(0) == "Received Transaction ID")
                    {
                        if (!csv.ReadHeader()) throw new NotSupportedException("??");
                        var records = csv.GetRecords<CoinbaseTaxFileRecord>()
                            .OrderBy(x => x.SentDate).ToList();
                        return records;
                    }
                }
                throw new NotSupportedException("Could not find header");
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
                var r = new MyOutputRecord() { Timestamp = cr.Timestamp };

                var sign = Math.Sign(cr.Amount ?? 0m);
                // + = bought crypto
                // - = sold crypto; got bank

                r.ToCrypto = cr.Amount ?? 0m;

                if (cr.TransferTotal.HasValue && cr.TransferFee.HasValue)
                {
                    if (sign == 1)
                    {
                        // bought crypto -- lost from bank that got converted
                        r.ToBank = -sign * (cr.TransferTotal.Value - cr.TransferFee.Value);
                        // but the fees just rack up
                        r.ToFee = cr.TransferFee.Value;
                    }
                    else if (sign == -1)
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

    }
}
