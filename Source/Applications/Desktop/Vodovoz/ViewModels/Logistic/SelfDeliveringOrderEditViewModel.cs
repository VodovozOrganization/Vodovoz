using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public class SelfDeliveringOrderEditViewModel : EntityTabViewModelBase<Order>
	{
		public SelfDeliveringOrderEditViewModel(

			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);
			TabName = "Редактирование самовывоза";
			var t = Entity.GetNomenclaturesWithFixPrices;
			var e = Entity;
			
			SaveCommand = new DelegateCommand(
				() => Save(),
				() => BeforeSave());
			CloseCommand = new DelegateCommand(() => Console.WriteLine());
			PaymentTypeCommand = new DelegateCommand(() => Console.WriteLine());

		}
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		public DelegateCommand PaymentTypeCommand { get; }
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

		protected override bool BeforeSave()
		{

			return base.BeforeSave();
		}
		public IEnumerable<GeoGroup> GetSelfDeliveryGeoGroups()
		{
			var currentGeoGroupId = Entity?.SelfDeliveryGeoGroup?.Id;

			var geoGroups = UoW.GetAll<GeoGroup>().Where(geo => !geo.IsArchived || geo.Id == currentGeoGroupId).ToList();

			return geoGroups;
		}
	}
}
