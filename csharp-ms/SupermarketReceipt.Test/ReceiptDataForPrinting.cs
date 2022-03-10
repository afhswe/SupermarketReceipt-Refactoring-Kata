using System.Collections.Generic;

namespace SupermarketReceipt.Test
{
    public class ReceiptDataForPrinting
    {
        public double TotalPrice { get; set; }
        public SpecialOfferType SpecialOffer { get; set; }
        public List<Discount> Discounts { get; set; }
        public List<ReceiptItem> Items { get; set; }
    }
}