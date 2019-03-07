using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;
using QS.Project.Repositories;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region IOrmDialog implementation

		public IUnitOfWork UoW => uow;
		public enum OpenParameter { Sender, Receiver }

		#endregion

		#region Конструкторы

		public RouteListAddressesTransferringDlg()
		{
			this.Build();
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		public RouteListAddressesTransferringDlg (RouteList routeList, OpenParameter param) : this ()
		{
			var rl = UoW.GetById<RouteList> (routeList.Id);
			switch (param) {
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
			var vmFrom = new RouteListsVM ();
			vmFrom.Filter.OnlyStatuses = new [] {
				RouteListStatus.EnRoute,
				RouteListStatus.OnClosing
			};
			GC.KeepAlive(vmFrom);
			vmFrom.Filter.SetFilterDates (DateTime.Today.AddDays (-3), DateTime.Today.AddDays (1));
			yentryreferenceRLFrom.RepresentationModel = vmFrom;
			yentryreferenceRLFrom.CanEditReference = UserPermissionRepository.CurrentUserPresetPermissions["can_delete"];

			var vmTo = new RouteListsVM ();
			vmTo.Filter.OnlyStatuses = new [] {
				RouteListStatus.New,
				RouteListStatus.InLoading,
				RouteListStatus.EnRoute,
				RouteListStatus.OnClosing
			};
			vmTo.Filter.SetFilterDates (DateTime.Today.AddDays (-3), DateTime.Today.AddDays (1));
			yentryreferenceRLTo.RepresentationModel = vmTo;
			yentryreferenceRLTo.CanEditReference = UserPermissionRepository.CurrentUserPresetPermissions["can_delete"];

			yentryreferenceRLFrom.Changed += YentryreferenceRLFrom_Changed;
			yentryreferenceRLTo	 .Changed += YentryreferenceRLTo_Changed;

			//Для каждой TreeView нужен свой экземпляр ColumnsConfig
			ytreeviewRLFrom	.ColumnsConfig = GetColumnsConfig(false);
			ytreeviewRLTo	.ColumnsConfig = GetColumnsConfig(true);

			ytreeviewRLFrom .Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRLTo	.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewRLFrom .Selection.Changed += YtreeviewRLFrom_OnSelectionChanged;
			ytreeviewRLTo	.Selection.Changed += YtreeviewRLTo_OnSelectionChanged;
		}
		
		void YtreeviewRLFrom_OnSelectionChanged (object sender, EventArgs e)
		{
			CheckSensitivities();
		}

		void YtreeviewRLTo_OnSelectionChanged (object sender, EventArgs e)
		{
			CheckSensitivities();

			buttonRevert.Sensitive = ytreeviewRLTo.GetSelectedObjects<RouteListItemNode> ()
				.Any (x => x.WasTransfered);
		}

		private IColumnsConfig GetColumnsConfig (bool isRightPanel)
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

		void YentryreferenceRLFrom_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRLFrom.Subject == null)
			{
				ytreeviewRLFrom.ItemsDataSource = null;
				return;
			}
			
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;

			if (DomainHelper.EqualDomainObjects(routeListFrom, routeListTo)) {
				yentryreferenceRLFrom.Subject = null;
				MessageDialogWorks.RunErrorDialog ("Вы дурачёк?", "Вы не можете забирать адреса из того же МЛ, в который собираетесь передавать.");
				return;
			}

			if (TabParent != null) {
				var tab = TabParent.FindTab (OrmMain.GenerateDialogHashName<RouteList> (routeListFrom.Id));

				if (!(tab is RouteListClosingDlg)) { 
					if (tab != null) {
						MessageDialogWorks.RunErrorDialog ("Маршрутный лист уже открыт в другой вкладке");
						yentryreferenceRLFrom.Subject = null;
						return;
					}
				}
			}
			
			CheckSensitivities();

			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach (var item in routeListFrom.Addresses)
				items.Add(new RouteListItemNode{RouteListItem = item});
			ytreeviewRLFrom.ItemsDataSource = items;
		}

		void YentryreferenceRLTo_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRLTo.Subject == null)
			{
				ytreeviewRLTo.ItemsDataSource = null;
				return;
			}
			
			RouteList routeListTo = yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;

			if(DomainHelper.EqualDomainObjects (routeListFrom, routeListTo))
			{
				yentryreferenceRLTo.Subject = null;
				MessageDialogWorks.RunErrorDialog ("Вы дурачёк?", "Вы не можете передавать адреса в тот же МЛ, из которого забираете.");
				return;
			}

			if (TabParent != null) {
				var tab = TabParent.FindTab (OrmMain.GenerateDialogHashName<RouteList> (routeListTo.Id));
				if (!(tab is RouteListClosingDlg)) {
					if (tab != null) {
						MessageDialogWorks.RunErrorDialog ("Маршрутный лист уже открыт в другой вкладке");
						yentryreferenceRLTo.Subject = null;
						return;
					}
				}
			}

			CheckSensitivities();

			routeListTo.UoW = uow;

			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach (var item in routeListTo.Addresses)
				items.Add(new RouteListItemNode{RouteListItem = item});
			ytreeviewRLTo.ItemsDataSource = items;
		}

		private void UpdateNodes()
		{
			YentryreferenceRLFrom_Changed(null, null);
			YentryreferenceRLTo_Changed(null, null);
		}

		protected void OnButtonTransferClicked (object sender, EventArgs e)
		{
			//Дополнительные проверки
			RouteList routeListTo 	= yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;
			var messages = new List<string>();

			if (routeListTo == null || routeListFrom == null || routeListTo.Id == routeListFrom.Id)
				return;

			List<RouteListItemNode> needReloadNotSet = new List<RouteListItemNode>();
			
			foreach (var row in ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode>())
			{
				RouteListItem item = row?.RouteListItem;
				logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

				if (item == null || item.Status == RouteListItemStatus.Transfered)
					continue;

				if(!row.LeftNeedToReload && !row.LeftNotNeedToReload)
				{
					needReloadNotSet.Add(row);
					continue;
				}

				RouteListItem newItem = new RouteListItem(routeListTo, item.Order, item.Status);
				newItem.WasTransfered = true;
				newItem.NeedToReload = row.LeftNeedToReload;
				newItem.WithForwarder = routeListTo.Forwarder != null;
				routeListTo.ObservableAddresses.Add(newItem);

				item.TransferedTo = newItem;

				if (routeListTo.ClosingFilled)
					newItem.FirstFillClosing (UoW);
				UoW.Save (item);
				UoW.Save (newItem);
			}

			if(routeListFrom.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListFrom.UpdateMovementOperations());
			}

			if(routeListTo.Status == RouteListStatus.Closed)
			{
				messages.AddRange(routeListTo.UpdateMovementOperations());
			}

			uow.Save (routeListTo);
			uow.Save (routeListFrom);

			uow.Commit ();

			if(needReloadNotSet.Count > 0)
				MessageDialogWorks.RunWarningDialog("Для следующих адресов не была указана необходимость загрузки, поэтому они не были перенесены:\n * " +
				                                    String.Join("\n * ", needReloadNotSet.Select(x => x.Address))
												   );
			if(messages.Count > 0)
				MessageDialogWorks.RunInfoDialog(String.Format("Были выполнены следующие действия:\n*{0}", String.Join("\n*", messages)));

			UpdateNodes();
			CheckSensitivities ();
		}

		private void CheckSensitivities ()
		{
			bool routeListToIsSelected = yentryreferenceRLTo.Subject != null;
			bool existToTransfer = ytreeviewRLFrom.GetSelectedObjects<RouteListItemNode> ().Any (x => x.Status != RouteListItemStatus.Transfered);

			buttonTransfer.Sensitive = existToTransfer && routeListToIsSelected;
		}

		protected void OnButtonRevertClicked (object sender, EventArgs e)
		{
			var toRevert = ytreeviewRLTo.GetSelectedObjects<RouteListItemNode> ()
			                            .Where (x => x.WasTransfered).Select (x => x.RouteListItem);
			foreach(var address in toRevert)
			{
				if(address.Status == RouteListItemStatus.Transfered)
				{
					MessageDialogWorks.RunWarningDialog (String.Format ("Адрес {0} сам перенесен в МЛ №{1}. Отмена этого переноса не возможна. Сначала нужно отменить перенос в {1} МЛ.", address?.Order?.DeliveryPoint.ShortAddress, address.TransferedTo?.RouteList.Id));
					continue;
				}

				RouteListItem pastPlace = null;
				if (yentryreferenceRLFrom.Subject != null)
				{
					pastPlace = (yentryreferenceRLFrom.Subject as RouteList)
						.Addresses.FirstOrDefault (x => x.TransferedTo != null && x.TransferedTo.Id == address.Id);
				}
				if(pastPlace == null)
				{
					pastPlace = Repository.Logistics.RouteListItemRepository.GetTransferedFrom (UoW, address);
				}

				if(pastPlace != null)
				{
					pastPlace.SetStatusWithoutOrderChange (address.Status);
					pastPlace.DriverBottlesReturned = address.DriverBottlesReturned;
					pastPlace.TransferedTo = null;
					if (pastPlace.RouteList.ClosingFilled)
						pastPlace.FirstFillClosing (UoW);
					UoW.Save (pastPlace);
				}
				address.RouteList.ObservableAddresses.Remove (address);
				UoW.Save (address.RouteList);
			}

			UoW.Commit ();
			UpdateNodes ();
		}

		#endregion
	}

	public class RouteListItemNode {
		public string Id => RouteListItem.Order.Id.ToString();
		public string Date => RouteListItem.Order.DeliveryDate.Value.ToString("d");
		public string Address => RouteListItem.Order.DeliveryPoint?.ShortAddress ?? "Нет адреса";
		public RouteListItemStatus Status => RouteListItem.Status;

		public bool NeedToReload {
			get => RouteListItem.NeedToReload;
			set {
				if(RouteListItem.WasTransfered)
				{
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

