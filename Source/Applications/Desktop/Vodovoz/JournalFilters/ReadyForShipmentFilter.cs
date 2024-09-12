using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class ReadyForShipmentFilter : RepresentationFilterBase<ReadyForShipmentFilter>, ISingleUoWDialog, INotifyPropertyChanged
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private Warehouse _warehouse;

		public event PropertyChangedEventHandler PropertyChanged;

		public ReadyForShipmentFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ReadyForShipmentFilter()
		{
			Build();
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(_warehouse != value)
				{
					_warehouse = value;
					OnRefiltered();
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Warehouse)));
				}
			}
		}

		public ITdiTab ParentTab { get; set; }

		public EntityEntryViewModel<Warehouse> WarehouseViewModel { get; private set; }

		protected override void ConfigureWithUow()
		{
			var warehousesList = new StoreDocumentHelper(new UserSettingsService())
				.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.WarehouseView)
				.OrderBy(w => w.Name).ToList();

			bool accessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			var navigatiuonManager = _lifetimeScope.Resolve<INavigationManager>();

			var builderFactory = new LegacyEEVMBuilderFactory<ReadyForShipmentFilter>(ParentTab, this, UoW, navigatiuonManager, _lifetimeScope);

			WarehouseViewModel = builderFactory.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = warehousesList.Select(x => x.Id);
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			WarehouseViewModel.Entity = CurrentUserSettings.Settings.DefaultWarehouse ?? null;

			yspeccomboWarehouse.Visible = false;

			if(accessToWarehouseAndComplaints)
			{
				WarehouseViewModel.IsEditable = false;
			}

			entityentryWarehouse.ViewModel = WarehouseViewModel;
		}

		protected override void OnDestroyed()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
	}
}

