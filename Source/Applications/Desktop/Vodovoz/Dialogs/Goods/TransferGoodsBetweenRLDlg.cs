using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Parameters;

namespace Vodovoz
{
	public partial class TransferGoodsBetweenRLDlg : QS.Dialog.Gtk.TdiTabBase, ITdiDialog, ISingleUoWDialog
	{
		#region Поля

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		private readonly IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository;
		private readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());

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

		public TransferGoodsBetweenRLDlg(IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository)
		{
			this.Build();
			this.employeeNomenclatureMovementRepository = employeeNomenclatureMovementRepository ??
			                                              throw new ArgumentNullException(nameof(employeeNomenclatureMovementRepository));
			this.TabName = "Перенос разгрузок";
			ConfigureDlg();
		}

		public TransferGoodsBetweenRLDlg(RouteList routeList, 
		                                 OpenParameter param, 
		                                 IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository) : this(employeeNomenclatureMovementRepository)
		{
			switch(param) {
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

		public bool HasChanges => UoW.HasChanges;

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			//Настройка элементов откуда переносим
			RouteListsFilter filterFrom = new RouteListsFilter(UoW);
			filterFrom.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListFrom.RepresentationModel = new ViewModel.RouteListsVM(filterFrom);
			yentryreferenceRouteListFrom.Changed += YentryreferenceRouteListFrom_Changed;
			yentryreferenceRouteListFrom.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			ylistcomboReceptionTicketFrom.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketFrom.ItemSelected += YlistcomboReceptionTicketFrom_ItemSelected;

			ytreeviewFrom.ColumnsConfig = colConfigFrom;

			//Настройка компонентов куда переносим
			RouteListsFilter filterTo = new RouteListsFilter(UoW);
			filterTo.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListTo.RepresentationModel = new ViewModel.RouteListsVM(filterTo);
			yentryreferenceRouteListTo.Changed += YentryreferenceRouteListTo_Changed;
			yentryreferenceRouteListTo.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			ylistcomboReceptionTicketTo.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketTo.ItemSelected += YlistcomboReceptionTicketTo_ItemSelected;

			ytreeviewTo.ColumnsConfig = colConfigTo;

			CheckSensitivities();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<CarUnloadDocument>(
				s => {
					foreach(var doc in s.Select(x => x.GetEntity<CarUnloadDocument>())) {
						if(yentryreferenceRouteListFrom.Subject is RouteList rlFrom && doc.RouteList.Id == rlFrom.Id)
							YentryreferenceRouteListFrom_Changed(this, new EventArgs());
						if(yentryreferenceRouteListTo.Subject is RouteList rlTo && doc.RouteList.Id == rlTo.Id)
							YentryreferenceRouteListTo_Changed(this, new EventArgs());
					}
				}
			);
		}

		private void CheckSensitivities()
		{
			buttonTransfer.Sensitive =
				ylistcomboReceptionTicketFrom.SelectedItem != null && ylistcomboReceptionTicketTo.SelectedItem != null;

			buttonCreateNewReceptionTicket.Sensitive = yentryreferenceRouteListTo.Subject != null && ylistcomboReceptionTicketFrom.SelectedItem != null;

			yentryreferenceRouteListFrom.Sensitive = !HasChanges;
			yentryreferenceRouteListTo.Sensitive = !HasChanges;

			ylistcomboReceptionTicketFrom.Sensitive = !HasChanges;
			ylistcomboReceptionTicketTo.Sensitive = !HasChanges;

			ytreeviewFrom.YTreeModel?.EmitModelChanged();
			ytreeviewTo.YTreeModel?.EmitModelChanged();
		}

		#endregion

		#region Обработчики событий

		void YentryreferenceRouteListFrom_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRouteListFrom.Subject == null)
				return;

