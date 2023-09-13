using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets.Cells;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.Settings.Cash;
using Vodovoz.Infrastructure;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : TdiTabBase, ISingleUoWDialog
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();

		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider));
		private readonly IEmployeeNomenclatureMovementRepository _employeeNomenclatureMovementRepository;
		private readonly ITerminalNomenclatureProvider _terminalNomenclatureProvider;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IEmployeeService _employeeService;
		private readonly ICommonServices _commonServices;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;
		private readonly IEmployeeRepository _employeeRepository;

		private IRouteListProfitabilityController _routeListProfitabilityController;
		private IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;

		private GenericObservableList<EmployeeBalanceNode> ObservableDriverBalanceFrom { get; set; } = new GenericObservableList<EmployeeBalanceNode>();
		private GenericObservableList<EmployeeBalanceNode> ObservableDriverBalanceTo { get; set; } = new GenericObservableList<EmployeeBalanceNode>();

		private DeliveryFreeBalanceViewModel _deliveryFreeBalanceViewModelFrom;
		private DeliveryFreeBalanceViewModel _deliveryFreeBalanceViewModelTo;
		private RouteListJournalFilterViewModel _routeListJournalFilterViewModelFrom;
		private RouteListJournalFilterViewModel _routeListJournalFilterViewModelTo;
		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		public enum OpenParameter { Sender, Receiver }

		#endregion

		#region Конструкторы

		public RouteListAddressesTransferringDlg(IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository,
			ITerminalNomenclatureProvider terminalNomenclatureProvider,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IEmployeeRepository employeeRepository,
			INomenclatureParametersProvider nomenclatureParametersProvider
			)
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
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

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
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IEmployeeRepository employeeRepository,
			INomenclatureParametersProvider nomenclatureParametersProvider
			)
			: this(
				employeeNomenclatureMovementRepository,
				terminalNomenclatureProvider,
				routeListRepository,
				routeListItemRepository,
				employeeService,
				commonServices,
				financialCategoriesGroupsSettings,
				employeeRepository,
				nomenclatureParametersProvider)
		{
			var rl = UoW.GetById<RouteList>(routeListId);

			switch(param)
			{
				case OpenParameter.Sender:
					evmeRouteListFrom.Subject = rl;
					break;
				case OpenParameter.Receiver:
					evmeRouteListTo.Subject = rl;
					break;
			}
		}

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			hpanedMain.Position = Screen.RootWindow.FrameExtents.Width / 2;

			var nomenclatureParametersProvider = new NomenclatureParametersProvider(_parametersProvider);
			
			_routeListProfitabilityController = new RouteListProfitabilityController(new RouteListProfitabilityFactory(),
				nomenclatureParametersProvider, new ProfitabilityConstantsRepository(),
				new RouteListProfitabilityRepository(), _routeListRepository, new NomenclatureRepository(nomenclatureParametersProvider));

			_routeListAddressKeepingDocumentController = new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureParametersProvider);
			

			IRouteListJournalFactory routeListJournalFactory = new RouteListJournalFactory();
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			_routeListJournalFilterViewModelFrom = new RouteListJournalFilterViewModel()
			{
				DisplayableStatuses = new[] { RouteListStatus.EnRoute },
				StartDate = DateTime.Today.AddDays(-3),
				EndDate = DateTime.Today.AddDays(1)
			};

			_routeListJournalFilterViewModelFrom.AddressTypeNodes.ForEach(x => x.Selected = true);

			evmeRouteListFrom.SetEntityAutocompleteSelectorFactory(routeListJournalFactory
				.CreateRouteListJournalAutocompleteSelectorFactory(scope, _routeListJournalFilterViewModelFrom));

			_routeListJournalFilterViewModelTo = new RouteListJournalFilterViewModel()
			{
				DisplayableStatuses = new[] {
					RouteListStatus.New,
					RouteListStatus.InLoading,
					RouteListStatus.EnRoute,
					RouteListStatus.OnClosing
				},
				StartDate = DateTime.Today.AddDays(-3),
				EndDate = DateTime.Today.AddDays(1)
			};

			_routeListJournalFilterViewModelTo.AddressTypeNodes.ForEach(x => x.Selected = true);

			evmeRouteListTo.SetEntityAutocompleteSelectorFactory(routeListJournalFactory
				.CreateRouteListJournalAutocompleteSelectorFactory(scope, _routeListJournalFilterViewModelTo));

			evmeRouteListFrom.Changed += OnRouteListFromChanged;
			evmeRouteListTo.Changed += OnRouteListToChanged;

			//Для каждой TreeView нужен свой экземпляр ColumnsConfig
			ytreeviewRLFrom.ColumnsConfig = GetColumnsConfig(false);
			ytreeviewRLTo.ColumnsConfig = GetColumnsConfig(true);

			ytreeviewRLFrom.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRLTo.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewRLFrom.Selection.Changed += YtreeviewRLFrom_OnSelectionChanged;
			ytreeviewRLTo.Selection.Changed += YtreeviewRLTo_OnSelectionChanged;

			ytreeviewRLFrom.ItemsDataSource = RouteListItemsFrom;

			ConfigureTreeViewsDriverBalance();

			ybtnTransferTerminal.Clicked += (sender, e) => TransferTerminal.Execute();
			ybtnRevertTerminal.Clicked += (sender, e) => RevertTerminal.Execute();

			ybuttonAddOrder.Clicked += (sender, e) => AddOrderToRouteListEnRouteCommand.Execute();

			_deliveryFreeBalanceViewModelFrom = new DeliveryFreeBalanceViewModel();
			var deliveryfreebalanceviewFrom = new DeliveryFreeBalanceView(_deliveryFreeBalanceViewModelFrom);
			deliveryfreebalanceviewFrom.ShowAll();
			yhboxDeliveryFreeBalanceFrom.PackStart(deliveryfreebalanceviewFrom, true, true, 0);

			_deliveryFreeBalanceViewModelTo = new DeliveryFreeBalanceViewModel();
			var deliveryfreebalanceviewTo = new DeliveryFreeBalanceView(_deliveryFreeBalanceViewModelTo);
			deliveryfreebalanceviewTo.ShowAll();
			yhboxDeliveryFreeBalanceTo.PackStart(deliveryfreebalanceviewTo, true, true, 0);
		}

		void YtreeviewRLFrom_OnSelectionChanged(object sender, EventArgs e)
		{
			CheckSensitivities();
		}

		void YtreeviewRLTo_OnSelectionChanged(object sender, EventArgs e)
		{
			CheckSensitivities();

			var selectedNodes = ytreeviewRLTo.GetSelectedObjects<RouteListItemNode>();

			buttonRevert.Sensitive = selectedNodes
				.Any(x =>
					x.Status == RouteListItemStatus.EnRoute
					&& (x.WasTransfered
					    || (x.IsFromFreeBalance && x.Status != RouteListItemStatus.Transfered)));
		}

		private IColumnsConfig GetColumnsConfig(bool isRightPanel)
		{
			var colorGreen = GdkColors.Green;
			var basePrimary = GdkColors.PrimaryBase;

			var config = ColumnsConfigFactory.Create<RouteListItemNode>()
				.AddColumn("Еж.\nномер").AddTextRenderer(node => node.DalyNumber)
				.AddColumn("Заказ").AddTextRenderer(node => node.Id)
				.AddColumn("Дата").AddTextRenderer(node => node.Date)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Бутыли").AddTextRenderer(node => node.BottlesCount)
				.AddColumn("Статус").AddTextRenderer(node => node.RouteListItem.RouteList == null ? "" : node.Status.GetEnumTitle())
				.AddColumn("Доставка\nза час")
					.AddToggleRenderer(x => x.IsFastDelivery).Editing(false);

			if(isRightPanel)
			{
				config.AddColumn("Тип переноса").AddTextRenderer(node => node.RouteListItem.AddressTransferType.HasValue ? node.RouteListItem.AddressTransferType.GetEnumTitle() : "");
			}
			else
			{
				config.AddColumn("Тип переноса")
					.AddToggleRenderer(x => x.IsNeedToReload).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddTextRenderer(x => AddressTransferType.NeedToReload.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddToggleRenderer(x => x.IsFromHandToHandTransfer).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddTextRenderer(x => AddressTransferType.FromHandToHand.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddToggleRenderer(x => x.IsFromFreeBalance).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, false))
					.AddTextRenderer(x => AddressTransferType.FromFreeBalance.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, false));
			}

			return config.AddColumn("Нужен\nтерминал").AddToggleRenderer(x => x.NeedTerminal).Editing(false)
			             .AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
			             .RowCells().AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.WasTransfered ? colorGreen : basePrimary)
			             .Finish();
		}

		private void ApplyCellRendererSetter(CellRenderer nodeCellRenderer, RouteListItemNode routeListItemNode, bool isHiddenForNewOrder)
		{
			if(routeListItemNode.RouteListItem.RouteList == null /* && routeListItemNode.IsFromFreeBalance */&& isHiddenForNewOrder)
			{
				nodeCellRenderer.Visible = false;

				return;
			}

			var isActive = routeListItemNode.Status != RouteListItemStatus.Transfered
			               && routeListItemNode.RouteListItem.RouteList != null;

			nodeCellRenderer.Sensitive = isActive;

			if(nodeCellRenderer is NodeCellRendererToggle<RouteListItemNode> toggle)
			{
				toggle.Activatable = isActive;
			}
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

        private void OnRouteListFromChanged(object sender, EventArgs e)
        {
            RouteListItemsFrom.Clear();

            ybuttonAddOrder.Sensitive = evmeRouteListFrom.Subject == null;

			if(evmeRouteListFrom.Subject == null)
            {
	            _deliveryFreeBalanceViewModelFrom.ObservableDeliveryFreeBalanceOperations = new GenericObservableList<DeliveryFreeBalanceOperation>();
	            return;
            }

            RouteList routeListFrom = evmeRouteListFrom.Subject as RouteList;
            RouteList routeListTo = evmeRouteListTo.Subject as RouteList;

            _deliveryFreeBalanceViewModelFrom.ObservableDeliveryFreeBalanceOperations = routeListFrom.ObservableDeliveryFreeBalanceOperations;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo))
            {
                evmeRouteListFrom.Subject = null;
                MessageDialogHelper.RunErrorDialog("Вы не можете забирать адреса из того же МЛ, в который собираетесь передавать.");
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
                        evmeRouteListFrom.Subject = null;
                        return;
                    }
                }
            }

            CheckSensitivities();

            foreach(var item in routeListFrom.Addresses)
            {
                RouteListItemsFrom.Add(new RouteListItemNode { RouteListItem = item });
            }

            FillObservableDriverBalance(ObservableDriverBalanceFrom, routeListFrom);

            ytreeviewRLFrom.ColumnsConfig = GetColumnsConfig(false);

			ytreeviewRLFrom.YTreeModel.EmitModelChanged();

		}

        private void OnRouteListToChanged(object sender, EventArgs e)
        {
	        if(evmeRouteListTo.Subject == null)
            {
                ytreeviewRLTo.ItemsDataSource = null;
                _deliveryFreeBalanceViewModelTo.ObservableDeliveryFreeBalanceOperations = new GenericObservableList<DeliveryFreeBalanceOperation>();

				return;
            }

            RouteList routeListTo = evmeRouteListTo.Subject as RouteList;
            RouteList routeListFrom = evmeRouteListFrom.Subject as RouteList;

            _deliveryFreeBalanceViewModelTo.ObservableDeliveryFreeBalanceOperations = routeListTo.ObservableDeliveryFreeBalanceOperations;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo))
            {
                evmeRouteListTo.Subject = null;
                MessageDialogHelper.RunErrorDialog("Вы не можете передавать адреса в тот же МЛ, из которого забираете.");
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
                        evmeRouteListTo.Subject = null;
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
			OnRouteListFromChanged(null, null);
			OnRouteListToChanged(null, null);
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
            var routeListFrom = evmeRouteListFrom.Subject as RouteList;
            var routeListTo = evmeRouteListTo.Subject as RouteList;
            var deliveryNotEnoughQuantityList = new List<RouteListItemNode>();

            if(routeListFrom == null && routeListTo != null)
            {
	            TransferAddressWithoutRouteList(routeListTo);
            }

            if(routeListTo == null || routeListFrom == null || routeListTo.Id == routeListFrom.Id)
			{
				return;
			}

            var messages = new List<string>();

            List<RouteListItemNode> transferTypeNotSet = new List<RouteListItemNode>();
			List<RouteListItemNode> transferTypeSetAndRlEnRoute = new List<RouteListItemNode>();

			foreach(var row in ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>())
			{
				RouteListItem item = row?.RouteListItem;
				_logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

				if(item == null || item.Status == RouteListItemStatus.Transfered)
				{
					continue;
				}

				if(!row.IsNeedToReload && !row.IsFromHandToHandTransfer && !row.IsFromFreeBalance)
				{
					transferTypeNotSet.Add(row);
					continue;
				}

				if(row.IsNeedToReload && routeListTo.Status >= RouteListStatus.EnRoute)
				{
					transferTypeSetAndRlEnRoute.Add(row);
					continue;
				}

				if(row.IsFromFreeBalance)
				{
					var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(UoW, row.RouteListItem.Order, routeListTo);

					if(!hasBalanceForTransfer)
					{
						deliveryNotEnoughQuantityList.Add(row);
						continue;
					}
				}

				if(HasAddressChanges(item))
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Статус {item.Title} был изменён другим пользователем, для его переноса переоткройте диалог.");
					continue;
				}

				var transferredAddressFromRouteListTo =
					_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(UoW, routeListTo.Id, item.Order.Id);

				RouteListItem newItem = null;

				if(transferredAddressFromRouteListTo != null)
				{
					newItem = transferredAddressFromRouteListTo;
					newItem.AddressTransferType = item.AddressTransferType;
					item.WasTransfered = false;
					routeListTo.RevertTransferAddress(_wageParameterService, newItem, item);
					routeListFrom.TransferAddressTo(UoW, item, newItem);
					newItem.WasTransfered = true;
				}
				else
				{
					newItem = new RouteListItem(routeListTo, item.Order, item.Status)
					{
						WasTransfered = true,
						AddressTransferType = row.IsNeedToReload
							? AddressTransferType.NeedToReload
							: row.IsFromHandToHandTransfer
								? AddressTransferType.FromHandToHand
								: AddressTransferType.FromFreeBalance,
						WithForwarder = routeListTo.Forwarder != null
					};

					routeListTo.ObservableAddresses.Add(newItem);
					routeListFrom.TransferAddressTo(UoW, item, newItem);
				}

				if(routeListTo.Status == RouteListStatus.New)
				{
					if(item.AddressTransferType == AddressTransferType.NeedToReload)
					{
						item.Order.ChangeStatus(OrderStatus.InTravelList);
					}
					if(item.AddressTransferType == AddressTransferType.FromHandToHand)
					{
						item.Order.ChangeStatus(OrderStatus.OnLoading);
					}
				}

				//Пересчёт зарплаты после изменения МЛ
				routeListFrom.CalculateWages(_wageParameterService);
				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, routeListFrom);
				routeListTo.CalculateWages(_wageParameterService);
				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, routeListTo);
					
				item.RecalculateTotalCash();
				newItem.RecalculateTotalCash();

				if(routeListTo.ClosingFilled)
				{
					newItem.FirstFillClosing(_wageParameterService);
				}

				UoW.Save(item);
				UoW.Save(newItem);

				UpdateTranferDocuments(item, newItem);

				UoW.Commit();
			}
			

			if(routeListFrom.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListFrom.UpdateMovementOperations(_financialCategoriesGroupsSettings));
			}

			if(routeListTo.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListTo.UpdateMovementOperations(_financialCategoriesGroupsSettings));
			}

			UoW.Save(routeListTo);
			UoW.Save(routeListFrom);

			UoW.Commit();

			if(transferTypeNotSet.Count > 0)
			{
				MessageDialogHelper.RunWarningDialog("Для следующих адресов не была указана необходимость загрузки, поэтому они не были перенесены:\n * " +
													string.Join("\n * ", transferTypeNotSet.Select(x => x.Address))
												   );
			}

			if(transferTypeSetAndRlEnRoute.Count > 0)
			{
                MessageDialogHelper.RunWarningDialog("Для следующих адресов была указана необходимость загрузки при переносе в МЛ со статусом \"В пути\" и выше , поэтому они не были перенесены:\n * " + 
                                                     string.Join("\n * ", transferTypeSetAndRlEnRoute.Select(x => x.Address)));
            }

			ShowNotEnoughQuantityMessageIfNeeded(deliveryNotEnoughQuantityList);

			if(messages.Count > 0)
			{
                MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
            }

			UpdateNodes();
			CheckSensitivities();
		}

        private void TransferAddressWithoutRouteList(RouteList routeListTo)
        {
            var deliveryNotEnoughQuantityList = new List<RouteListItemNode>();
            var invalidTransferTypeList = new List<RouteListItemNode>();

            foreach(var row in ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>())
            {
                if(row.IsFromFreeBalance)
                {
                    var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(UoW, row.RouteListItem.Order, routeListTo);

                    if(!hasBalanceForTransfer)
                    {
                        deliveryNotEnoughQuantityList.Add(row);
                        continue;
                    }

                    AddOrderInRouteListEnRoute(routeListTo, row.RouteListItem.Order);

                    RouteListItemsFrom.Remove(row);
                }
                else
                {
                    invalidTransferTypeList.Add(row);
                }
            }

            ShowNotEnoughQuantityMessageIfNeeded(deliveryNotEnoughQuantityList);
            ShowInvalidTransferTypeMessageIfNeeded(invalidTransferTypeList);
        }

        private void ShowNotEnoughQuantityMessageIfNeeded(List<RouteListItemNode> deliveryNotEnoughQuantityList)
        {
            if(deliveryNotEnoughQuantityList.Count > 0)
            {
                _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
                    "Для следующих адресов у водителя не хватает остатков, поэтому они не были перенесены:\n * " +
                    string.Join("\n * ", deliveryNotEnoughQuantityList.Select(x => x.Address)));
            }
        }

        private void ShowInvalidTransferTypeMessageIfNeeded(List<RouteListItemNode> invalidTransferTypeList)
        {
            if(invalidTransferTypeList.Count > 0)
            {
                _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
                    "Для следующих адресов выбран неверный тип переноса, пожтому они не были перенесены:\n * " +
                    string.Join("\n * ", invalidTransferTypeList.Select(x => x.Address)));
            }
        }

        private void CheckSensitivities()
		{
			bool routeListToIsSelected = evmeRouteListTo.Subject != null;
			var selectedNodes = ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>();
			buttonTransfer.Sensitive = selectedNodes.All(x => x.Status == RouteListItemStatus.EnRoute)
									   && routeListToIsSelected;
		}

		protected void OnButtonRevertClicked(object sender, EventArgs e)
		{
			List<string> deliveryNotEnoughQuantityAddresses = new List<string>();

            var addedEnRouteWithoutPastPlace = ytreeviewRLTo
                .GetSelectedObjects<RouteListItemNode>()
                .Where(x => !x.WasTransfered && x.IsFromFreeBalance && x.Status != RouteListItemStatus.Transfered)
                .Select(x => x.RouteListItem)
                .ToList();

            foreach(var address in addedEnRouteWithoutPastPlace)
            {
                var routeList = address.RouteList;

                address.Order.ChangeStatus(OrderStatus.Accepted);

                routeList.Addresses.Remove(address);

                routeList.CalculateWages(_wageParameterService);
                _routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, address.RouteList);

                var routeListKeepingDocument = UoW.GetAll<RouteListAddressKeepingDocument>()
                    .Where(x => x.RouteListItem.Id == address.Id)
                    .SingleOrDefault();

                foreach(var item in routeListKeepingDocument.Items)
                {
                    routeList.ObservableDeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
                }

                UoW.Delete(routeListKeepingDocument);

                UoW.Delete(address);

                UoW.Save(address.RouteList);

                UoW.Commit();

                if(evmeRouteListFrom.Subject == null
                   && RouteListItemsFrom.All(x => x.RouteListItem.Order.Id != address.Order.Id))
                {
                    var newRouteListItem = new RouteListItem
                    {
                        Order = address.Order,
                        AddressTransferType = AddressTransferType.FromFreeBalance
                    };

                    RouteListItemsFrom.Add(new RouteListItemNode { RouteListItem = newRouteListItem });
                }

                return;
            }

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

				if(HasAddressChanges(address))
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Адрес {address.Title} был изменён другим пользователем, переоткройте диалог.");
					continue;
				}

				RouteListItem pastPlace = 
					(evmeRouteListFrom.Subject as RouteList)
						?.Addresses
						?.FirstOrDefault(x => x.TransferedTo != null && x.TransferedTo.Id == address.Id)
					?? _routeListItemRepository.GetTransferedFrom(UoW, address);

				var previousRouteList = pastPlace?.RouteList;

				if(pastPlace != null)
				{
					if(pastPlace.TransferedTo.AddressTransferType.Value == AddressTransferType.FromFreeBalance)
					{
						var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(UoW, address.Order, pastPlace.RouteList);

						if(!hasBalanceForTransfer)
						{
							deliveryNotEnoughQuantityAddresses.Add(address.Title);
							continue;
						}
					}

					previousRouteList.RevertTransferAddress(_wageParameterService, pastPlace, address);
					pastPlace.AddressTransferType = address.AddressTransferType;
					pastPlace.WasTransfered = true;
					UpdateTranferDocuments(address, pastPlace);
					pastPlace.RecalculateTotalCash();
					UoW.Save(pastPlace);
					address.RouteList.TransferAddressTo(UoW, address, pastPlace);
					address.WasTransfered = false;
				}

				address.RouteList.CalculateWages(_wageParameterService);
				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, address.RouteList);
				address.RecalculateTotalCash();

				UoW.Save(address.RouteList);
			}

			UoW.Commit();
			UpdateNodes();

			if(deliveryNotEnoughQuantityAddresses.Count > 0)
			{
				MessageDialogHelper.RunWarningDialog("Для следующих адресов у водителя не хватает остатков, поэтому они не были перенесены:\n * " +
				                                     string.Join("\n * ", deliveryNotEnoughQuantityAddresses));
			}
		}

		private bool HasAddressChanges(RouteListItem address)
		{
			RouteListItemStatus actualStatus;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Получение статуса адреса"))
			{
				actualStatus = uow.GetById<RouteListItem>(address.Id).Status;
			}
			
			if(actualStatus == address.Status)
			{
				return false;
			}

			return true;
		}

		private void UpdateTranferDocuments(RouteListItem from, RouteListItem to)
		{
			var addressTransferController = new AddressTransferController(new EmployeeRepository());
			addressTransferController.UpdateDocuments(from, to, UoW);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			_routeListJournalFilterViewModelFrom?.Dispose();
			_routeListJournalFilterViewModelTo?.Dispose();
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

					var routeListFrom = evmeRouteListFrom.Subject as RouteList;
					var routeListTo = evmeRouteListTo.Subject as RouteList;

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
			() => evmeRouteListFrom.Subject != null && evmeRouteListTo.Subject != null
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

					var routeListFrom = evmeRouteListTo.Subject as RouteList;

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

					var routeListTo = evmeRouteListFrom.Subject as RouteList;
					
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
			() => evmeRouteListFrom.Subject != null && evmeRouteListTo.Subject != null
		));


		private DelegateCommand _addOrderCommand;
        public DelegateCommand AddOrderToRouteListEnRouteCommand => _addOrderCommand ?? (_addOrderCommand = new DelegateCommand(
            () =>
            {
                SelectNewOrdersForRouteListEnRoute();
            }
        ));

        #endregion

        private void SelectNewOrdersForRouteListEnRoute()
        {
	        var routeListItemsTo = evmeRouteListTo.Subject as RouteList;
	        var routeListToItems = routeListItemsTo?.Addresses.Select(t => t.Order.Id) ?? new List<int>();

	        var filter = new OrderJournalFilterViewModel(
                new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()),
                new DeliveryPointJournalFactory(),
                new EmployeeJournalFactory())
            {
				ExceptIds = RouteListItemsFrom.Select(f => f.RouteListItem.Order.Id)
					.Concat(routeListToItems)
					.ToArray()
			};

            filter.SetAndRefilterAtOnce(
                x => x.RestrictFilterDateType = OrdersDateFilterType.DeliveryDate,
                x => x.RestrictStatus = OrderStatus.Accepted,
                x => x.RestrictWithoutSelfDelivery = true,
                x => x.RestrictOnlySelfDelivery = false,
                x => x.RestrictHideService = true,
                x => x.ExcludeClosingDocumentDeliverySchedule = true
            );

            var orderPage = Startup.MainWin.NavigationManager.OpenViewModel<OrderForRouteListJournalViewModel, OrderJournalFilterViewModel>(null, filter);
            orderPage.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
            orderPage.ViewModel.OnEntitySelectedResult += OnOrderSelectedResult;
        }

        private void OnOrderSelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			foreach(var selectedNode in e.SelectedNodes)
			{
				var order = UoW.GetById<Order>(selectedNode.Id);

				var newRouteListItem = new RouteListItem
				{
					Order = order,
					AddressTransferType = AddressTransferType.FromFreeBalance
				};

				RouteListItemsFrom.Add(new RouteListItemNode { RouteListItem = newRouteListItem });
			}
			ytreeviewRLFrom.ColumnsConfig = GetColumnsConfig(false);
			ytreeviewRLFrom.YTreeModel.EmitModelChanged();
		}

        private void AddOrderInRouteListEnRoute(RouteList routeList, Order order)
        {
            var newRouteListItem = new RouteListItem(routeList, order, RouteListItemStatus.EnRoute)
            {
                WithForwarder = routeList.Forwarder != null,
                AddressTransferType = AddressTransferType.FromFreeBalance
            };

            routeList.ObservableAddresses.Add(newRouteListItem);

            routeList.CalculateWages(_wageParameterService);

            _routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, routeList);

            newRouteListItem.RecalculateTotalCash();

            if(routeList.ClosingFilled)
            {
                newRouteListItem.FirstFillClosing(_wageParameterService);
            }

            UoW.Save(newRouteListItem);

            _routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(UoW, newRouteListItem, DeliveryFreeBalanceType.Decrease, needRouteListUpdate: true);

            UoW.Commit();
        }

        GenericObservableList<RouteListItemNode> RouteListItemsFrom = new GenericObservableList<RouteListItemNode>();
    }

	public class RouteListItemNode
	{
		public string Id => RouteListItem.Order.Id.ToString();
		public string Date => RouteListItem.Order.DeliveryDate.Value.ToString("d");
		public string Address => RouteListItem.Order.DeliveryPoint?.ShortAddress ?? "Нет адреса";
		public RouteListItemStatus Status => RouteListItem.Status;

		public bool IsNeedToReload
		{
			get => RouteListItem.AddressTransferType == AddressTransferType.NeedToReload;
			set
			{
				if(value)
				{
					RouteListItem.AddressTransferType = AddressTransferType.NeedToReload;
				}
				else
				{
					RouteListItem.AddressTransferType = null;
				}
			}
		}
		
		public bool IsFromHandToHandTransfer
		{
			get => RouteListItem.AddressTransferType == AddressTransferType.FromHandToHand;
			set
			{
				if(value)
				{
					RouteListItem.AddressTransferType = AddressTransferType.FromHandToHand;
				}
				else
				{
					RouteListItem.AddressTransferType = null;
				}
			}
		}

		public bool IsFromFreeBalance
		{
			get => RouteListItem.AddressTransferType == AddressTransferType.FromFreeBalance;
			set
			{
				if(value)
				{
					RouteListItem.AddressTransferType = AddressTransferType.FromFreeBalance;
				}
				else
				{
					RouteListItem.AddressTransferType = null;
				}
			}
		}

		public bool IsFastDelivery => RouteListItem.Order.IsFastDelivery;

		public bool WasTransfered => RouteListItem.WasTransfered;
		public string Comment => RouteListItem.Comment ?? "";

		public string BottlesCount => 
			$"{(RouteListItem.Order.OrderItems.Where(bot => bot.Nomenclature.Category == NomenclatureCategory.water && !bot.Nomenclature.IsDisposableTare).Sum(bot => bot.Count)):N0}";

		public RouteListItem RouteListItem { get; set; }
		public string DalyNumber => RouteListItem.Order.DailyNumber.ToString();
		public bool NeedTerminal => RouteListItem.Order.PaymentType == PaymentType.Terminal;
	}
}

