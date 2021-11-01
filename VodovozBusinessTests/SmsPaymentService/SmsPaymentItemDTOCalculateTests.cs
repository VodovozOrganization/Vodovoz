using NSubstitute;
using NUnit.Framework;
using SmsPaymentService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Models;

namespace VodovozBusinessTests.SmsPaymentService
{
	[TestFixture]
    public class SmsPaymentItemDTOCalculateTests
    {
        static IEnumerable OrdersTestSource()
        {
            yield return new Order
            {
                OrderItems = new List<OrderItem>
                    {
                        // (3*150-50)/3 = 133,3333333333333
                        new OrderItem {Count = 3, Price = 150, DiscountMoney = 50, Nomenclature = new Nomenclature()},
                        // (5*100)/5 = 100
                        new OrderItem {Count = 5, Price = 100, Nomenclature = new Nomenclature()}
                    }
            };

            yield return new Order
            {
                OrderItems = new List<OrderItem>
                    {
                        // (12*420-340)/12 = 391,6666666666667
                        new OrderItem {Count = 12, Price = 420, DiscountMoney = 340, Nomenclature = new Nomenclature()}
                    }
            };

            yield return new Order
            {
                OrderItems = new List<OrderItem>
                    {
                        // (7*200-400)/7 = 142,8571428571429
                        new OrderItem {Count = 7, Price = 200, DiscountMoney = 400, Nomenclature = new Nomenclature()},
                        // (11*50-50)/11 = 45,45454545454545
                        new OrderItem {Count = 11, Price = 50, DiscountMoney = 50, Nomenclature = new Nomenclature()},
                        // (3*500-500)/3 = 333,3333333333333
                        new OrderItem {Count = 3, Price = 500, DiscountMoney = 500, Nomenclature = new Nomenclature()},
                        // (4*120-10)/4 = 117,5
                        new OrderItem {Count = 4, Price = 120, DiscountMoney = 10, Nomenclature = new Nomenclature()},
                        // (8*500)/8 = 500
                        new OrderItem {Count = 8, Price = 500, Nomenclature = new Nomenclature()}
                    }
            };
        }

        [Test(Description = "Сумма распределённых по номенклатурам цен*количество равна сумме заказа")]
        [TestCaseSource(nameof(OrdersTestSource))]
        public void SmsPaymentItemsPriceSumEqualOrderSum(Order order)
        {
            // arrange
            var organizationProvidertMock = Substitute.For<IOrganizationProvider>();
            var smsPaymentMock = Substitute.For<SmsPayment>();
            var uowMock = Substitute.For<IUnitOfWork>();
            var paymentFromMock = Substitute.For<PaymentFrom>();
            
            SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);
            
            // act

            var smsPaymentDTO = smsPaymentDTOFactory.CreateSmsPaymentDTO(uowMock, smsPaymentMock, order, paymentFromMock);
            var result = smsPaymentDTO.Items.Sum(x => x.Price * x.Quantity);

            // assert

            Assert.That(result, Is.EqualTo(order.OrderPositiveSum));
        }

        [Test(Description = "Распределение цен по номенклатурам (число без дроби)")]
        public void SmsPaymentPriceForNomenclatureDistributionsWithoutFraction()
        {
            // arrange
            var organizationProvidertMock = Substitute.For<IOrganizationProvider>();
            var smsPaymentMock = Substitute.For<SmsPayment>();
            var orderMock = Substitute.For<Order>();
            var uowMock = Substitute.For<IUnitOfWork>();
            var paymentFromMock = Substitute.For<PaymentFrom>();
            
            SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);
            orderMock.OrderItems = new List<OrderItem>
            {
                new OrderItem {Count = 3, Price = 300, Nomenclature = new Nomenclature()},
            };

            // act

            var smsPaymentDTO = smsPaymentDTOFactory.CreateSmsPaymentDTO(uowMock, smsPaymentMock, orderMock, paymentFromMock);

            // assert

            Assert.That(smsPaymentDTO.Items.Count, Is.EqualTo(1));

