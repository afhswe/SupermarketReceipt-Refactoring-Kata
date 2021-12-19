using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;
using SupermarketReceipt;
using Xunit;

namespace Supermarket.Test
{
    public class SupermarketTest
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        public void ApproveReceiptCalculation()
        {
            CombinationApprovals.VerifyAllCombinations(
            CallCheckoutAndGetReceipt,
            new double[] {
                1,
                5
            },
            new[] {
                ProductUnit.Each,
                ProductUnit.Kilo
            },
            new[] {
                SpecialOfferType.TenPercentDiscount,
                SpecialOfferType.FiveForAmount,
                SpecialOfferType.ThreeForTwo,
                SpecialOfferType.TwoForAmount
            },
            new[] {
                10.0
            });
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
            return new ReceiptPrinter().PrintReceipt(receipt);
        }
    }
}