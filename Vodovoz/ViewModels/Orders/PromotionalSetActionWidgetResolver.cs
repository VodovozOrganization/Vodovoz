using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		private readonly IUnitOfWork _uow;
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;

		public PromotionalSetActionWidgetResolver(
			IUnitOfWork uow, 
			INomenclatureSelectorFactory nomenclatureSelectorFactory)
		{
			_uow = uow;
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
		}
		
		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice:
					var filter = new NomenclatureFilterViewModel();
					filter.RestrictCategory = NomenclatureCategory.water;
					
					var nomenclatureSelectorFactory = _nomenclatureSelectorFactory.CreateNomenclatureAutocompleteSelectorFactory(filter);
					
					return new AddFixPriceActionViewModel(_uow, promotionalSet, ServicesConfig.CommonServices, nomenclatureSelectorFactory);
				default: 
					throw new ArgumentException();
			}
		}
	}
}
