using System;
using QSTDI;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using Vodovoz.Domain.Documents;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz
{
	public partial class TransferGoodsBetweenRLDlg : TdiTabBase, ITdiDialog, IOrmDialog
	{
		#region Поля

		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		IColumnsConfig colConfigFrom = ColumnsConfigFactory.Create<CarUnloadDocumentNode>()
			.AddColumn("Номенклатура").AddTextRenderer(d => d.Nomenclature)
			.AddColumn("Кол-во").AddNumericRenderer(d => d.ItemsCount)
			.AddColumn("Перенести").AddNumericRenderer(d => d.TransferCount)
				.Adjustment(new Gtk.Adjustment(0, 0, 1000, 1, 1, 1)).Editing()
			.AddColumn("Остаток").AddNumericRenderer(d => d.Residue)
			.Finish();

		IColumnsConfig colConfigTo = ColumnsConfigFactory.Create<CarUnloadDocumentNode>()
			.AddColumn("Номенклатура").AddTextRenderer(d => d.Nomenclature)
			.AddColumn("Кол-во").AddNumericRenderer(d => d.ItemsCount)
			.Finish();
		
		#endregion

		#region Конструкторы

		public TransferGoodsBetweenRLDlg()
		{
			this.Build();
			this.TabName = "Перенос разгрузок";
			ConfigureDlg();
		}

		public TransferGoodsBetweenRLDlg(RouteList routeList, OpenParameter param) : this()
		{
			switch (param)
			{
				case OpenParameter.Sender:
					yentryreferenceRouteListFrom.Subject = routeList;
					break;
				case OpenParameter.Receiver:
					yentryreferenceRouteListTo.Subject = routeList;
					break;
			}
		}

		#endregion

		#region ITdiDialog implementation

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			UoW.Commit();

			CheckSensitivities();
			return true;
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges {
			get {
				return UoW.HasChanges;
			}
		}

		#endregion

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get { return uow; } }

		public object EntityObject { get { return null; } }

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			//Настройка элементов откуда переносим
			RouteListsFilter filterFrom = new RouteListsFilter(UoW);
			filterFrom.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListFrom.RepresentationModel = new ViewModel.RouteListsVM(filterFrom);
			yentryreferenceRouteListFrom.Changed += YentryreferenceRouteListFrom_Changed;

			ylistcomboReceptionTicketFrom.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketFrom.ItemSelected += YlistcomboReceptionTicketFrom_ItemSelected;

			ytreeviewFrom.ColumnsConfig = colConfigFrom;

			//Настройка компонентов куда переносим
			RouteListsFilter filterTo = new RouteListsFilter(UoW);
			filterTo.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListTo.RepresentationModel = new ViewModel.RouteListsVM(filterTo);
			yentryreferenceRouteListTo.Changed += YentryreferenceRouteListTo_Changed;

			ylistcomboReceptionTicketTo.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketTo.ItemSelected += YlistcomboReceptionTicketTo_ItemSelected;

			ytreeviewTo.ColumnsConfig = colConfigTo;

			CheckSensitivities();
		}

		private void CheckSensitivities()
		{
			buttonTransfer.Sensitive =
				ylistcomboReceptionTicketFrom.SelectedItem != null && ylistcomboReceptionTicketTo.SelectedItem != null;

			buttonCreateNewReceptionTicket.Sensitive = yentryreferenceRouteListTo.Subject != null;

			yentryreferenceRouteListFrom .Sensitive = !HasChanges;
			yentryreferenceRouteListTo	 .Sensitive = !HasChanges;

			ylistcomboReceptionTicketFrom.Sensitive = !HasChanges;
			ylistcomboReceptionTicketTo	 .Sensitive = !HasChanges;

			ytreeviewFrom.YTreeModel?.EmitModelChanged();
			ytreeviewTo	 .YTreeModel?.EmitModelChanged();
		}
		
		#endregion

		#region Обработчики событий

		void YentryreferenceRouteListFrom_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRouteListFrom.Subject == null)
				return;

			RouteList routeList = (RouteList)yentryreferenceRouteListFrom.Subject;

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == routeList.Id).List();
			
			ylistcomboReceptionTicketFrom.ItemsList = unloadDocs;
			if (unloadDocs.Count == 1)
				ylistcomboReceptionTicketFrom.Active = 0;

			CheckSensitivities();
		}
		
		void YentryreferenceRouteListTo_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRouteListTo.Subject == null)
				return;

			RouteList routeList = (RouteList)yentryreferenceRouteListTo.Subject;

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == routeList.Id).List();

			ylistcomboReceptionTicketTo.ItemsList = unloadDocs;
			if (unloadDocs.Count == 1)
				ylistcomboReceptionTicketTo.Active = 0;

			CheckSensitivities();
		}

		void YlistcomboReceptionTicketFrom_ItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;

			var result = new List<CarUnloadDocumentNode>();

			foreach (var item in selectedItem.Items) {
				result.Add(new CarUnloadDocumentNode{DocumentItem = item});
			}

			ytreeviewFrom.SetItemsSource(result);

			CheckSensitivities();
		}
		
		void YlistcomboReceptionTicketTo_ItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;

			var result = new List<CarUnloadDocumentNode>();

			foreach (var item in selectedItem.Items) {
				result.Add(new CarUnloadDocumentNode{DocumentItem = item});
			}

			ytreeviewTo.SetItemsSource(result);

			CheckSensitivities();
		}

		protected void OnButtonTransferClicked (object sender, EventArgs e)
		{
			var itemsFrom 	= ytreeviewFrom.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var itemsTo 	= ytreeviewTo.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var fromDoc = ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument;
			var toDoc = ylistcomboReceptionTicketTo.SelectedItem as CarUnloadDocument;

			foreach (var from in itemsFrom.Where(i => i.TransferCount > 0))
			{
				int transfer = from.TransferCount;
				//Заполняем для краткости
				var nomenclature = from.DocumentItem.MovementOperation.Nomenclature;
				var receiveType  = from.DocumentItem.ReciveType;

				var to = itemsTo
					.FirstOrDefault(i => i.DocumentItem.MovementOperation.Nomenclature.Id == nomenclature.Id);

				if(to == null)
				{
					toDoc.AddItem(receiveType, nomenclature, null, transfer, null);

					foreach (var item in toDoc.Items)
					{
						var exist = itemsTo.FirstOrDefault(i => i.DocumentItem.Id == item.Id);
						if (exist == null)
							itemsTo.Add(new CarUnloadDocumentNode{ DocumentItem = item });
					}
				} else {
					to.DocumentItem.MovementOperation.Amount += transfer;

					UoW.Save(to.DocumentItem.MovementOperation);
				}

				from.DocumentItem.MovementOperation.Amount -= transfer;
				if (from.DocumentItem.MovementOperation.Amount == 0)
				{
					var item = fromDoc.Items.First(i => i.Id == from.DocumentItem.Id);
					fromDoc.Items.Remove(item);

					UoW.Delete(from.DocumentItem.MovementOperation);
				} else {
					UoW.Save(from.DocumentItem.MovementOperation);
				}

				from.TransferCount = 0;
			}
			UoW.Save(fromDoc);
			UoW.Save(toDoc);
			CheckSensitivities();
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			Save();
			OnCloseTab(false);
		}

		protected void OnButtonCreateNewReceptionTicketClicked (object sender, EventArgs e)
		{
			var dlg = new CarUnloadDocumentDlg((yentryreferenceRouteListTo.Subject as RouteList).Id, null);
			this.TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		#endregion

		#region Внутренние классы

		public enum OpenParameter { Sender, Receiver }
		
		private class CarUnloadDocumentNode {
			public string Id 			{ get {return DocumentItem.Id.ToString();} }
			public string Nomenclature 	{ get {return DocumentItem.MovementOperation.Nomenclature.OfficialName;} }
			public int 	  ItemsCount 	{ get {return (int)DocumentItem.MovementOperation.Amount;} }

			private int transferCount = 0;
			public int 	  TransferCount { get {return transferCount;}	
				set {
					transferCount = value;
					if (value < 0)
						transferCount = 0;
					if (value > ItemsCount)
						transferCount = ItemsCount;
				} }
			
			public int 	  Residue 		{ get { return ItemsCount - TransferCount;} }

			public CarUnloadDocumentItem DocumentItem { get; set; }
		}

		#endregion
	}
}

