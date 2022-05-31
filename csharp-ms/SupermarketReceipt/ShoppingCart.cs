using System.Collections.Generic;

namespace SupermarketReceipt
{
    public class ShoppingCart
    {
        private readonly List<ProductQuantity> items = new List<ProductQuantity>();
        private readonly Dictionary<Product, double> productQuantities = new Dictionary<Product, double>();


        public List<ProductQuantity> GetItems()
        {
            return new List<ProductQuantity>(items);
        }

        public void AddItem(Product product)
        {
            if (product.Name.Length < 3)
            {
                throw new InvalidProductNameException($"Product name {product.Name} invalid, has to be at least 2 characters");
            }
            AddItemQuantity(product, 1.0);
        }


        public void AddItemQuantity(Product product, double quantity)
        {
            items.Add(new ProductQuantity(product, quantity));
            if (productQuantities.ContainsKey(product))
            {
                var newAmount = productQuantities[product] + quantity;
                productQuantities[product] = newAmount;
            }
            else
            {
                productQuantities.Add(product, quantity);
            }
        }

        public Dictionary<Product, double> GetProductQuantities()
        {
            return productQuantities;
        }
    }
}