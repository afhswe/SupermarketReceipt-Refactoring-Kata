using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketReceipt
{
    public class Teller
    {
        private readonly ISupermarketCatalog catalog;
        public readonly List<Offer> Offers = new List<Offer>();
        private readonly INotificationService notificationService;

        public Teller(ISupermarketCatalog catalog, INotificationService notificationService)
        {
            this.catalog = catalog;
            this.notificationService = notificationService;
        }

        public void AddSpecialOffer(SpecialOfferType offerType, Product product, double argument)
        {
            Offers.Add(new Offer(offerType, argument, product));
        }

        public Receipt CheckOutArticlesFrom(ShoppingCart theCart)
        {
            var receipt = new Receipt();
            var productQuantities = theCart.GetItems();
            foreach (var pq in productQuantities)
            {
                var p = pq.Product;
                var quantity = pq.Quantity;
                var unitPrice = catalog.GetUnitPrice(p);
                var price = quantity * unitPrice;
                receipt.AddProduct(p, quantity, unitPrice, price);
            }

            HandleOffers(theCart, receipt);

            if (theCart.GetItems().Count >= 2)
            {
                notificationService.SendReceipt(receipt);
            }

            return receipt;
        }

        private void HandleOffers(ShoppingCart theCart, Receipt receipt)
        {
            foreach (var product in theCart.GetProductQuantities().Keys)
            {
                var quantity = theCart.GetProductQuantities()[product];
                var quantityAsInt = (int) quantity;
                if (Offers.Find(o => o.Product.Name == product.Name) != null)
                {
                    var offer = Offers.Find(o => o.Product.Name == product.Name);
                    var unitPrice = catalog.GetUnitPrice(product);
                    Discount discount = null;
                    var x = 1;
                    if (offer.OfferType == SpecialOfferType.ThreeForTwo)
                    {
                        x = 3;
                    }

                    var numberOfXs = quantityAsInt / x;
                    if (offer.OfferType == SpecialOfferType.ThreeForTwo && quantityAsInt > 2)
                    {
                        var discountAmount = quantity * unitPrice -
                                             (numberOfXs * 2 * unitPrice + quantityAsInt % 3 * unitPrice);
                        discount = new Discount(product, "3 for 2", -discountAmount);
                    }

                    if (offer.OfferType == SpecialOfferType.PercentageDiscount)
                        discount = new Discount(product, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0);

                    if (discount != null)
                        receipt.AddDiscount(discount);
                }
            }
        }
    }
}