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
            string[] productNames =
            {
                "toothbrush",
                "apples"
            };
            double[] productPrices =
            {
                0.99,
                1.99,
            };
            double[] itemQuantities =
            {
                1,
                2.5,
                5
            };
            ProductUnit[] productUnits =
            {
                ProductUnit.Each,
                ProductUnit.Kilo
            };
            bool[] useSpecialOfferType =
            {
                true,
                false
            };
            SpecialOfferType[] specialOfferTypes =
            {
                SpecialOfferType.TenPercentDiscount,
                SpecialOfferType.FiveForAmount,
                SpecialOfferType.ThreeForTwo,
                SpecialOfferType.TwoForAmount
            };
            int[] numberOfProductAmounts =
            {
                1,
                2,
                3
            };
            double[] specialOfferArguments =
            {
                10.0
            };

            CombinationApprovals.VerifyAllCombinations(
            CallCheckoutAndGetReceipt,
            productNames,
            productPrices,
            itemQuantities,
            productUnits,
            useSpecialOfferType,
            specialOfferTypes,
            specialOfferArguments,
            numberOfProductAmounts);
        }

        private string CallCheckoutAndGetReceipt(
            string productName,
            double productPrice,
            double itemQuantity,
            ProductUnit productUnit,
            bool useSpecialOffer,
            SpecialOfferType specialOfferType,
            double specialOfferArgument,
            int numberOfProducts)
        {
            var catalog = new FakeCatalog();
            var cart = new ShoppingCart();
            var teller = new Teller(catalog);

            for (int i = 0; i < numberOfProducts; i++)
            {
                var product = new Product($"{productName}-{i}", productUnit);
                catalog.AddProduct(product, productPrice);
                cart.AddItem(product);
                cart.AddItemQuantity(product, itemQuantity);
                if (useSpecialOffer)
                {
                    teller.AddSpecialOffer(specialOfferType, product, specialOfferArgument);
                }
            }

            var receipt = teller.CheckOutArticlesFrom(cart);
            return new ReceiptPrinter().PrintReceipt(receipt);
        }
    }
}