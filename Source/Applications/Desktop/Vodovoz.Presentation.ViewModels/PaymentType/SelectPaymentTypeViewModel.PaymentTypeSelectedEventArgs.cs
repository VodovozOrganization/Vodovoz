using System;
using ClientPaymentType = Vodovoz.Domain.Client.PaymentType;

namespace Vodovoz.Presentation.ViewModels.PaymentType
{
	public partial class SelectPaymentTypeViewModel
	{
		public class PaymentTypeSelectedEventArgs : EventArgs
		{
			public PaymentTypeSelectedEventArgs(ClientPaymentType paymentType)
			{
				PaymentType = paymentType;
			}

			public ClientPaymentType PaymentType { get; }
		}

	}
}
