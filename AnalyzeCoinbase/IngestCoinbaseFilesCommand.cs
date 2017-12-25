using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace AnalyzeCoinbase
{
    public class IngestCoinbaseFilesCommand
    {
        public CoinbaseData Execute(string path, string mask)
        {
            var result = new CoinbaseData(); 
            var files = Directory.GetFiles(path, mask, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                if (content.StartsWith("Transactions"))
                {
                    var transactions = ReadCoinbaseTransactionsFile(file);
                    result.TransactionFileRecords.AddRange(transactions); // need to de-dupe them later.
                    
                } else if (content.StartsWith("Cost Basis for Taxes"))
                {
                    var taxes = ReadCoinbaseTaxFile(file);
                    result.TaxFileRecords.AddRange(taxes);  // need to dedupe them later
                }
            }
            // Distinct the things incase overlap between files. 
            result.TransactionFileRecords = result.TransactionFileRecords
                .GroupBy(x => x.CoinbaseID)
                .Select(x => x.FirstOrDefault())
                .OrderBy(x=>x.Timestamp)
                .ToList();

            result.TaxFileRecords = result.TaxFileRecords
                .GroupBy(x => new {x.ReceivedTransactionID, x.SentTransactionID, x.SentTotal})
                .Select(x => x.FirstOrDefault())
                .OrderBy(x=>x.SentDate)
                .ThenBy(x=>x.ReceivedDate)
                .ToList(); 

            return result; 
        }

        public class CoinbaseData
        {
            public CoinbaseData()
            {
                TaxFileRecords = new List<CoinbaseTaxFileRecord>();
                TransactionFileRecords = new List<CoinbaseTransactionFileRecord>();
            }
            public List<CoinbaseTaxFileRecord> TaxFileRecords { get; set; }
            public List<CoinbaseTransactionFileRecord> TransactionFileRecords { get; set; }
        }

        private List<CoinbaseTaxFileRecord> ReadCoinbaseTaxFile(string fileName)
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

        private List<CoinbaseTransactionFileRecord> ReadCoinbaseTransactionsFile(string fileName)
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


    }
}