            Assert.That(smsPaymentDTO.Items[0].Price, Is.EqualTo(300));
            Assert.That(smsPaymentDTO.Items[0].Quantity, Is.EqualTo(3));
        }

        [Test(Description = "Распределение цен по номенклатурам (число с округляемой к меньшему периодической дробью 133.33333...)")]
        public void SmsPaymentPriceForNomenclatureDistributionsWithCirculatingFractionDown()
        {
            // arrange
            var organizationProvidertMock = Substitute.For<IOrganizationProvider>();
            var smsPaymentMock = Substitute.For<SmsPayment>();
            var orderMock = Substitute.For<Order>();
            var uowMock = Substitute.For<IUnitOfWork>();
            var paymentFromMock = Substitute.For<PaymentFrom>();
            
            SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);
            
            orderMock.OrderItems = new List<OrderItem>
            {
                new OrderItem {Count = 3, Price = 150, DiscountMoney = 50, Nomenclature = new Nomenclature()}
            };

            // act

            var smsPaymentDTO = smsPaymentDTOFactory.CreateSmsPaymentDTO(uowMock, smsPaymentMock, orderMock, paymentFromMock);

            // assert

            Assert.That(smsPaymentDTO.Items.Count, Is.EqualTo(2));

            Assert.That(smsPaymentDTO.Items[0].Price, Is.EqualTo(133.33));
            Assert.That(smsPaymentDTO.Items[0].Quantity, Is.EqualTo(2));

            Assert.That(smsPaymentDTO.Items[1].Price, Is.EqualTo(133.34));
            Assert.That(smsPaymentDTO.Items[1].Quantity, Is.EqualTo(1));
        }

        [Test(Description = "Распределение цен по номенклатурам (число с округляемой к большему периодической дробью 142.857142857142...)")]
        public void SmsPaymentPriceForNomenclatureDistributionsWithCirculatingFractionUp()
        {
            // arrange
            var organizationProvidertMock = Substitute.For<IOrganizationProvider>();
            var smsPaymentMock = Substitute.For<SmsPayment>();
            var orderMock = Substitute.For<Order>();
            var uowMock = Substitute.For<IUnitOfWork>();
            var paymentFromMock = Substitute.For<PaymentFrom>();
            
            SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);
            
            orderMock.OrderItems = new List<OrderItem>
            {
                new OrderItem {Count = 7, Price = 200, DiscountMoney = 400, Nomenclature = new Nomenclature()},
            };

            // act

            var smsPaymentDTO = smsPaymentDTOFactory.CreateSmsPaymentDTO(uowMock, smsPaymentMock, orderMock, paymentFromMock);

            // assert

            Assert.That(smsPaymentDTO.Items.Count, Is.EqualTo(2));

            Assert.That(smsPaymentDTO.Items[0].Price, Is.EqualTo(142.86));
            Assert.That(smsPaymentDTO.Items[0].Quantity, Is.EqualTo(6));

            Assert.That(smsPaymentDTO.Items[1].Price, Is.EqualTo(142.84));
            Assert.That(smsPaymentDTO.Items[1].Quantity, Is.EqualTo(1));
        }

        [Test(Description = "Распределение цен по номенклатурам (комбинация разных вариантов )")]
        public void SmsPaymentPriceForNomenclatureDistributionsWithCirculatingFractionCombine()
        {
	        // arrange
	        var organizationProvidertMock = Substitute.For<IOrganizationProvider>();
	        var smsPaymentMock = Substitute.For<SmsPayment>();
	        var orderMock = Substitute.For<Order>();
	        var uowMock = Substitute.For<IUnitOfWork>();
	        var paymentFromMock = Substitute.For<PaymentFrom>();
	        
	        SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);
	        
	        orderMock.OrderItems = new List<OrderItem>
	        {
		        new OrderItem {Count = 3, Price = 150, DiscountMoney = 50, Nomenclature = new Nomenclature()},
		        new OrderItem {Count = 7, Price = 200, DiscountMoney = 400, Nomenclature = new Nomenclature()},
		        new OrderItem {Count = 3, Price = 100,  Nomenclature = new Nomenclature()}
            };

	        // act

	        var smsPaymentDTO = smsPaymentDTOFactory.CreateSmsPaymentDTO(uowMock, smsPaymentMock, orderMock, paymentFromMock);

	        // assert

	        Assert.That(smsPaymentDTO.Items.Count, Is.EqualTo(4));

	        Assert.That(smsPaymentDTO.Items[0].Price, Is.EqualTo(133.33));
	        Assert.That(smsPaymentDTO.Items[0].Quantity, Is.EqualTo(2));

	        Assert.That(smsPaymentDTO.Items[1].Price, Is.EqualTo(133.32));
	        Assert.That(smsPaymentDTO.Items[1].Quantity, Is.EqualTo(1));

	        Assert.That(smsPaymentDTO.Items[2].Price, Is.EqualTo(142.86));
	        Assert.That(smsPaymentDTO.Items[2].Quantity, Is.EqualTo(7));

	        Assert.That(smsPaymentDTO.Items[3].Price, Is.EqualTo(100));
	        Assert.That(smsPaymentDTO.Items[3].Quantity, Is.EqualTo(3));
        }

    }
}
