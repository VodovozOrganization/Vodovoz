using Gamma.Utilities;
using QS.Utilities;
using System;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Settings.Organizations;

namespace PayPageAPI.Models
{
	public class PayViewModel
	{
		private const string _paymentAttemptMessage = "Если не получилось с первого раза, то";
		private decimal _orderSum;
		private FastPaymentStatus _fastPaymentStatus;

		private readonly IFastPaymentSettings _fastPaymentSettings;
		private readonly IOrganizationSettings _organizationSettings;

		public PayViewModel(
			IFastPaymentSettings fastPaymentSettings,
			IOrganizationSettings organizationSettings,
			FastPayment fastPayment)
		{
			_fastPaymentSettings =
				fastPaymentSettings ?? throw new ArgumentNullException(nameof(fastPaymentSettings));
			_organizationSettings =
				organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));

			if(fastPayment == null)
			{
				throw new ArgumentNullException(nameof(fastPayment));
			}
			
			Initialize(fastPayment);
		}

		public int OrderNum { get; private set; }
		public DateTime? OrderDate { get; private set; }
		public string Ticket { get; private set; }
		public bool IsOnlineOrder { get; private set; }

		public string PayUrl => $"{_fastPaymentSettings.GetAvangardFastPayBaseUrl}?ticket={Ticket}";
		public string SumString => _orderSum.ToShortCurrencyString();
		public string StatusString => _fastPaymentStatus.GetEnumTitle();
		public bool IsNotProcessingStatus => _fastPaymentStatus != FastPaymentStatus.Processing;
		public bool IsNotProcessingOrNotPaymentByCard => _fastPaymentStatus != FastPaymentStatus.Processing || !IsPaymentByCard;
		public bool IsPerformedStatus => _fastPaymentStatus == FastPaymentStatus.Performed;
		public string PayOrderTitle => IsOnlineOrder ? $"Оплата онлайн-заказа №{OrderNum}" : $"Оплата заказа №{OrderNum}";
		public string PaymentAttemptMessage => IsOnlineOrder
			? $"{_paymentAttemptMessage} вернитесь в свой заказ и попробуйте снова"
			: $"{_paymentAttemptMessage} перезвоните нам для получения новой ссылки";
		public string OfertaUrl { get; private set; }
		public bool IsPaymentByCard { get; private set; }

		private void Initialize(FastPayment fastPayment)
		{
			if(fastPayment.Order != null)
			{
				FillOrderData(fastPayment.Order.Id, fastPayment.Order.DeliveryDate, fastPayment.Order.OrderSum);
			}
			else
			{
				FillOrderData(fastPayment.OnlineOrderId.Value, DateTime.Today, fastPayment.Amount);
				IsOnlineOrder = true;
			}
			
			Ticket = fastPayment.Ticket;
			_fastPaymentStatus = fastPayment.FastPaymentStatus;
			IsPaymentByCard = fastPayment.FastPaymentPayType == FastPaymentPayType.ByCard;
			FillOfertaUrl(fastPayment.Organization);
		}

		private void FillOfertaUrl(Organization organization)
		{
			if(organization == null || organization.Id == _organizationSettings.VodovozNorthOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_north.pdf";
			}
			else if(organization.Id == _organizationSettings.VodovozSouthOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_south.pdf";
			}
			else if(organization.Id == _organizationSettings.VodovozEastOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_east.pdf";
			}
			else if(organization.Id == _organizationSettings.VodovozOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv.pdf";
			}
			else if(organization.Id == _organizationSettings.KulerServiceOrganizationId)
			{
				OfertaUrl = "pdf/offer_kuler_service.pdf";
			}
			else
			{
				OfertaUrl = "pdf/offer_world_of_drinks.pdf";
			}
		}

		private void FillOrderData(int orderId, DateTime? orderDate, decimal orderSum)
		{
			OrderNum = orderId;
			OrderDate = orderDate;
			_orderSum = orderSum;
		}
	}
}
