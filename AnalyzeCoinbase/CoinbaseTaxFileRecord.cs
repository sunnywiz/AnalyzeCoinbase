using System;
using CsvHelper.Configuration;

namespace AnalyzeCoinbase
{
    public class CoinbaseTaxFileRecord
    {
        public DateTime ReceivedDate { get; set; }
        public DateTime SentDate { get; set; }        
        public decimal? SentTotal { get; set; }
        public string SentDescription { get; set; }
        public string ReceivedTransactionID { get; set; }
        public string SentTransactionID { get; set; }
    }

    public sealed class CoinbaseTaxFileRecordMap : ClassMap<CoinbaseTaxFileRecord>
    {
        public CoinbaseTaxFileRecordMap()
        {
            AutoMap();
            Map(m => m.ReceivedDate).Name("Received Date");
            Map(m => m.SentDate).Name("Sent Date");
            Map(m => m.SentTotal).Name("Sent Total (USD)");
            Map(m => m.SentTransactionID).Name("Sent Transaction ID");
            Map(m => m.ReceivedTransactionID).Name("Received Transaction ID");
            Map(m => m.SentDescription).Name("Sent Description");
        }
    }


}