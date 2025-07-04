using Autofac;
using Microsoft.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Print;
using Vodovoz.ViewModels.Warehouses;
using VodovozBusiness.Controllers;

namespace Vodovoz
{
	public partial class CarLoadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarLoadDocument>
	{
		private ILifetimeScope _lifetimeScope;

		private ILogger<CarLoadDocumentDlg> _logger;
		private IStockRepository _stockRepository;
		private IEmployeeRepository _employeeRepository;
		private IRouteListRepository _routeListRepository;
		private IRouteListService _routeListService;
		private INomenclatureSettings _nomenclatureSettings;
		private IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private IEventsQrPlacer _eventsQrPlacer;
		private ICounterpartyEdoAccountController _edoAccountController;

		public INavigationManager NavigationManager { get; private set; }

		public CarLoadDocumentDlg()
		{
			ResolveDependencies();
			Build();

			ConfigureNewDoc();
			ConfigureDlg();
			OnWarehouseChangedByUser(null, EventArgs.Empty);
		}

		public CarLoadDocumentDlg(int routeListId, int? warehouseId)
		{
			ResolveDependencies();
			Build();
			ConfigureNewDoc();

			if(warehouseId.HasValue)
			{
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);

			}
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarLoadDocumentDlg(int id)
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<CarLoadDocument>(id);
			ConfigureDlg();
		}

		public CarLoadDocumentDlg(CarLoadDocument sub) : this(sub.Id) { }