			RouteList routeList = (RouteList)yentryreferenceRouteListFrom.Subject;

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == routeList.Id).List();

			ylistcomboReceptionTicketFrom.ItemsList = unloadDocs;
			if(unloadDocs.Count == 1)
				ylistcomboReceptionTicketFrom.Active = 0;

			CheckSensitivities();
		}

		void YentryreferenceRouteListTo_Changed(object sender, EventArgs e)
		{
			if(yentryreferenceRouteListTo.Subject == null)
				return;

			RouteList routeList = (RouteList)yentryreferenceRouteListTo.Subject;

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == routeList.Id).List();

			ylistcomboReceptionTicketTo.ItemsList = unloadDocs;
			if(unloadDocs.Count == 1)
				ylistcomboReceptionTicketTo.Active = 0;

			CheckSensitivities();
		}

		void YlistcomboReceptionTicketFrom_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;

			ytreeviewFrom.SetItemsSource(GetNodesWithoutDriverBalanceNomenclatures(selectedItem));

			CheckSensitivities();
		}

		void YlistcomboReceptionTicketTo_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;
			
			ytreeviewTo.SetItemsSource(GetNodesWithoutDriverBalanceNomenclatures(selectedItem));

			CheckSensitivities();
		}

		private IList<CarUnloadDocumentNode> GetNodesWithoutDriverBalanceNomenclatures(CarUnloadDocument selectedItem) {
			var result = new List<CarUnloadDocumentNode>();

			var driverBalanceNomenclatures =
				employeeNomenclatureMovementRepository.GetNomenclaturesFromDriverBalance(UoW, selectedItem.RouteList.Driver.Id);

			foreach(var item in selectedItem.Items) {
				var nomenclature = driverBalanceNomenclatures.SingleOrDefault(x =>
					x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
				
				if (nomenclature == null) {
					result.Add(new CarUnloadDocumentNode {DocumentItem = item});
				}
			}

			return result;
		} 

		protected void OnButtonTransferClicked(object sender, EventArgs e)
		{
			var itemsFrom = ytreeviewFrom.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var itemsTo = ytreeviewTo.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var fromDoc = ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument;
			var toDoc = ylistcomboReceptionTicketTo.SelectedItem as CarUnloadDocument;

			foreach(var from in itemsFrom.Where(i => i.TransferCount > 0))
			{
				int transfer = from.TransferCount;
				//Заполняем для краткости
				var nomenclature = from.DocumentItem.GoodsAccountingOperation.Nomenclature;
				var receiveType = from.DocumentItem.ReciveType;

				var to = itemsTo
					.FirstOrDefault(i => i.DocumentItem.GoodsAccountingOperation.Nomenclature.Id == nomenclature.Id);

				if(to == null)
				{
					var tetminalId = _baseParametersProvider.GetNomenclatureIdForTerminal;
					toDoc.AddItem(receiveType, nomenclature, null, transfer, null, tetminalId);

					foreach(var item in toDoc.Items)
					{
						var exist = itemsTo.FirstOrDefault(i => i.DocumentItem.Id == item.Id);
						if(exist == null)
							itemsTo.Add(new CarUnloadDocumentNode { DocumentItem = item });
					}
				}
				else
				{
					to.DocumentItem.GoodsAccountingOperation.Amount += transfer;
					to.DocumentItem.DeliveryFreeBalanceOperation.Amount -= transfer;

					UoW.Save(to.DocumentItem.GoodsAccountingOperation);
					UoW.Save(to.DocumentItem.DeliveryFreeBalanceOperation);
				}

				from.DocumentItem.GoodsAccountingOperation.Amount -= transfer;
				from.DocumentItem.DeliveryFreeBalanceOperation.Amount += transfer;

				if(from.DocumentItem.GoodsAccountingOperation.Amount == 0)
				{
					var item = fromDoc.Items.First(i => i.Id == from.DocumentItem.Id);
					fromDoc.Items.Remove(item);

					UoW.Delete(from.DocumentItem.GoodsAccountingOperation);
					UoW.Delete(from.DocumentItem.DeliveryFreeBalanceOperation);
				}
				else
				{
					UoW.Save(from.DocumentItem.GoodsAccountingOperation);
					UoW.Save(from.DocumentItem.DeliveryFreeBalanceOperation);
				}

				from.TransferCount = 0;
			}
			UoW.Save(fromDoc);
			UoW.Save(toDoc);
			CheckSensitivities();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
			OnCloseTab(false);
		}

		protected void OnButtonCreateNewReceptionTicketClicked(object sender, EventArgs e)
		{
			TabParent.AddSlaveTab(
				this,
				new CarUnloadDocumentDlg(
					(yentryreferenceRouteListTo.Subject as RouteList).Id,
					(ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument).Id,
					(ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument).TimeStamp
				)
			);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		#endregion

		#region Внутренние классы

		public enum OpenParameter { Sender, Receiver }

		private class CarUnloadDocumentNode
		{
			public string Id { get { return DocumentItem.Id.ToString(); } }
			public string Nomenclature { get { return DocumentItem.GoodsAccountingOperation.Nomenclature.OfficialName; } }
			public int ItemsCount { get { return (int)DocumentItem.GoodsAccountingOperation.Amount; } }

			private int transferCount = 0;
			public int TransferCount {
				get { return transferCount; }
				set {
					transferCount = value;
					if(value < 0)
						transferCount = 0;
					if(value > ItemsCount)
						transferCount = ItemsCount;
				}
			}

			public int Residue { get { return ItemsCount - TransferCount; } }

			public CarUnloadDocumentItem DocumentItem { get; set; }
		}

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			UoW?.Dispose();
			base.Dispose();
		}

		#endregion
	}
}

