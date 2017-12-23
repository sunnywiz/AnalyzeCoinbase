using System;

namespace AnalyzeCoinbase
{
    internal class MyOutputRecord
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