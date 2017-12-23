using System;
using CsvHelper.Configuration;

namespace AnalyzeCoinbase
{
    public class CoinbaseTaxFileRecord
    {
        public DateTime SentDate { get; set; }        
        public decimal? SentTotal { get; set; }
        public decimal? SentQuantity { get; set; }
        public string SentTransactionID { get; set; }
    }

    public sealed class CoinbaseTaxFileRecordMap : ClassMap<CoinbaseTaxFileRecord>
    {
        public CoinbaseTaxFileRecordMap()
        {
            AutoMap();
            Map(m => m.SentDate).Name("Sent Date");
            Map(m => m.SentTotal).Name("Sent Total (USD)");
            Map(m => m.SentQuantity).Name("Sent Quantity (BTC)");
            Map(m => m.SentTransactionID).Name("Sent Transaction ID");
        }
    }


}