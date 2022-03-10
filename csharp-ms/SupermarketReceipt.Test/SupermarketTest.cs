using System;
using System.Collections.Generic;
using System.Globalization;
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
        public void TotalPrice_ShouldBeZero_ForEmptyCart()
        {
            var sut = new Teller(null, null);
            ShoppingCart cart = new ShoppingCart();
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(0.0);
            receipt.GetDiscounts().Should().HaveCount(0);
            receipt.GetItems().Should().HaveCount(0);
        }

        [Fact]
        public void ShouldApplyTenPercentDiscount()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product product = new Product("chocolate", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(product)).Returns(1);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);
            ShoppingCart cart = new ShoppingCart();
            cart.AddItem(product);
            sut.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, product, 10);
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(0.9);
            receipt.GetDiscounts().Should().Contain(discount => discount.Description.Equals("10% off"));
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(1);
            receipt.GetItems()[0].TotalPrice.Should().Be(1);
            receipt.GetItems()[0].Product.Should().BeSameAs(product);

            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(product), Times.Exactly(2));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Never);
        }

        [Fact]
        public void ShouldProvideReceiptWithCheckedOutItems()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product product = new Product("chocolate", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(product)).Returns(1);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);
            ShoppingCart cart = new ShoppingCart();
            cart.AddItem(product);
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(1);
            receipt.GetDiscounts().Should().HaveCount(0);
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(1);
            receipt.GetItems()[0].TotalPrice.Should().Be(1);
            receipt.GetItems()[0].Product.Should().BeSameAs(product);
            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(product), Times.Exactly(1));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Never);
        }


        [Fact]
        public void ShouldSendReceipt_ForTwoOrMoreItems()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product chocolate = new Product("chocolate", ProductUnit.Each);
            Product gin = new Product("gin", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(chocolate)).Returns(1);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(gin)).Returns(50);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);

            ShoppingCart cart = new ShoppingCart();
            cart.AddItemQuantity(chocolate, 1);
            cart.AddItemQuantity(gin, 1);
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(51);
            receipt.GetDiscounts().Should().HaveCount(0);
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(1);
            receipt.GetItems()[0].TotalPrice.Should().Be(1);
            receipt.GetItems()[0].Product.Should().BeSameAs(chocolate);
            receipt.GetItems()[1].Price.Should().Be(50);
            receipt.GetItems()[1].Quantity.Should().Be(1);
            receipt.GetItems()[1].TotalPrice.Should().Be(50);
            receipt.GetItems()[1].Product.Should().BeSameAs(gin);

            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(chocolate), Times.Exactly(1));
            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(gin), Times.Exactly(1));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Once);
        }

        [Fact]
        public void ShouldNotSendReceipt_ForLessThanTwoItems()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product chocolate = new Product("chocolate", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(chocolate)).Returns(1);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);

            ShoppingCart cart = new ShoppingCart();
            cart.AddItemQuantity(chocolate, 1);
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(1);
            receipt.GetDiscounts().Should().HaveCount(0);
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(1);
            receipt.GetItems()[0].TotalPrice.Should().Be(1);
            receipt.GetItems()[0].Product.Should().BeSameAs(chocolate);

            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(chocolate), Times.Exactly(1));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Never);
        }

        [Fact]
        public void ShouldChargeForTwoItems_GivenThreeItems_AndThreeForTwoDiscount()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product chocolate = new Product("chocolate", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(chocolate)).Returns(1);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);
            ShoppingCart cart = new ShoppingCart();
            sut.AddSpecialOffer(SpecialOfferType.ThreeForTwo, chocolate, 10);

            cart.AddItemQuantity(chocolate, 3);
            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().Be(2);
            receipt.GetDiscounts().Should().HaveCount(1);
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(3);
            receipt.GetItems()[0].TotalPrice.Should().Be(3);
            receipt.GetItems()[0].Product.Should().BeSameAs(chocolate);

            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(chocolate), Times.Exactly(2));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Never);
        }

        [Fact]
        public void ShouldApplyTenPercentDiscount_And_FiveForAmountDiscount()
        {
            var supermarketCatalog = new Mock<ISupermarketCatalog>();
            Product chocolate = new Product("chocolate", ProductUnit.Each);
            Product beer = new Product("beer", ProductUnit.Each);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(chocolate)).Returns(1);
            supermarketCatalog.Setup(catalog => catalog.GetUnitPrice(beer)).Returns(2.50);

            var notificationService = new Mock<INotificationService>();
            var sut = new Teller(supermarketCatalog.Object, notificationService.Object);
            sut.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, chocolate, 10);
            sut.AddSpecialOffer(SpecialOfferType.FiveForAmount, beer, 1);

            ShoppingCart cart = new ShoppingCart();
            cart.AddItemQuantity(chocolate, 1);
            cart.AddItemQuantity(beer, 5);

            var receipt = sut.CheckOutArticlesFrom(cart);
            receipt.GetTotalPrice().Should().BeInRange(1.9, 1.91);
            receipt.GetDiscounts().Should().HaveCount(2);
            receipt.GetItems()[0].Price.Should().Be(1);
            receipt.GetItems()[0].Quantity.Should().Be(1);
            receipt.GetItems()[0].TotalPrice.Should().Be(1);
            receipt.GetItems()[0].Product.Should().BeSameAs(chocolate);
            receipt.GetItems()[1].Price.Should().Be(2.5);
            receipt.GetItems()[1].Quantity.Should().Be(5);
            receipt.GetItems()[1].TotalPrice.Should().Be(12.5);
            receipt.GetItems()[1].Product.Should().BeSameAs(beer);

            supermarketCatalog.Verify(catalog => catalog.GetUnitPrice(chocolate), Times.Exactly(2));
            notificationService.Verify(service => service.SendReceipt(receipt), Times.Once);
        }
    }
}