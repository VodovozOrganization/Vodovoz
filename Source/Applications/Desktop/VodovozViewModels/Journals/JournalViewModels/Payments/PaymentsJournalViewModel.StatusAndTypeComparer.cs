using System;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public partial class PaymentsJournalViewModel
	{
		public struct StatusAndTypeComparer : IComparable<StatusAndTypeComparer>, IComparable
		{
			private static readonly Type[] _nodeTypes = new Type[]
			{
				typeof(Payment),
				typeof(PaymentWriteOff)
			};

			public PaymentState? PaymentState { get; set; }

			public Type NodeType { get; set; }

			public int CompareTo(StatusAndTypeComparer other)
			{
				if(PaymentState == other.PaymentState)
				{
					return 0;
				}

				if(other.NodeType == typeof(PaymentWriteOff))
				{
					return PaymentState > Core.Domain.Payments.PaymentState.undistributed ? 1 : -1;
				}
				else if(other.NodeType == typeof(PaymentWriteOff))
				{
					return PaymentState < Core.Domain.Payments.PaymentState.undistributed ? 1 : -1;
				}

				return PaymentState > other.PaymentState ? 1 : -1;
			}

			public int CompareTo(object obj)
			{
				return obj is StatusAndTypeComparer satc ? CompareTo(satc) : 0;
			}
		}
	}
}
