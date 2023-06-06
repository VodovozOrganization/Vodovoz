using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.JournalSelector;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClientBalanceFilter : RepresentationFilterBase<ClientBalanceFilter>
	{
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter = new DeliveryPointJournalFilterViewModel();

		protected override void ConfigureWithUow()
		{
			nomenclatureEntry.SetEntityAutocompleteSelectorFactory(
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope()),
					new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())), new UserRepository()));
			
			nomenclatureEntry.ChangedByUser += NomenclatureEntryOnChangedByUser;

			entryClient.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices));
			var dpFactory = new DeliveryPointJournalFactory(_deliveryPointJournalFilter);
			evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			evmeDeliveryPoint.Changed += (sender, args) => OnRefiltered();

			var userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			if(userHasOnlyAccessToWarehouseAndComplaints)
			{
				evmeDeliveryPoint.CanEditReference = false;
			}
		}

		public ClientBalanceFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ClientBalanceFilter()
		{
			this.Build();
		}

		public Counterparty RestrictCounterparty {
			get { return entryClient.Subject as Counterparty; }
			set {
				entryClient.Subject = value;
				entryClient.Sensitive = false;
			}
		}

		public Nomenclature RestrictNomenclature {
			get { return nomenclatureEntry.Subject as Nomenclature; }
			set {
				nomenclatureEntry.Subject = value;
				nomenclatureEntry.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return evmeDeliveryPoint.Subject as DeliveryPoint; }
			set {
				evmeDeliveryPoint.Subject = value;
				evmeDeliveryPoint.Sensitive = false;
			}
		}

		public bool RestrictIncludeSold {
			get { return checkIncludeSold.Active; }
			set {
				checkIncludeSold.Active = value;
				checkIncludeSold.Sensitive = false;
			}
		}
		
		private void NomenclatureEntryOnChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnSpeccomboStockItemSelected(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryClientChanged(object sender, EventArgs e)
		{
			evmeDeliveryPoint.Sensitive = RestrictCounterparty != null;
			if(RestrictCounterparty == null)
			{
				evmeDeliveryPoint.Subject = null;
			}
			else
			{
				evmeDeliveryPoint.Subject = null;
				_deliveryPointJournalFilter.Counterparty = entryClient.Subject as Counterparty;
			}
			OnRefiltered();
		}

		protected void OnCheckIncludeSoldToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
