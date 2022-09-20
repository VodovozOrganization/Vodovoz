using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModel;
using GC = System.GC;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : TdiTabBase, ISingleUoWDialog
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));
		private readonly IEmployeeNomenclatureMovementRepository _employeeNomenclatureMovementRepository;
		private readonly ITerminalNomenclatureProvider _terminalNomenclatureProvider;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IEmployeeService _employeeService;
		private readonly ICommonServices _commonServices;
		private readonly ICategoryRepository _categoryRepository;

		private GenericObservableList<EmployeeBalanceNode> ObservableDriverBalanceFrom { get; set; } = new GenericObservableList<EmployeeBalanceNode>();
		private GenericObservableList<EmployeeBalanceNode> ObservableDriverBalanceTo { get; set; } = new GenericObservableList<EmployeeBalanceNode>();

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		public enum OpenParameter { Sender, Receiver }

		#endregion

		#region Конструкторы

		public RouteListAddressesTransferringDlg(
			IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository,
			ITerminalNomenclatureProvider terminalNomenclatureProvider,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			ICategoryRepository categoryRepository)
		{
			Build();
			_employeeNomenclatureMovementRepository = employeeNomenclatureMovementRepository
				?? throw new ArgumentNullException(nameof(employeeNomenclatureMovementRepository));
			_terminalNomenclatureProvider = terminalNomenclatureProvider
				?? throw new ArgumentNullException(nameof(terminalNomenclatureProvider));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
			
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		public RouteListAddressesTransferringDlg(
			int routeListId,
			OpenParameter param,
			IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository,
			ITerminalNomenclatureProvider terminalNomenclatureProvider,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			ICategoryRepository categoryRepository)
			: this(
				employeeNomenclatureMovementRepository,
				terminalNomenclatureProvider,
				routeListRepository,
				routeListItemRepository,
				employeeService,
				commonServices,
				categoryRepository)
		{
			var rl = UoW.GetById<RouteList>(routeListId);

			switch(param)
			{
				case OpenParameter.Sender:
					yentryreferenceRLFrom.Subject = rl;
					break;
				case OpenParameter.Receiver:
					yentryreferenceRLTo.Subject = rl;
					break;
			}
		}

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			var filterFrom = new RouteListsFilter(UoW);
			filterFrom.SetAndRefilterAtOnce(
				f => f.OnlyStatuses = new[] {
					RouteListStatus.EnRoute,
					RouteListStatus.OnClosing
				},
				f => f.SetFilterDates(
					DateTime.Today.AddDays(-3),
					DateTime.Today.AddDays(1)
				)
			);

			var vmFrom = new RouteListsVM(filterFrom);
			GC.KeepAlive(vmFrom);
			yentryreferenceRLFrom.RepresentationModel = vmFrom;
			yentryreferenceRLFrom.JournalButtons = QS.Project.Dialogs.GtkUI.Buttons.Add | QS.Project.Dialogs.GtkUI.Buttons.Edit;
			yentryreferenceRLFrom.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			var filterTo = new RouteListsFilter(UoW);
			filterTo.SetAndRefilterAtOnce(
				f => f.OnlyStatuses = new[] {
					RouteListStatus.New,
					RouteListStatus.InLoading,
					RouteListStatus.EnRoute,
					RouteListStatus.OnClosing
				},
				f => f.SetFilterDates(
					DateTime.Today.AddDays(-3),
					DateTime.Today.AddDays(1)
				)
			);

			var vmTo = new RouteListsVM(filterTo);
			yentryreferenceRLTo.RepresentationModel = vmTo;
			yentryreferenceRLTo.JournalButtons = QS.Project.Dialogs.GtkUI.Buttons.Add | QS.Project.Dialogs.GtkUI.Buttons.Edit;
			yentryreferenceRLTo.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			yentryreferenceRLFrom.Changed += YentryreferenceRLFrom_Changed;
			yentryreferenceRLTo.Changed += YentryreferenceRLTo_Changed;

			//Для каждой TreeView нужен свой экземпляр ColumnsConfig
			ytreeviewRLFrom.ColumnsConfig = GetColumnsConfig(false);
			ytreeviewRLTo.ColumnsConfig = GetColumnsConfig(true);

			ytreeviewRLFrom.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRLTo.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewRLFrom.Selection.Changed += YtreeviewRLFrom_OnSelectionChanged;
			ytreeviewRLTo.Selection.Changed += YtreeviewRLTo_OnSelectionChanged;

			ConfigureTreeViewsDriverBalance();

			ybtnTransferTerminal.Clicked += (sender, e) => TransferTerminal.Execute();
			ybtnRevertTerminal.Clicked += (sender, e) => RevertTerminal.Execute();
		}

		void YtreeviewRLFrom_OnSelectionChanged(object sender, EventArgs e)
		{
			CheckSensitivities();
		}

		void YtreeviewRLTo_OnSelectionChanged(object sender, EventArgs e)
		{
			CheckSensitivities();

			buttonRevert.Sensitive = ytreeviewRLTo.GetSelectedObjects<RouteListItemNode>()
				.Any(x => x.WasTransfered);
		}

		private IColumnsConfig GetColumnsConfig(bool isRightPanel)
		{
			var colorGreen = new Gdk.Color(0x44, 0xcc, 0x49);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);

			var config = ColumnsConfigFactory.Create<RouteListItemNode>()
				.AddColumn("Еж. номер").AddTextRenderer(node => node.DalyNumber)
				.AddColumn("Заказ").AddTextRenderer(node => node.Id)
				.AddColumn("Дата").AddTextRenderer(node => node.Date)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Бутыли").AddTextRenderer(node => node.BottlesCount)
				.AddColumn("Статус").AddEnumRenderer(node => node.Status)
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.IsFastDelivery).Editing(false);

			if(isRightPanel)
			{
				config.AddColumn("Нужна загрузка").AddToggleRenderer(node => node.NeedToReload)
					.Editing(false);
			}
			else
			{
				config.AddColumn("Нужна загрузка")
					.AddToggleRenderer(x => x.LeftNeedToReload).Radio()
					.AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered && !x.IsFastDelivery)
					.AddTextRenderer(x => (x.Status != RouteListItemStatus.Transfered && !x.IsFastDelivery) ? "Да" : "")
					.AddToggleRenderer(x => x.LeftNotNeedToReload).Radio()
					.AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered && !x.IsFastDelivery)
					.AddTextRenderer(x => (x.Status != RouteListItemStatus.Transfered && !x.IsFastDelivery) ? "Нет" : "");
			}

			return config.AddColumn("Нужен терминал").AddToggleRenderer(x => x.NeedTerminal).Editing(false)
			             .AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
			             .RowCells().AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.WasTransfered ? colorGreen : colorWhite)
			             .Finish();
		}

		private void ConfigureTreeViewsDriverBalance()
		{
			yTreeViewDriverBalanceFrom.ColumnsConfig = FluentColumnsConfig<EmployeeBalanceNode>.Create()
				.AddColumn("Код").AddTextRenderer(n => n.NomenclatureId.ToString())
				.AddColumn("Номенклатура").AddTextRenderer(n => n.NomenclatureName)
				.AddColumn("Количество").AddTextRenderer(n => n.Amount.ToString())
				.Finish();

			yTreeViewDriverBalanceTo.ColumnsConfig = FluentColumnsConfig<EmployeeBalanceNode>.Create()
				.AddColumn("Код").AddTextRenderer(n => n.NomenclatureId.ToString())
				.AddColumn("Номенклатура").AddTextRenderer(n => n.NomenclatureName)
				.AddColumn("Количество").AddTextRenderer(n => n.Amount.ToString())
				.Finish();

			yTreeViewDriverBalanceFrom.ItemsDataSource = ObservableDriverBalanceFrom;
			yTreeViewDriverBalanceTo.ItemsDataSource = ObservableDriverBalanceTo;
		}

		void YentryreferenceRLFrom_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRLFrom.Subject == null)
			{
				ytreeviewRLFrom.ItemsDataSource = null;
				return;
			}

			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo))
			{
				yentryreferenceRLFrom.Subject = null;
				MessageDialogHelper.RunErrorDialog("Вы дурачёк?", "Вы не можете забирать адреса из того же МЛ, в который собираетесь передавать.");
				return;
			}

			if(TabParent != null)
			{
				var tab = TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeListFrom.Id));

				if(!(tab is RouteListClosingDlg))
				{
					if(tab != null)
					{
						MessageDialogHelper.RunErrorDialog("Маршрутный лист уже открыт в другой вкладке");
						yentryreferenceRLFrom.Subject = null;
						return;
					}
				}
			}

			CheckSensitivities();

			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach(var item in routeListFrom.Addresses)
			{

				items.Add(new RouteListItemNode { RouteListItem = item });

			}

			ytreeviewRLFrom.ItemsDataSource = items;

			FillObservableDriverBalance(ObservableDriverBalanceFrom, routeListFrom);
		}

		void YentryreferenceRLTo_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRLTo.Subject == null)
			{
				ytreeviewRLTo.ItemsDataSource = null;
				return;
			}

			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo))
			{
				yentryreferenceRLTo.Subject = null;
				MessageDialogHelper.RunErrorDialog("Вы дурачёк?", "Вы не можете передавать адреса в тот же МЛ, из которого забираете.");
				return;
			}

			if(TabParent != null)
			{
				var tab = TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeListTo.Id));
				if(!(tab is RouteListClosingDlg))
				{
					if(tab != null)
					{
						MessageDialogHelper.RunErrorDialog("Маршрутный лист уже открыт в другой вкладке");
						yentryreferenceRLTo.Subject = null;
						return;
					}
				}
			}

			CheckSensitivities();

			routeListTo.UoW = UoW;
			IList<RouteListItemNode> items = new List<RouteListItemNode>();

			foreach(var item in routeListTo.Addresses)
			{
				items.Add(new RouteListItemNode { RouteListItem = item });
			}

			ytreeviewRLTo.ItemsDataSource = items;
			FillObservableDriverBalance(ObservableDriverBalanceTo, routeListTo);
		}

		private void UpdateNodes()
		{
			YentryreferenceRLFrom_Changed(null, null);
			YentryreferenceRLTo_Changed(null, null);
		}

		private void FillObservableDriverBalance(GenericObservableList<EmployeeBalanceNode> observableDriverBalance, RouteList routeList)
		{
			observableDriverBalance.Clear();

			var driverTerminalBalance = _employeeNomenclatureMovementRepository.GetTerminalFromDriverBalance(UoW,
				routeList.Driver.Id,
				_terminalNomenclatureProvider.GetNomenclatureIdForTerminal);

			if (driverTerminalBalance != null)
			{
				observableDriverBalance.Add(driverTerminalBalance);
			}
		}

		protected void OnButtonTransferClicked(object sender, EventArgs e)
		{
			//Дополнительные проверки
			var routeListTo = yentryreferenceRLTo.Subject as RouteList;
			var routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			var messages = new List<string>();

			if(routeListTo == null || routeListFrom == null || routeListTo.Id == routeListFrom.Id)
			{
				return;
			}

			List<RouteListItemNode> needReloadNotSet = new List<RouteListItemNode>();
			List<RouteListItemNode> needReloadSetAndRlEnRoute = new List<RouteListItemNode>();
			List<RouteListItemNode> fastDeliveryNotEnoughQuantity = new List<RouteListItemNode>();

			foreach(var row in ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>())
			{
				RouteListItem item = row?.RouteListItem;
				_logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

				if(item == null || item.Status == RouteListItemStatus.Transfered)
				{
					continue;
				}

				if(!row.IsFastDelivery && !row.LeftNeedToReload && !row.LeftNotNeedToReload)
				{
					needReloadNotSet.Add(row);
					continue;
				}

				if(row.LeftNeedToReload && routeListTo.Status >= RouteListStatus.EnRoute)
				{
					needReloadSetAndRlEnRoute.Add(row);
					continue;
				}

				if(row.IsFastDelivery)
				{
					var hasEnoughQuantityForFastDelivery = _routeListItemRepository
						.HasEnoughQuantityForFastDelivery(UoW, row.RouteListItem, routeListTo);

					if(!hasEnoughQuantityForFastDelivery)
					{
						fastDeliveryNotEnoughQuantity.Add(row);
						continue;
					}
				}

				var transferredAddressFromRouteListTo =
					_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(UoW, routeListTo.Id, item.Order.Id);

				RouteListItem newItem = null;

				if(transferredAddressFromRouteListTo != null)
				{
					newItem = transferredAddressFromRouteListTo;
					item.WasTransfered = false;
					routeListTo.RevertTransferAddress(_wageParameterService, newItem, item);
					routeListFrom.TransferAddressTo(item, newItem);
					newItem.WasTransfered = true;
				}
				else
				{
					newItem = new RouteListItem(routeListTo, item.Order, item.Status)
					{
						WasTransfered = true,
						NeedToReload = row.LeftNeedToReload,
						WithForwarder = routeListTo.Forwarder != null
					};

					routeListTo.ObservableAddresses.Add(newItem);
					routeListFrom.TransferAddressTo(item, newItem);
				}

				//Пересчёт зарплаты после изменения МЛ
				routeListFrom.CalculateWages(_wageParameterService);
				routeListTo.CalculateWages(_wageParameterService);
				
				item.RecalculateTotalCash();
				newItem.RecalculateTotalCash();

				if(routeListTo.ClosingFilled)
				{
					newItem.FirstFillClosing(_wageParameterService);
				}

				UoW.Save(item);
				UoW.Save(newItem);

				UoW.Commit();
			}
			
			UpdateTranferDocuments(routeListFrom, routeListTo);

			if(routeListFrom.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListFrom.UpdateMovementOperations(_categoryRepository));
			}

			if(routeListTo.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListTo.UpdateMovementOperations(_categoryRepository));
			}

			UoW.Save(routeListTo);
			UoW.Save(routeListFrom);

			UoW.Commit();

			if(needReloadNotSet.Count > 0)
			{
				MessageDialogHelper.RunWarningDialog("Для следующих адресов не была указана необходимость загрузки, поэтому они не были перенесены:\n * " +
													string.Join("\n * ", needReloadNotSet.Select(x => x.Address))
												   );
			}

			if(needReloadSetAndRlEnRoute.Count > 0)
			{
                MessageDialogHelper.RunWarningDialog("Для следующих адресов была указана необходимость загрузки при переносе в МЛ со статусом \"В пути\" и выше , поэтому они не были перенесены:\n * " + 
                                                     string.Join("\n * ", needReloadSetAndRlEnRoute.Select(x => x.Address)));
            }

			if(fastDeliveryNotEnoughQuantity.Count > 0)
			{
				MessageDialogHelper.RunWarningDialog("Для следующих адресов c доставкой за час у водителя не хватает остатков, поэтому они не были перенесены:\n * " + 
				                                     string.Join("\n * ", fastDeliveryNotEnoughQuantity.Select(x => x.Address)));
			}

			if(messages.Count > 0)
			{
                MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
            }

			UpdateNodes();
			CheckSensitivities();
		}

		private void CheckSensitivities()
		{
			bool routeListToIsSelected = yentryreferenceRLTo.Subject != null;
			bool existToTransfer = ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>().Any(x => x.Status != RouteListItemStatus.Transfered);

			buttonTransfer.Sensitive = existToTransfer && routeListToIsSelected;
		}

		protected void OnButtonRevertClicked(object sender, EventArgs e)
		{
			var toRevert = ytreeviewRLTo
				.GetSelectedObjects<RouteListItemNode>()
				.Where(x => x.WasTransfered)
				.Select(x => x.RouteListItem)
				.ToList();
			
			foreach(var address in toRevert)
			{
				if(address.Status == RouteListItemStatus.Transfered)
				{
					MessageDialogHelper.RunWarningDialog(string.Format("Адрес {0} сам перенесен в МЛ №{1}. Отмена этого переноса не возможна. Сначала нужно отменить перенос в {1} МЛ.", address?.Order?.DeliveryPoint.ShortAddress, address.TransferedTo?.RouteList.Id));
					continue;
				}

				RouteListItem pastPlace = 
					(yentryreferenceRLFrom.Subject as RouteList)
						?.Addresses
						?.FirstOrDefault(x => x.TransferedTo != null && x.TransferedTo.Id == address.Id)
					?? _routeListItemRepository.GetTransferedFrom(UoW, address);

				var previousRouteList = pastPlace?.RouteList;

				if(pastPlace != null)
				{
					previousRouteList.RevertTransferAddress(_wageParameterService, pastPlace, address);
					pastPlace.NeedToReload = address.NeedToReload;
					pastPlace.WasTransfered = true;
					UpdateTranferDocuments(pastPlace.RouteList, address.RouteList);
					pastPlace.RecalculateTotalCash();
					UoW.Save(pastPlace);
					address.RouteList.TransferAddressTo(address, pastPlace);
					address.WasTransfered = false;
				}

				address.RouteList.CalculateWages(_wageParameterService);
				address.RecalculateTotalCash();

				UoW.Save(address.RouteList);
			}
			
			UoW.Commit();
			UpdateNodes();
		}

		private void UpdateTranferDocuments(RouteList from, RouteList to)
		{
			var addressTransferController = new AddressTransferController(new EmployeeRepository());
			addressTransferController.UpdateDocuments(from, to, UoW);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}

		#endregion

		#region Команды

		private DelegateCommand _transferTerminal = null;
		public DelegateCommand TransferTerminal => _transferTerminal ?? (_transferTerminal = new DelegateCommand(
			() => {
				var selectedNode = (yTreeViewDriverBalanceFrom.GetSelectedObject() as EmployeeBalanceNode);

				if (selectedNode != null)
				{
					if (selectedNode.Amount == 0)
					{
						MessageDialogHelper.RunErrorDialog("Вы не можете передавать терминал, т.к. его нет на балансе у водителя.", "Ошибка");
						return;
					}

					var routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
					var routeListTo = yentryreferenceRLTo.Subject as RouteList;

					var giveoutDocFrom = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, routeListFrom?.Driver);
					if(giveoutDocFrom is DriverAttachedTerminalGiveoutDocument)
					{
						MessageDialogHelper.RunErrorDialog(
							$"Нельзя передать терминал от водителя {routeListFrom?.Driver.GetPersonNameWithInitials()}, " +
							$"к которому привязан терминал.\r\nВодителю {routeListTo?.Driver.GetPersonNameWithInitials()}, " +
							"которому передается заказ, необходима допогрузка", "Ошибка");
						return;
					}

					if (ObservableDriverBalanceTo.Any(x => x.NomenclatureId == _terminalNomenclatureProvider.GetNomenclatureIdForTerminal 
					                                       && x.Amount > 0))
					{
						MessageDialogHelper.RunErrorDialog("У водителя уже есть терминал для оплаты.", "Ошибка");
						return;
					}
					
					var terminal = UoW.GetById<Nomenclature>(selectedNode.NomenclatureId);

					var operationFrom = new EmployeeNomenclatureMovementOperation
					{
						Employee = routeListFrom.Driver,
						Nomenclature = terminal,
						Amount = -1,
						OperationTime = DateTime.Now
					};
					
					var operationTo = new EmployeeNomenclatureMovementOperation
					{
						Employee = routeListTo.Driver,
						Nomenclature = terminal,
						Amount = 1,
						OperationTime = DateTime.Now
					};

					var driverTerminalTransferDocument = new AnotherDriverTerminalTransferDocument()
					{
						Author = _employeeService.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId),
						CreateDate = DateTime.Now,
						DriverFrom = routeListFrom.Driver,
						DriverTo = routeListTo.Driver,
						RouteListFrom = routeListFrom,
						RouteListTo = routeListTo,
						EmployeeNomenclatureMovementOperationFrom = operationFrom,
						EmployeeNomenclatureMovementOperationTo = operationTo
					};

					UoW.Save(driverTerminalTransferDocument);
					UoW.Save(operationFrom);
					UoW.Save(operationTo);
					UoW.Commit();

					FillObservableDriverBalance(ObservableDriverBalanceFrom, routeListFrom);
					FillObservableDriverBalance(ObservableDriverBalanceTo, routeListTo);
				}
			},
			() => yentryreferenceRLFrom.Subject != null && yentryreferenceRLTo.Subject != null
		));
		
		private DelegateCommand _revertTerminal = null;
		public DelegateCommand RevertTerminal => _revertTerminal ?? (_revertTerminal = new DelegateCommand(
			() => {
				var selectedNode = (yTreeViewDriverBalanceTo.GetSelectedObject() as EmployeeBalanceNode);

				if (selectedNode != null) {
					if (selectedNode.Amount == 0) {
						MessageDialogHelper.RunErrorDialog(
							"Вы не можете передавать терминал, т.к. его нет на балансе у водителя.", "Ошибка");
						return;
					}
					
					if (ObservableDriverBalanceFrom.Any(x => 
						x.NomenclatureId == _terminalNomenclatureProvider.GetNomenclatureIdForTerminal && x.Amount > 0)) {
						MessageDialogHelper.RunErrorDialog("У водителя уже есть терминал для оплаты.", "Ошибка");
						return;
					}

					var routeListFrom = yentryreferenceRLTo.Subject as RouteList;

					var giveoutDocTo = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, routeListFrom?.Driver);
					if(giveoutDocTo is DriverAttachedTerminalGiveoutDocument)
					{
						MessageDialogHelper.RunErrorDialog($"Нельзя вернуть терминал от водителя {routeListFrom?.Driver.GetPersonNameWithInitials()}" +
						                                   ", к которому привязан терминал.", "Ошибка");
						return;
					}

					var terminal = UoW.GetById<Nomenclature>(selectedNode.NomenclatureId);
					

					var operationFrom = new EmployeeNomenclatureMovementOperation {
						Employee = routeListFrom.Driver,
						Nomenclature = terminal,
						Amount = -1,
						OperationTime = DateTime.Now
					};

					var routeListTo = yentryreferenceRLFrom.Subject as RouteList;
					
					var operationTo = new EmployeeNomenclatureMovementOperation {
						Employee = routeListTo.Driver,
						Nomenclature = terminal,
						Amount = 1,
						OperationTime = DateTime.Now
					};

					var driverTerminalTransferDocument = new AnotherDriverTerminalTransferDocument()
					{
						Author = _employeeService.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId),
						CreateDate = DateTime.Now,
						DriverFrom = routeListFrom.Driver,
						DriverTo = routeListTo.Driver,
						RouteListFrom = routeListFrom,
						RouteListTo = routeListTo,
						EmployeeNomenclatureMovementOperationFrom = operationFrom,
						EmployeeNomenclatureMovementOperationTo = operationTo
					};

					UoW.Save(driverTerminalTransferDocument);
					UoW.Save(operationFrom);
					UoW.Save(operationTo);
					UoW.Commit();

					FillObservableDriverBalance(ObservableDriverBalanceTo, routeListFrom);
					FillObservableDriverBalance(ObservableDriverBalanceFrom, routeListTo);
				}
			},
			() => yentryreferenceRLFrom.Subject != null && yentryreferenceRLTo.Subject != null
		));

		#endregion
	}

	public class RouteListItemNode
	{
		public string Id => RouteListItem.Order.Id.ToString();
		public string Date => RouteListItem.Order.DeliveryDate.Value.ToString("d");
		public string Address => RouteListItem.Order.DeliveryPoint?.ShortAddress ?? "Нет адреса";
		public RouteListItemStatus Status => RouteListItem.Status;

		public bool NeedToReload => RouteListItem.NeedToReload;
		public bool IsFastDelivery => RouteListItem.Order.IsFastDelivery;

		bool _leftNeedToReload;
		public bool LeftNeedToReload
		{
			get => _leftNeedToReload;
			set
			{
				_leftNeedToReload = value;
				if(value)
				{
					_leftNotNeedToReload = false;
				}
			}
		}

		bool _leftNotNeedToReload;
		public bool LeftNotNeedToReload
		{
			get => _leftNotNeedToReload;
			set
			{
				_leftNotNeedToReload = value;
				if(value)
				{
					_leftNeedToReload = false;
				}
			}
		}

		public bool WasTransfered => RouteListItem.WasTransfered;
		public string Comment => RouteListItem.Comment ?? "";

		public string BottlesCount => 
			$"{(RouteListItem.Order.OrderItems.Where(bot => bot.Nomenclature.Category == NomenclatureCategory.water && !bot.Nomenclature.IsDisposableTare).Sum(bot => bot.Count)):N0}";

		public RouteListItem RouteListItem { get; set; }
		public string DalyNumber => RouteListItem.Order.DailyNumber.ToString();
		public bool NeedTerminal => RouteListItem.Order.PaymentType == PaymentType.Terminal;
	}
}

