using System.Collections.Generic;
using System.Linq;

namespace SupermarketReceipt
{
    public class Teller
    {
        private readonly ISupermarketCatalog _catalog;
        private readonly Dictionary<Product, Offer> _offers = new Dictionary<Product, Offer>();
        private INotificationService _notificationService;

        public Teller(ISupermarketCatalog catalog, INotificationService notificationService)
        {
            _catalog = catalog;
            _notificationService = notificationService;
        }

        public void AddSpecialOffer(SpecialOfferType offerType, Product product, double argument)
        {
            _offers[product] = new Offer(offerType, product, argument);
        }

        public Receipt CheckOutArticlesFrom(ShoppingCart theCart)
        {
            var receipt = new Receipt();
            var productQuantities = theCart.GetItems();
            foreach (var pq in productQuantities)
            {
                var p = pq.Product;
                var quantity = pq.Quantity;
                var unitPrice = _catalog.GetUnitPrice(p);
                var price = quantity * unitPrice;
                receipt.AddProduct(p, quantity, unitPrice, price);
            }

            HandleOffers(theCart, receipt);

            if (theCart.GetItems().Count >= 2)
            {
                _notificationService.SendReceipt(receipt);
            }

            return receipt;
        }

        private void HandleOffers(ShoppingCart theCart, Receipt receipt)
        {
            foreach (var p1 in theCart.GetProductQuantities().Keys)
            {
                var quantity = theCart.GetProductQuantities()[p1];
                var quantityAsInt = (int) quantity;
                if (_offers.ContainsKey(p1))
                {
                    var offer = _offers[p1];
                    var unitPrice = _catalog.GetUnitPrice(p1);
                    Discount discount = null;
                    var x = 1;
                    if (offer.OfferType == SpecialOfferType.ThreeForTwo)
                    {
                        x = 3;
                    }
                    else if (offer.OfferType == SpecialOfferType.TwoForAmount)
                    {
                        x = 2;
                        if (quantityAsInt >= 2)
                        {
                            var total = offer.Argument * (quantityAsInt / x) + quantityAsInt % 2 * unitPrice;
                            var discountN = unitPrice * quantity - total;
                            discount = new Discount(p1, "2 for " + offer.Argument, -discountN);
                        }
                    }

                    if (offer.OfferType == SpecialOfferType.FiveForAmount) x = 5;
                    var numberOfXs = quantityAsInt / x;
                    if (offer.OfferType == SpecialOfferType.ThreeForTwo && quantityAsInt > 2)
                    {
                        var discountAmount = quantity * unitPrice -
                                             (numberOfXs * 2 * unitPrice + quantityAsInt % 3 * unitPrice);
                        discount = new Discount(p1, "3 for 2", -discountAmount);
                    }

                    if (offer.OfferType == SpecialOfferType.TenPercentDiscount)
                        discount = new Discount(p1, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0);
                    if (offer.OfferType == SpecialOfferType.FiveForAmount && quantityAsInt >= 5)
                    {
                        var discountTotal = unitPrice * quantity -
                                            (offer.Argument * numberOfXs + quantityAsInt % 5 * unitPrice);
                        discount = new Discount(p1, x + " for " + offer.Argument, -discountTotal);
                    }

                    if (discount != null)
                        receipt.AddDiscount(discount);
                }
            }
        }
    }
}