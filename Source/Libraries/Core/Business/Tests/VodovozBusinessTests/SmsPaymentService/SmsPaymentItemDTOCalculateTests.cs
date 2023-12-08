using NSubstitute;
using NUnit.Framework;
using SmsPaymentService;
using System.Collections;
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
			var order1 = new Order();

			// (3*150-50)/3 = 133,3333333333333
			order1.AddNomenclature(new Nomenclature(), 3, 50, true, new DiscountReason());
			order1.OrderItems.LastOrDefault().SetPrice(150);

			// (5*100)/5 = 100
			order1.AddNomenclature(new Nomenclature(), 5);
			order1.OrderItems.LastOrDefault().SetPrice(100);

			yield return order1;

			var order2 = new Order();

			// (12*420-340)/12 = 391,6666666666667
			order2.AddNomenclature(new Nomenclature(), 12, 340, true, new DiscountReason());
			order2.OrderItems.LastOrDefault().SetPrice(420);

			yield return order2;

			var order3 = new Order();

			// (7*200-400)/7 = 142,8571428571429
			order3.AddNomenclature(new Nomenclature(), 7, 400, true, new DiscountReason());
			order3.OrderItems.LastOrDefault().SetPrice(200);

			// (11*50-50)/11 = 45,45454545454545
			order3.AddNomenclature(new Nomenclature(), 11, 50, true, new DiscountReason());
			order3.OrderItems.LastOrDefault().SetPrice(50);

			// (3*500-500)/3 = 333,3333333333333
			order3.AddNomenclature(new Nomenclature(), 3, 500, true, new DiscountReason());
			order3.OrderItems.LastOrDefault().SetPrice(500);

			// (4*120-10)/4 = 117,5
			order3.AddNomenclature(new Nomenclature(), 4, 10, true, new DiscountReason());
			order3.OrderItems.LastOrDefault().SetPrice(120);

			// (8*500)/8 = 500
			order3.AddNomenclature(new Nomenclature(), 8);
			order3.OrderItems.LastOrDefault().SetPrice(500);

			yield return order3;
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
			var orderMock = new Order();
			var uowMock = Substitute.For<IUnitOfWork>();
			var paymentFromMock = Substitute.For<PaymentFrom>();
			
			SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);

			orderMock.AddNomenclature(new Nomenclature(), 3);
			orderMock.OrderItems.LastOrDefault().SetPrice(300);

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
			var orderMock = new Order();
			var uowMock = Substitute.For<IUnitOfWork>();
			var paymentFromMock = Substitute.For<PaymentFrom>();
			
			SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);

			orderMock.AddNomenclature(new Nomenclature(), 3, 50, true, new DiscountReason());
			orderMock.OrderItems.LastOrDefault().SetPrice(150);

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
			var orderMock = new Order();
			var uowMock = Substitute.For<IUnitOfWork>();
			var paymentFromMock = Substitute.For<PaymentFrom>();
			
			SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);

			orderMock.AddNomenclature(new Nomenclature(), 7, 400, true, new DiscountReason());
			orderMock.OrderItems.LastOrDefault().SetPrice(200);

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
			var discountReason = Substitute.For<DiscountReason>();
			var orderMock = new Order();
			var uowMock = Substitute.For<IUnitOfWork>();
			var paymentFromMock = Substitute.For<PaymentFrom>();
			
			SmsPaymentDTOFactory smsPaymentDTOFactory = new SmsPaymentDTOFactory(organizationProvidertMock);

			orderMock.AddNomenclature(new Nomenclature(), 3, 50, true, discountReason);
			orderMock.OrderItems.LastOrDefault().SetPrice(150);

			orderMock.AddNomenclature(new Nomenclature(), 7, 400, true, discountReason);
			orderMock.OrderItems.LastOrDefault().SetPrice(200);

			orderMock.AddNomenclature(new Nomenclature(), 3);
			orderMock.OrderItems.LastOrDefault().SetPrice(100);

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
