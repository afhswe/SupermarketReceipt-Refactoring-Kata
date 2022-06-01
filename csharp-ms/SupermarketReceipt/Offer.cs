namespace SupermarketReceipt
{
    public enum SpecialOfferType
    {
        ThreeForTwo,
        PercentageDiscount
    }

    public class Offer
    {
        public Offer(SpecialOfferType offerType, double argument, Product product)
        {
            OfferType = offerType;
            Argument = argument;
            Product = product;
        }

        public SpecialOfferType OfferType { get; }
        public double Argument { get; }
        public Product Product { get; }

    }
}