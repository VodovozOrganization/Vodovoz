using Autofac;
using Microsoft.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Tools;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;

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
		private ITerminalNomenclatureProvider _terminalNomenclatureProvider;
		private IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private IEventsQrPlacer _eventsQrPlacer;

		public INavigationManager NavigationManager { get; private set; }

		public CarLoadDocumentDlg()
		{
			ResolveDependencies();
			Build();

			ConfigureNewDoc();
			ConfigureDlg();
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
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarLoadDocument>(id);
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
			_terminalNomenclatureProvider = _lifetimeScope.Resolve<ITerminalNomenclatureProvider>();
			_routeListDailyNumberProvider = _lifetimeScope.Resolve<IRouteListDailyNumberProvider>();
			_eventsQrPlacer = _lifetimeScope.Resolve<IEventsQrPlacer>();
		}

		private void ConfigureNewDoc()
		{
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarLoadDocument>();
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.CarLoadEdit);
		}

		private void ConfigureDlg()
		{
			NavigationManager = Startup.MainWin.NavigationManager;

			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
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

			entryRouteList.Sensitive = ySpecCmbWarehouses.Sensitive = ytextviewCommnet.Editable = editing;
			carloaddocumentview1.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ySpecCmbWarehouses.ItemsList = storeDocument.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarLoadEdit);
			ySpecCmbWarehouses.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(CarLoadPrintableDocuments);

			UpdateRouteListInfo();
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAlreadyLoaded(UoW, _routeListRepository);
			Entity.UpdateInRouteListAmount(UoW, _routeListRepository);
			carloaddocumentview1.DocumentUoW = UoWGeneric;
			carloaddocumentview1.SetButtonEditing(editing);
			buttonSave.Sensitive = editing;
			if(!editing)
			{
				HasChanges = false;
			}

			if(UoW.IsNew && Entity.Warehouse != null)
			{
				carloaddocumentview1.FillItemsByWarehouse();
			}

			ySpecCmbWarehouses.ItemSelected += OnYSpecCmbWarehousesItemSelected;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);

			Entity.CanEdit =
				permmissionValidator.Validate(typeof(CarLoadDocument), currentUserId, nameof(RetroactivelyClosePermission));

			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryRouteList.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ySpecCmbWarehouses.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewRouteListInfo.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				carloaddocumentview1.Sensitive = false;

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
			var validator = new ObjectValidator(new GtkValidationViewFactory());
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

			Entity.UpdateOperations(UoW, _terminalNomenclatureProvider.GetNomenclatureIdForTerminal);

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

		protected void OnYSpecCmbWarehousesItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAmounts();
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

			var rdlPath = "Reports/Store/CarLoadDocument.rdl";
			_routeListDailyNumberProvider.GetOrCreateDailyNumber(Entity.RouteList.Id, Entity.RouteList.Date);

			_eventsQrPlacer.AddQrEventForDocument(UoW, Entity.Id, EventQrDocumentType.CarLoadDocument, ref rdlPath);

			var reportInfo = new ReportInfo
			{
				Title = Entity.Title,
				Path = rdlPath,
				Parameters = new System.Collections.Generic.Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					},
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
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

