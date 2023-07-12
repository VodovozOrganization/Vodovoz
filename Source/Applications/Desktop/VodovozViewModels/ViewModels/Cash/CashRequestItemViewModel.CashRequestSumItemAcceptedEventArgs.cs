using System;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public partial class CashRequestItemViewModel
	{
		public class CashRequestSumItemAcceptedEventArgs : EventArgs
		{
			public CashRequestSumItemAcceptedEventArgs(CashRequestSumItem cashRequestSumItem)
			{
				AcceptedEntity = cashRequestSumItem;
			}

			public CashRequestSumItem AcceptedEntity { get; private set; }
		}
	}
}
