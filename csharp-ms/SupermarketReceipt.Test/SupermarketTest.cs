using System.Text.Json;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using System.Text.Json.Serialization;
using Xunit;
using FluentAssertions;
using Moq;

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
            var product = new Product($"chocolate", productUnit);
            var catalogStub = new Mock<ISupermarketCatalog>();
            catalogStub.Setup(c => c.GetUnitPrice(product)).Returns(1.00);

            var notificationServiceMock = new Mock<INotificationService>();
            var teller = new Teller(catalogStub.Object, notificationServiceMock.Object);

            var cart = new ShoppingCart();
            cart.AddItem(product);
            cart.AddItemQuantity(product, itemQuantity);
            teller.AddSpecialOffer(specialOfferType, product, specialOfferArgument);

            var receipt = teller.CheckOutArticlesFrom(cart);
            var receiptAsJson = ReceiptAsJson(receipt);

            //catalogStub.Verify(c => c.GetUnitPrice(It.IsAny<Product>()), Times.Exactly(3));
            notificationServiceMock.Verify(notificationService => notificationService.SendReceipt(receipt), Times.Once());
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
}