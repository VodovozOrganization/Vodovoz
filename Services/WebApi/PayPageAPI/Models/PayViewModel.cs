using System;
using Gamma.Utilities;
using QS.Utilities;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Parameters;

namespace PayPageAPI.Models
{
	public class PayViewModel
	{
		private readonly decimal _orderSum;
		private readonly FastPaymentStatus _fastPaymentStatus;

		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		
		public PayViewModel(IFastPaymentParametersProvider fastPaymentParametersProvider, FastPayment fastPayment)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));

			if(fastPayment == null)
			{
				throw new ArgumentNullException(nameof(fastPayment));
			}
			
			OrderNum = fastPayment.Order.Id;
			OrderDate = fastPayment.Order.DeliveryDate;
			_orderSum = fastPayment.Order.OrderSum;
			Ticket = fastPayment.Ticket;
			_fastPaymentStatus = fastPayment.FastPaymentStatus;
		}

		public int OrderNum { get; }
		public DateTime? OrderDate { get; }
		public string Ticket { get; }

		public string PayUrl => $"{_fastPaymentParametersProvider.GetAvangardFastPayBaseUrl}?ticket={Ticket}";
		public string SumString => _orderSum.ToShortCurrencyString();
		public string StatusString => _fastPaymentStatus.GetEnumTitle();
		public bool IsNotPayable => _fastPaymentStatus != FastPaymentStatus.Processing;
	}
}
