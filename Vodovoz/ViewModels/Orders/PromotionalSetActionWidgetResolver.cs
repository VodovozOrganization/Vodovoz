using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		public PromotionalSetActionWidgetResolver(IUnitOfWork UoW)
		{
			uow = UoW;
		}

		private IUnitOfWork uow;

		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice: return new AddFixPriceActionViewModel(uow, promotionalSet, ServicesConfig.CommonServices);
				default: throw new ArgumentException();
			}
		}
	}
}
