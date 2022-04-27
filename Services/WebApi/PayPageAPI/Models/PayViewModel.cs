using System;
using Gamma.Utilities;
using QS.Utilities;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Parameters;

namespace PayPageAPI.Models
{
	public class PayViewModel
	{
		private decimal _orderSum;
		private FastPaymentStatus _fastPaymentStatus;

		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		
		public PayViewModel(IFastPaymentParametersProvider fastPaymentParametersProvider, FastPayment fastPayment)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));

			if(fastPayment == null)
			{
				throw new ArgumentNullException(nameof(fastPayment));
			}
			
			Initialize(fastPayment);
		}

		private void Initialize(FastPayment fastPayment)
		{
			if(fastPayment.Order != null)
			{
				OrderNum = fastPayment.Order.Id;
				OrderDate = fastPayment.Order.DeliveryDate;
				_orderSum = fastPayment.Order.OrderSum;
			}
			else
			{
				OrderNum = fastPayment.OnlineOrderId;
				OrderDate = DateTime.Today;
				_orderSum = fastPayment.Amount;
				IsOnlineOrder = true;
			}
			
			Ticket = fastPayment.Ticket;
			_fastPaymentStatus = fastPayment.FastPaymentStatus;
		}

		public int OrderNum { get; private set; }
		public DateTime? OrderDate { get; private set; }
		public string Ticket { get; private set; }
		public bool IsOnlineOrder { get; private set; }

		public string PayUrl => $"{_fastPaymentParametersProvider.GetAvangardFastPayBaseUrl}?ticket={Ticket}";
		public string SumString => _orderSum.ToShortCurrencyString();
		public string StatusString => _fastPaymentStatus.GetEnumTitle();
		public bool IsNotPayable => _fastPaymentStatus != FastPaymentStatus.Processing;
		public string PayOrderTitle => IsOnlineOrder ? $"Оплата онлайн-заказа№{OrderNum}" : $"Оплата заказа№{OrderNum}";
	}
}
