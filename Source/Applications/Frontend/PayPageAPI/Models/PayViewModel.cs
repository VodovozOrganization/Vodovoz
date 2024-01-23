using System;
using Gamma.Utilities;
using QS.Utilities;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace PayPageAPI.Models
{
	public class PayViewModel
	{
		private const string _paymentAttemptMessage = "Если не получилось с первого раза, то";
		private decimal _orderSum;
		private FastPaymentStatus _fastPaymentStatus;

		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;

		public PayViewModel(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IOrganizationParametersProvider organizationParametersProvider,
			FastPayment fastPayment)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			_organizationParametersProvider =
				organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));

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

		public string PayUrl => $"{_fastPaymentParametersProvider.GetAvangardFastPayBaseUrl}?ticket={Ticket}";
		public string SumString => _orderSum.ToShortCurrencyString();
		public string StatusString => _fastPaymentStatus.GetEnumTitle();
		public bool IsNotProcessingStatus => _fastPaymentStatus != FastPaymentStatus.Processing;
		public bool IsPerformedStatus => _fastPaymentStatus == FastPaymentStatus.Performed;
		public string PayOrderTitle => IsOnlineOrder ? $"Оплата онлайн-заказа №{OrderNum}" : $"Оплата заказа №{OrderNum}";
		public string PaymentAttemptMessage => IsOnlineOrder
			? $"{_paymentAttemptMessage} вернитесь в свой заказ и попробуйте снова"
			: $"{_paymentAttemptMessage} перезвоните нам для получения новой ссылки";
		public string OfertaUrl { get; private set; }
		
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
			FillOfertaUrl(fastPayment.Organization);
		}

		private void FillOfertaUrl(Organization organization)
		{
			if(organization == null || organization.Id == _organizationParametersProvider.VodovozNorthOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_north.pdf";
			}
			else if(organization.Id == _organizationParametersProvider.VodovozSouthOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_south.pdf";
			}
			else if(organization.Id == _organizationParametersProvider.VodovozEastOrganizationId)
			{
				OfertaUrl = "pdf/offer_vv_east.pdf";
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
