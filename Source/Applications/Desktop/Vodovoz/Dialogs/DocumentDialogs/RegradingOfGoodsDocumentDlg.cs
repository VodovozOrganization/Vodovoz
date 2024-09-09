using System;
using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz
{
	public partial class RegradingOfGoodsDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<RegradingOfGoodsDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IEmployeeRepository _employeeRepository;
		private IUserRepository _userRepository;
		private IStoreDocumentHelper _storeDocumentHelper;
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

		public RegradingOfGoodsDocumentDlg()
		{
			ResolveDependencies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<RegradingOfGoodsDocument> ();
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			
			Entity.Warehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.RegradingOfGoodsEdit);

			ConfigureDlg();
		}

		public RegradingOfGoodsDocumentDlg (int id)
		{
			ResolveDependencies();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RegradingOfGoodsDocument> (id);
			
			ConfigureDlg();
		}

		public RegradingOfGoodsDocumentDlg (RegradingOfGoodsDocument sub) : this (sub.Id)
		{
		}

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_userRepository = _lifetimeScope.Resolve<IUserRepository>();
			_storeDocumentHelper = _lifetimeScope.Resolve<IStoreDocumentHelper>();
		}

		void ConfigureDlg ()
		{
			regradingofgoodsitemsview.NavigationManager = Startup.MainWin.NavigationManager;
			regradingofgoodsitemsview.ParrentDlg = this;

			if(_storeDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.RegradingOfGoodsEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.RegradingOfGoodsEdit, Entity.Warehouse);
			regradingofgoodsitemsview.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			
			var userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.User.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			if(userHasOnlyAccessToWarehouseAndComplaints)
			{
				warehouseEntry.CanEditReference = false;
			}
			else
			{
				warehouseEntry.CanEditReference = true;
			}

			var availableWarehousesIds =
				_storeDocumentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.RegradingOfGoodsEdit);
			Action<WarehouseJournalFilterViewModel> filterParams = f => f.IncludeWarehouseIds = availableWarehousesIds;

			var warehouseJournalFactory = new WarehouseJournalFactory();

			warehouseEntry.SetEntityAutocompleteSelectorFactory(warehouseJournalFactory.CreateSelectorFactory(_lifetimeScope, filterParams));
			warehouseEntry.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			regradingofgoodsitemsview.DocumentUoW = UoWGeneric;
			if (Entity.Items.Count > 0)
				warehouseEntry.Sensitive = false;

			var permmissionValidator = new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			Entity.CanEdit = permmissionValidator.Validate(typeof(RegradingOfGoodsDocument), _userRepository.GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				warehouseEntry.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				regradingofgoodsitemsview.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}
		}

		public override bool Save ()
		{
			if(!Entity.CanEdit)
				return false;

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем документ пересортицы...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
		
		protected override void OnDestroyed()
		{
			_employeeRepository = null;
			_userRepository = null;
			_storeDocumentHelper = null;

			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
	}
}

