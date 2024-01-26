using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[ToolboxItem(true)]
	public partial class ClientBalanceFilter : RepresentationFilterBase<ClientBalanceFilter>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private Nomenclature _restrictNomenclature;
		private Nomenclature _nomenclature;
		private bool _canChangeNomenclature;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter = new DeliveryPointJournalFilterViewModel();

		protected override void ConfigureWithUow()
		{
			//nomenclatureEntry.SetEntityAutocompleteSelectorFactory(
			//new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
			//	typeof(Nomenclature),
			//	() => _lifetimeScope.Resolve<NomenclaturesJournalViewModel>()));
			
			//nomenclatureEntry.ChangedByUser += NomenclatureEntryOnChangedByUser;

			entryClient.SetEntityAutocompleteSelectorFactory(
				_lifetimeScope.Resolve<ICounterpartyJournalFactory>().CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			var dpFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			dpFactory.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilter);
			evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			evmeDeliveryPoint.Changed += (sender, args) => OnRefiltered();

			var userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

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
			Build();
		}

		public Counterparty RestrictCounterparty {
			get { return entryClient.Subject as Counterparty; }
			set {
				entryClient.Subject = value;
				entryClient.Sensitive = false;
			}
		}

		public Nomenclature RestrictNomenclature
		{
			get => _restrictNomenclature;
			set
			{
				_restrictNomenclature = value;

				if(value is null)
				{
					CanChangeNomenclature = true;
					Nomenclature = value;
					return;
				}

				CanChangeNomenclature = false;
				Nomenclature = value;

				entryNomenclature.Sensitive = CanChangeNomenclature;
			}
		}

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => _nomenclature = value;
		}

		public bool CanChangeNomenclature
		{
			get => _canChangeNomenclature;
			set => _canChangeNomenclature = value;
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

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
