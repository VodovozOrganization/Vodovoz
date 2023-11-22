using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetActionWidgetResolver
	{
		private readonly IUnitOfWork _uow;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;

		public PromotionalSetActionWidgetResolver(
			IUnitOfWork uow,
			ILifetimeScope lifetimeScope,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository)
		{
			_uow = uow;
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
		}
		
		public WidgetViewModelBase Resolve(PromotionalSet promotionalSet, PromotionalSetActionType setActionType)
		{
			switch(setActionType) {
				case PromotionalSetActionType.FixedPrice:
					return new AddFixPriceActionViewModel(_uow, promotionalSet, ServicesConfig.CommonServices, new NomenclatureJournalFactory(_lifetimeScope));
				default: 
					throw new ArgumentException();
			}
		}
	}
}
