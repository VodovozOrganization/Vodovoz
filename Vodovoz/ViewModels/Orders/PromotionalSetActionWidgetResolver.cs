using System;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice: return new AddFixPriceActionViewModel(promotionalSet, ServicesConfig.CommonServices);
				default: throw new ArgumentException();
			}
		}
	}
}