		private void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			_logger = _lifetimeScope.Resolve<ILogger<CarLoadDocumentDlg>>();
			_stockRepository = _lifetimeScope.Resolve<IStockRepository>();
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_routeListRepository = _lifetimeScope.Resolve<IRouteListRepository>();
			_routeListService = _lifetimeScope.Resolve<IRouteListService>();
			_nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();
			_routeListDailyNumberProvider = _lifetimeScope.Resolve<IRouteListDailyNumberProvider>();
			_eventsQrPlacer = _lifetimeScope.Resolve<IEventsQrPlacer>();
			_edoAccountController = _lifetimeScope.Resolve<ICounterpartyEdoAccountController>();
		}

		private void ConfigureNewDoc()
		{
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<CarLoadDocument>();
			Entity.AuthorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			if(Entity.AuthorId == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			var storeDocument = new StoreDocumentHelper(new UserSettingsService());
			Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.CarLoadEdit);
		}

		private void OnWarehouseChangedByUser(object sender, EventArgs e)
		{
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAmounts();
		}

		private void ConfigureDlg()
		{
			NavigationManager = Startup.MainWin.NavigationManager;

			var storeDocument = new StoreDocumentHelper(new UserSettingsService());
			if(storeDocument.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.CarLoadEdit, Entity.Warehouse))
			{
				FailInitialize = true;
				return;
			}

			var currentUserId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			var hasPermitionToEditDocWithClosedRL =
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_change_car_load_and_unload_docs", currentUserId);

			var editing = storeDocument.CanEditDocument(WarehousePermissionsType.CarLoadEdit, Entity.Warehouse);
			editing &= Entity.RouteList?.Status != RouteListStatus.Closed || hasPermitionToEditDocWithClosedRL;

			entryRouteList.ViewModel = new LegacyEEVMBuilderFactory<CarLoadDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.RouteList)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(filter =>
				{
					filter.DisplayableStatuses = new[] { RouteListStatus.InLoading };
				})
				.Finish();

			entryRouteList.ViewModel.ChangedByUser += OnYentryrefRouteListChangedByUser;

			entryRouteList.ViewModel.IsEditable = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			entryRouteList.Sensitive = editing;
			ytextviewCommnet.Editable = editing;
			carloaddocumentview1.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();

			var warehouseViewModel = new LegacyEEVMBuilderFactory< CarLoadDocument >(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x=>x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = storeDocument.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarLoadEdit).Select(x => x.Id).ToList();
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			warehouseViewModel.IsEditable = editing;

			entryWarehouse.ViewModel = warehouseViewModel;
			entryWarehouse.ViewModel.ChangedByUser += OnWarehouseChangedByUser;

			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(CarLoadPrintableDocuments);

			UpdateRouteListInfo();
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAlreadyLoaded(UoW, _routeListRepository);
			Entity.UpdateInRouteListAmount(UoW, _routeListRepository);
			carloaddocumentview1.DocumentUoW = UoWGeneric;
			carloaddocumentview1.SetIsCanEditDocument(editing);
			buttonSave.Sensitive = editing;
			if(!editing)
			{
				HasChanges = false;
			}

			if(Entity.Id == 0 && Entity.Warehouse != null)
			{
				carloaddocumentview1.FillItemsByWarehouse();
			}

			var permmissionValidator =
				new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);

			Entity.CanEdit =
				permmissionValidator.Validate(typeof(CarLoadDocument), currentUserId, nameof(RetroactivelyClosePermission));

			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryRouteList.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryWarehouse.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewRouteListInfo.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				carloaddocumentview1.Sensitive = false;
				carloaddocumentview1.SetIsCanEditDocument(false);

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

			Entity.UpdateAlreadyLoaded(UoW, _routeListRepository);
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditorId == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if(!IsAllItemsInRouteListLoaded())
			{
				MessageDialogHelper.RunErrorDialog("В маршрутном листе имееются сетевые, либо госзаказы. Частичная погрузка запрещена!");
				return false;
			}

			if(Entity.Items.Any(x => x.Amount == 0))
			{
				var res = MessageDialogHelper.RunQuestionYesNoCancelDialog(
					$"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">В списке есть нулевые позиции. Убрать нулевые позиции перед сохранением?</span>");
				switch(res)
				{
					case -4:            //DeleteEvent
					case -6:            //Cancel
						return false;
					case -8:            //Yes
						Entity.ClearItemsFromZero();
						break;
					case -9:            //No
						break;
				}
			}

			Entity.UpdateOperations(UoW, _nomenclatureSettings.NomenclatureIdForTerminal);

			_logger.LogInformation("Сохраняем погрузочный талон...");
			UoWGeneric.Save();

			_logger.LogInformation("Меняем статус маршрутного листа...");
			if(_routeListService.TrySendEnRoute(UoW, Entity.RouteList, out _))
			{
				MessageDialogHelper.RunInfoDialog("Маршрутный лист отгружен полностью.");
			}

			UoW.Save(Entity.RouteList);

			UoW.Commit();

			_logger.LogInformation("Ok.");

			return true;
		}

		private bool IsAllItemsInRouteListLoaded()
		{
			var isNewEntity = Entity.Id == 0;

			var isAllItemsMustBeLoaded =
				(isNewEntity && Entity.RouteList.Addresses
					.Select(a => a.Order)
					.Any(o => o.IsNeedIndividualSetOnLoad(_edoAccountController) || o.IsNeedIndividualSetOnLoadForTender))
				|| (!isNewEntity && Entity.Items.Any(x => x.IsIndividualSetForOrder));

			if(!isAllItemsMustBeLoaded)
			{
				return true;
			}

			var groupedItemsInRouteList =
				Entity.GetCarLoadDocumentItemsFromRouteList(UoW, _routeListRepository, null, false)
				.GroupBy(x => (x.Nomenclature.Id, x.ExpireDatePercent))
				.ToDictionary(x => x.Key, x => x.Select(item => item.Amount).Sum());

			var groupedEntityItems = Entity.Items
				.GroupBy(x => (x.Nomenclature.Id, x.ExpireDatePercent))
				.ToDictionary(x => x.Key, x => x.Select(item => item.Amount).Sum());

			foreach(var item in groupedItemsInRouteList)
			{
				if(!groupedEntityItems.TryGetValue(item.Key, out var result))
				{
					return false;
				}

				if(result < item.Value)
				{
					return false;
				}
			}

			return true;
		}

		private void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null)
			{
				ytextviewRouteListInfo.Buffer.Text = string.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				string.Format("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
					Entity.RouteList.Id,
					Entity.RouteList.Date,
					Entity.RouteList.Driver.FullName,
					Entity.RouteList.Car.CarModel.Name,
					Entity.RouteList.Car.RegistrationNumber,
					Entity.RouteList.Forwarder != null ? Entity.RouteList.Forwarder.FullName : "(Отсутствует)"
				);
		}

		protected void OnYentryrefRouteListChangedByUser(object sender, EventArgs e)
		{
			UpdateRouteListInfo();
			if(Entity.Warehouse != null && Entity.RouteList != null)
			{
				carloaddocumentview1.FillItemsByWarehouse();
			}
		}

		protected void OnEnumPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			if(UoWGeneric.HasChanges)
			{
				if(CommonDialogs.SaveBeforePrint(typeof(CarLoadDocument), "талона"))
				{
					if(!Save())
					{
						return;
					}
				}
				else
				{
					return;
				}
			}

			_routeListDailyNumberProvider.GetOrCreateDailyNumber(Entity.RouteList.Id, Entity.RouteList.Date);

			var printDocumentsViewModel = _lifetimeScope.Resolve<PrintDocumentsSelectablePrinterViewModel>();
			TabParent.AddSlaveTab(this, printDocumentsViewModel);
			printDocumentsViewModel.ConfigureForCarLoadDocumentsPrint(Entity);
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}

	public enum CarLoadPrintableDocuments
	{
		[Display(Name = "Документ погрузки")]
		LoadDocument
	}
}

