using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz
{
	public partial class IncomingWaterDlg : QS.Dialog.Gtk.EntityDialogBase<IncomingWater>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private IEmployeeRepository _employeeRepository;
		private IUserRepository _userRepository;
		private IStoreDocumentHelper _storeDocumentHelper;

		public INavigationManager NavigationManager { get; private set; }
		public IEntityEntryViewModel SourceWarehouseViewModel { get; private set; }
		public IEntityEntryViewModel DestinationWarehouseViewModel { get; private set; }

		public IncomingWaterDlg()
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<IncomingWater>();
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.IncomingWarehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.IncomingWaterEdit);
			Entity.WriteOffWarehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.IncomingWaterEdit);

			ConfigureDlg();
		}

		public IncomingWaterDlg(int id)
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<IncomingWater>(id);

			ConfigureDlg();
		}

		public IncomingWaterDlg(IncomingWater sub) : this(sub.Id)
		{
		}

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_userRepository = _lifetimeScope.Resolve<IUserRepository>();
			_storeDocumentHelper = _lifetimeScope.Resolve<IStoreDocumentHelper>();
		}

		void ConfigureDlg()
		{
			NavigationManager = _lifetimeScope.Resolve<INavigationManager>();

			if(_storeDocumentHelper.CheckAllPermissions(
					UoW.IsNew, WarehousePermissionsType.IncomingWaterEdit, Entity.IncomingWarehouse, Entity.WriteOffWarehouse))
			{
				FailInitialize = true;
				return;
			}

			var editing = _storeDocumentHelper.CanEditDocument(
				WarehousePermissionsType.IncomingWaterEdit, Entity.IncomingWarehouse, Entity.WriteOffWarehouse);

			var entityEntryNomenclatureViewModel = new LegacyEEVMBuilderFactory<IncomingWater>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Product)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.HidenByDefault = true;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			entryProduct.ViewModel = entityEntryNomenclatureViewModel;

			buttonFill.Sensitive = entryProduct.ViewModel.IsEditable = spinAmount.Sensitive = editing;
			incomingwatermaterialview1.Sensitive = editing;

			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();
			spinAmount.Binding.AddBinding(Entity, e => e.Amount, w => w.ValueAsInt).InitializeFromSource();

			var userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.User.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			var availableWarehousesIds = _storeDocumentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.IncomingWaterEdit);

			var sourceWarehouseViewModelBuilderFactory = new LegacyEEVMBuilderFactory<IncomingWater>(this, Entity, UoW, NavigationManager, _lifetimeScope);
			var targetWarehouseViewModelBuilderFactory = new LegacyEEVMBuilderFactory<IncomingWater>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			SourceWarehouseViewModel = sourceWarehouseViewModelBuilderFactory
				.ForProperty(x => x.WriteOffWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = availableWarehousesIds;
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			DestinationWarehouseViewModel = sourceWarehouseViewModelBuilderFactory
				.ForProperty(x => x.IncomingWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = availableWarehousesIds;
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			SourceWarehouseViewModel.IsEditable = !userHasOnlyAccessToWarehouseAndComplaints;
			DestinationWarehouseViewModel.IsEditable = !userHasOnlyAccessToWarehouseAndComplaints;

			entityentrySourceWarehouse.ViewModel = SourceWarehouseViewModel;
			entityentryDestinationWarehouse.ViewModel = DestinationWarehouseViewModel;

			incomingwatermaterialview1.DocumentUoW = UoWGeneric;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);

			Entity.CanEdit =
				permmissionValidator.Validate(
					typeof(IncomingWater), _userRepository.GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));

			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				spinAmount.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryProduct.Sensitive = false;
				DestinationWarehouseViewModel.IsEditable = false;
				SourceWarehouseViewModel.IsEditable = false;
				buttonFill.Sensitive = false;
				incomingwatermaterialview1.Sensitive = false;
				buttonSave.Sensitive = false;
			}
			else
			{
				Entity.CanEdit = true;
			}
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
			{
				return false;
			}

			if(CheckWarehouseItems() == false)
			{
				MessageDialogHelper.RunErrorDialog("На складе не хватает материалов");
				return false;
			}

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info("Сохраняем документ производства...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		private bool CheckWarehouseItems()
		{
			foreach(var mater in Entity.Materials)
			{
				if(mater.Amount > mater.AmountOnSource)
				{
					return false;
				}
			}
			return true;
		}

		protected void OnButtonFillClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(ProductSpecification), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;

			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void SelectDialog_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var spec = e.Subject as ProductSpecification;
			UoWGeneric.Root.Product = spec.Product;
			UoWGeneric.Root.ObservableMaterials.Clear();
			foreach(var material in spec.Materials)
			{
				UoWGeneric.Root.AddMaterial(material);
			}
		}

		public override void Destroy()
		{
			_employeeRepository = null;
			_userRepository = null;
			_storeDocumentHelper = null;
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}

