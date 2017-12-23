using System;
using CsvHelper.Configuration;

namespace AnalyzeCoinbase
{
    public class CoinbaseTransactionFileRecord
    {
        public DateTime Timestamp { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TransferTotal { get; set; }
        public decimal? TransferFee { get; set; }
        public string CoinbaseID { get; set; }

    }

    public sealed class CoinbaseTransactionFileRecordMap : ClassMap<CoinbaseTransactionFileRecord>
    {
        public CoinbaseTransactionFileRecordMap()
        {
            AutoMap();
            Map(m => m.TransferTotal).Name("Transfer Total");
            Map(m => m.TransferFee).Name("Transfer Fee");
            Map(m => m.CoinbaseID).Name("Coinbase ID (visit https://www.coinbase.com/transactions/[ID] in your browser)");
        }
    }
}