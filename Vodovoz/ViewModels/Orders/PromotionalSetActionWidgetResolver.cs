using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		private readonly IUnitOfWork _uow;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;

		public PromotionalSetActionWidgetResolver(
			IUnitOfWork uow, 
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository)
		{
			_uow = uow;
			_counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
		}
		
		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice:
					var filter = new NomenclatureFilterViewModel();
					filter.RestrictCategory = NomenclatureCategory.water;
					
					var nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, filter, _counterpartySelectorFactory,
							 _nomenclatureRepository, _userRepository);
					
					return new AddFixPriceActionViewModel(_uow, promotionalSet, ServicesConfig.CommonServices, nomenclatureSelectorFactory);
				default: 
					throw new ArgumentException();
			}
		}
	}
}
