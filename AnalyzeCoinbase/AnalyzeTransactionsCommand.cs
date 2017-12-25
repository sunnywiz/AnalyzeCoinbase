using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzeCoinbase
{
    public class AnalyzeTransactionsCommand
    {
        public List<MyOutputRecord> Execute(IngestCoinbaseFilesCommand.CoinbaseData coinbaseData, string currency)
        {
            List<CoinbaseTransactionFileRecord> coinbaseRecords = coinbaseData.TransactionFileRecords.Where(x=>x.Currency == currency).OrderBy(x=>x.Timestamp).ToList();
            List<CoinbaseTaxFileRecord> taxes = coinbaseData.TaxFileRecords;  // secondary scan, so no problem scanning all of them. 
            var outputRecords = new List<MyOutputRecord>();
            if (!coinbaseRecords.Any()) return outputRecords; 

            var current = new MyOutputRecord()
            {
                Timestamp = coinbaseRecords.First().Timestamp,
                BankBalance = 0m,
                CryptoBalance = 0m,
                GoodsBalance = 0m
            };
            foreach (var cr in coinbaseRecords)
            {
                var r = new MyOutputRecord() { Timestamp = cr.Timestamp };

                var sign = Math.Sign(cr.Amount ?? 0m);
                // + = bought crypto
                // - = sold crypto; got bank

                r.ToCrypto = cr.Amount ?? 0m;

                if (cr.TransferTotal.HasValue && cr.TransferFee.HasValue)
                {
                    // we talked directly to our bank... 
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
                else if (!String.IsNullOrWhiteSpace(cr.CoinbaseID))
                {
                    // there was a transaction done of some sort... 
                    var goodsValue = taxes.Where(t => t.SentTransactionID == cr.CoinbaseID).Sum(t => t.SentTotal);
                    if (goodsValue.HasValue)
                    {
                        r.ToGoods = goodsValue.Value;
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

        public class MyOutputRecord
        {
            public DateTime Timestamp { get; set; }
            public decimal ToBank { get; set; }
            public decimal ToGoods { get; set; }
            public decimal ToCrypto { get; set; }
            public decimal ToFee { get; set; }
            public decimal FeeBalance { get; set; }
            public decimal BankBalance { get; set; }
            public decimal GoodsBalance { get; set; }
            public decimal CryptoBalance { get; set; }
        }

    }
}