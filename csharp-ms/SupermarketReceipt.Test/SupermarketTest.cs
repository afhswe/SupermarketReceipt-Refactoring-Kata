using System.Collections.Generic;
using System.Text.Json;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using System.Text.Json.Serialization;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SupermarketReceipt.Test
{
    public class SupermarketTest
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        public void ApproveReceiptCalculation()
        {
            var itemQuantities = new double[]
            {
                1,
                5
            };
            var productUnits = new[]
            {
                ProductUnit.Each,
                ProductUnit.Kilo
            };
            var specialOfferTypes = new[]
            {
                SpecialOfferType.TenPercentDiscount,
                SpecialOfferType.FiveForAmount,
                SpecialOfferType.ThreeForTwo,
                SpecialOfferType.TwoForAmount
            };
            var specialOfferArguments = new[]
            {
                10.0
            };

            CombinationApprovals.VerifyAllCombinations(
                CallCheckoutAndGetReceipt,
                itemQuantities,
                productUnits,
                specialOfferTypes,
                specialOfferArguments);
        }

        private string CallCheckoutAndGetReceipt(
            double itemQuantity,
            ProductUnit productUnit,
            SpecialOfferType specialOfferType,
            double specialOfferArgument)
        {
            var catalog = new FakeCatalog();
            var cart = new ShoppingCart();
            var teller = new Teller(catalog);

            var product = new Product($"chocolate", productUnit);
            catalog.AddProduct(product, 1.00);
            cart.AddItem(product);
            cart.AddItemQuantity(product, itemQuantity);
            teller.AddSpecialOffer(specialOfferType, product, specialOfferArgument);

            var receipt = teller.CheckOutArticlesFrom(cart);
            var receiptAsJson = ReceiptAsJson(receipt);
            return receiptAsJson;
        }

        private string ReceiptAsJson(Receipt receipt)
        {
            var receiptForTest = new ReceiptDataForPrinting();
            receiptForTest.Discounts = receipt.GetDiscounts();
            receiptForTest.TotalPrice = receipt.GetTotalPrice();
            receiptForTest.Items = receipt.GetItems();
            receiptForTest.SpecialOffer = SpecialOfferType.TenPercentDiscount;
            return JsonSerializer.Serialize(
                receiptForTest,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                });
        }
    }

    public class ReceiptDataForPrinting
    {
        public double TotalPrice { get; set; }
        public SpecialOfferType SpecialOffer { get; set; }
        public List<Discount> Discounts { get; set; }
        public List<ReceiptItem> Items { get; set; }
    }
}