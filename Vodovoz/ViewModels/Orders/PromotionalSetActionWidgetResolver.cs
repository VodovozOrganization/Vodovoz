using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;

		public PromotionalSetActionWidgetResolver(IUnitOfWork UoW, IEntityAutocompleteSelectorFactory counterpartySelectorFactory)
		{
			uow = UoW;
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
		}

		private IUnitOfWork uow;

		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice:
					var filter = new NomenclatureFilterViewModel();
					filter.RestrictCategory = NomenclatureCategory.water;
					
					var nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, filter, counterpartySelectorFactory);
					
					return new AddFixPriceActionViewModel(uow, promotionalSet, ServicesConfig.CommonServices, nomenclatureSelectorFactory);
				default: 
					throw new ArgumentException();
			}
		}
	}
}
