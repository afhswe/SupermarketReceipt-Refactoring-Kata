using System;
using System.Collections.Generic;

namespace SupermarketReceipt
{
    public class Receipt
    {
        private readonly List<Discount> discounts = new List<Discount>();
        private readonly List<ReceiptItem> items = new List<ReceiptItem>();

        public double GetTotalPrice()
        {
            var total = 0.0;
            foreach (var item in items) total += item.TotalPrice;
            foreach (var discount in discounts) total += discount.DiscountAmount;
            return total;
        }

        public void AddProduct(Product p, double quantity, double price, double totalPrice)
        {
            items.Add(new ReceiptItem(p, quantity, price, totalPrice));
        }

        public List<ReceiptItem> GetItems()
        {
            return items;
        }

        public void AddDiscount(Discount discount)
        {
            discounts.Add(discount);
        }

        public List<Discount> GetDiscounts()
        {
            return discounts;
        }
    }

    public class ReceiptItem
    {
        public ReceiptItem(Product p, double quantity, double price, double totalPrice)
        {
            Product = p;
            Quantity = quantity;
            Price = price;
            TotalPrice = totalPrice;
        }

        public Product Product { get; }
        public double Price { get; }
        public double TotalPrice { get; }
        public double Quantity { get; }
    }
}