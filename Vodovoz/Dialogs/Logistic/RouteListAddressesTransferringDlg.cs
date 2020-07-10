using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		WageParameterService wageParameterService = new WageParameterService(WageSingletonRepository.GetInstance(), new BaseParametersProvider());

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		public enum OpenParameter { Sender, Receiver }

		#endregion

		#region Конструкторы

		public RouteListAddressesTransferringDlg()
		{
			this.Build();
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		public RouteListAddressesTransferringDlg(RouteList routeList, OpenParameter param) : this()
		{
			var rl = UoW.GetById<RouteList>(routeList.Id);
			switch(param) {
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
				.AddColumn("Статус").AddEnumRenderer(node => node.Status);
			if(isRightPanel)
				config.AddColumn("Нужна загрузка").AddToggleRenderer(node => node.NeedToReload)
					  .AddSetter((c, n) => c.Sensitive = n.WasTransfered);
			else
				config.AddColumn("Нужна загрузка")
					  .AddToggleRenderer(x => x.LeftNeedToReload).Radio()
					  .AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered)
					  .AddTextRenderer(x => "Да")
					  .AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered)
					  .AddToggleRenderer(x => x.LeftNotNeedToReload).Radio()
					  .AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered)
					  .AddTextRenderer(x => "Нет")
					  .AddSetter((c, x) => c.Visible = x.Status != RouteListItemStatus.Transfered);

			return config.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.RowCells().AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.WasTransfered ? colorGreen : colorWhite)
				.Finish();
		}

		void YentryreferenceRLFrom_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRLFrom.Subject == null) {
				ytreeviewRLFrom.ItemsDataSource = null;
				return;
			}

			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo)) {
				yentryreferenceRLFrom.Subject = null;
				MessageDialogHelper.RunErrorDialog("Вы дурачёк?", "Вы не можете забирать адреса из того же МЛ, в который собираетесь передавать.");
				return;
			}

			if(TabParent != null) {
				var tab = TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeListFrom.Id));

				if(!(tab is RouteListClosingDlg)) {
					if(tab != null) {
						MessageDialogHelper.RunErrorDialog("Маршрутный лист уже открыт в другой вкладке");
						yentryreferenceRLFrom.Subject = null;
						return;
					}
				}
			}

			CheckSensitivities();

			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach(var item in routeListFrom.Addresses)
				items.Add(new RouteListItemNode { RouteListItem = item });
			ytreeviewRLFrom.ItemsDataSource = items;
		}

		void YentryreferenceRLTo_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRLTo.Subject == null) {
				ytreeviewRLTo.ItemsDataSource = null;
				return;
			}

			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;

			if(DomainHelper.EqualDomainObjects(routeListFrom, routeListTo)) {
				yentryreferenceRLTo.Subject = null;
				MessageDialogHelper.RunErrorDialog("Вы дурачёк?", "Вы не можете передавать адреса в тот же МЛ, из которого забираете.");
				return;
			}

			if(TabParent != null) {
				var tab = TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeListTo.Id));
				if(!(tab is RouteListClosingDlg)) {
					if(tab != null) {
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
				items.Add(new RouteListItemNode { RouteListItem = item });
			ytreeviewRLTo.ItemsDataSource = items;
		}

		private void UpdateNodes()
		{
			YentryreferenceRLFrom_Changed(null, null);
			YentryreferenceRLTo_Changed(null, null);
		}

		protected void OnButtonTransferClicked(object sender, EventArgs e)
		{
			//Дополнительные проверки
			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			var messages = new List<string>();

			if(routeListTo == null || routeListFrom == null || routeListTo.Id == routeListFrom.Id)
				return;

			List<RouteListItemNode> needReloadNotSet = new List<RouteListItemNode>();
			List<RouteListItemNode> needReloadSetAndRlEnRoute = new List<RouteListItemNode>();

			foreach(var row in ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>()) {
				RouteListItem item = row?.RouteListItem;
				logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

				if(item == null || item.Status == RouteListItemStatus.Transfered)
					continue;

				if(!row.LeftNeedToReload && !row.LeftNotNeedToReload) {
					needReloadNotSet.Add(row);
					continue;
				}

				if(row.LeftNeedToReload && routeListTo.Status >= RouteListStatus.EnRoute) {
					needReloadSetAndRlEnRoute.Add(row);
					continue;
				}

				RouteListItem newItem = new RouteListItem(routeListTo, item.Order, item.Status) {
					WasTransfered = true,
					NeedToReload = row.LeftNeedToReload,
					WithForwarder = routeListTo.Forwarder != null
				};
				routeListTo.ObservableAddresses.Add(newItem);

				item.TransferedTo = newItem;

				//Пересчёт зарплаты после изменения МЛ
				routeListFrom.CalculateWages(wageParameterService);
				routeListTo.CalculateWages(wageParameterService);

				if(routeListTo.ClosingFilled)
					newItem.FirstFillClosing(UoW, wageParameterService);
				UoW.Save(item);
				UoW.Save(newItem);
			}

			if(routeListFrom.Status == RouteListStatus.Closed) {
				messages.AddRange(routeListFrom.UpdateMovementOperations());
			}

			if(routeListTo.Status == RouteListStatus.Closed) {
				messages.AddRange(routeListTo.UpdateMovementOperations());
			}

			UoW.Save(routeListTo);
			UoW.Save(routeListFrom);

			UoW.Commit();

			if(needReloadNotSet.Count > 0)
				MessageDialogHelper.RunWarningDialog("Для следующих адресов не была указана необходимость загрузки, поэтому они не были перенесены:\n * " +
													String.Join("\n * ", needReloadNotSet.Select(x => x.Address))
												   );

			if(needReloadSetAndRlEnRoute.Count > 0)
				MessageDialogHelper.RunWarningDialog("Для следующих адресов была указана необходимость загрузки при переносе в МЛ со статусом \"В пути\" и выше , поэтому они не были перенесены:\n * " +
													String.Join("\n * ", needReloadSetAndRlEnRoute.Select(x => x.Address))
												   );

			if(messages.Count > 0)
				MessageDialogHelper.RunInfoDialog(String.Format("Были выполнены следующие действия:\n*{0}", String.Join("\n*", messages)));

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
			var toRevert = ytreeviewRLTo.GetSelectedObjects<RouteListItemNode>()
										.Where(x => x.WasTransfered).Select(x => x.RouteListItem);
			foreach(var address in toRevert) {
				if(address.Status == RouteListItemStatus.Transfered) {
					MessageDialogHelper.RunWarningDialog(String.Format("Адрес {0} сам перенесен в МЛ №{1}. Отмена этого переноса не возможна. Сначала нужно отменить перенос в {1} МЛ.", address?.Order?.DeliveryPoint.ShortAddress, address.TransferedTo?.RouteList.Id));
					continue;
				}

				RouteListItem pastPlace = null;
				if(yentryreferenceRLFrom.Subject != null) {
					pastPlace = (yentryreferenceRLFrom.Subject as RouteList)
						.Addresses.FirstOrDefault(x => x.TransferedTo != null && x.TransferedTo.Id == address.Id);
				}
				if(pastPlace == null) {
					pastPlace = new RouteListItemRepository().GetTransferedFrom(UoW, address);
				}

				if(pastPlace != null) {
					pastPlace.SetStatusWithoutOrderChange(address.Status);
					pastPlace.DriverBottlesReturned = address.DriverBottlesReturned;
					pastPlace.TransferedTo = null;
					if(pastPlace.RouteList.ClosingFilled)
						pastPlace.FirstFillClosing(UoW, wageParameterService);
					UoW.Save(pastPlace);
				}
				address.RouteList.ObservableAddresses.Remove(address);
				UoW.Save(address.RouteList);
			}

			UoW.Commit();
			UpdateNodes();
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}

		#endregion
	}

	public class RouteListItemNode
	{
		public string Id => RouteListItem.Order.Id.ToString();
		public string Date => RouteListItem.Order.DeliveryDate.Value.ToString("d");
		public string Address => RouteListItem.Order.DeliveryPoint?.ShortAddress ?? "Нет адреса";
		public RouteListItemStatus Status => RouteListItem.Status;

		public bool NeedToReload {
			get => RouteListItem.NeedToReload;
			set {
				if(RouteListItem.WasTransfered) {
					RouteListItem.NeedToReload = value;
					RouteListItem.RouteList.UoW.Save(RouteListItem);
					RouteListItem.RouteList.UoW.Commit();
				}
			}
		}

		bool leftNeedToReload;
		public bool LeftNeedToReload {
			get => leftNeedToReload;
			set {
				leftNeedToReload = value;
				if(value)
					leftNotNeedToReload = false;
			}
		}

		bool leftNotNeedToReload;
		public bool LeftNotNeedToReload {
			get => leftNotNeedToReload;
			set {
				leftNotNeedToReload = value;
				if(value)
					leftNeedToReload = false;
			}
		}

		public bool WasTransfered => RouteListItem.WasTransfered;
		public string Comment => RouteListItem.Comment ?? "";

		public string BottlesCount {
			get {
				return RouteListItem.Order.OrderItems
					.Where(bot => bot.Nomenclature.Category == NomenclatureCategory.water && !bot.Nomenclature.IsDisposableTare)
					.Sum(bot => bot.Count)
					.ToString();
			}

		}

		public RouteListItem RouteListItem { get; set; }
		public string DalyNumber => RouteListItem.Order.DailyNumber.ToString();
	}
}

