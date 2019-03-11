using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repository;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DepositRefundItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public IUnitOfWork UoW { get; set; }

		public Order Order { get; set; }

		public DepositRefundItemsView() => this.Build();

		public void Configure(IUnitOfWork uow, Order order, bool scrolled = false)
		{
			Order = order;
			this.UoW = uow;

			if(MyTab is OrderReturnsView) {
				treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип").SetDataProperty(node => node.DepositTypeString)
				.AddColumn("Название").AddTextRenderer(node => node.EquipmentNomenclature != null ? node.EquipmentNomenclature.Name : string.Empty)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(false)
				.AddColumn("Факт. кол-во").AddNumericRenderer(node => node.ActualCount).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(true)
				.AddColumn("Цена").AddNumericRenderer(node => node.Deposit).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total)
				.Finish();
			} else {
				treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип").SetDataProperty(node => node.DepositTypeString)
				.AddColumn("Название").AddTextRenderer(node => node.EquipmentNomenclature != null ? node.EquipmentNomenclature.Name : string.Empty)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(true)
				.AddColumn("Цена").AddNumericRenderer(node => node.Deposit).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total)
				.Finish();
			}

			treeDepositRefundItems.ItemsDataSource = Order.ObservableOrderDepositItems;
			treeDepositRefundItems.Selection.Changed += TreeDepositRefundItems_Selection_Changed;

			scrolledwindow2.VscrollbarPolicy = scrolled ? PolicyType.Always : PolicyType.Never;
		}

		protected void OnButtonNewBottleDepositClicked(object sender, EventArgs e)
		{
			OrderDepositItem newDepositItem = new OrderDepositItem {
				Count = MyTab is OrderReturnsView ? 0 : 1,
				ActualCount = 0,
				Order = Order,
				DepositType = DepositType.Bottles
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		protected void OnButtonDeleteDepositClicked(object sender, EventArgs e)
		{
			if(treeDepositRefundItems.GetSelectedObject() is OrderDepositItem depositItem)
				if(MyTab is OrderReturnsView)
					//Удаление только новых залогов добавленных из закрытия МЛ
					if(depositItem.Count == 0)
						Order.ObservableOrderDepositItems.Remove(depositItem);
					else
						Order.ObservableOrderDepositItems.Remove(depositItem);
		}

		protected void OnButtonNewEquipmentDepositClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog =
				new OrmReference(
					typeof(Nomenclature),
					UoW,
					NomenclatureRepository.NomenclatureEquipmentsQuery()
										  .GetExecutableQueryOver(UoW.Session)
										  .RootCriteria
					) {
					Mode = OrmReferenceMode.Select,
					TabName = "Оборудование",
					FilterClass = typeof(NomenclatureEquipTypeFilter)
				};
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void SelectDialog_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var selectedNode = (Nomenclature)e.Subject;
			OrderDepositItem newDepositItem = new OrderDepositItem {
				Count = 0,
				ActualCount = 0,
				Order = Order,
				EquipmentNomenclature = selectedNode,
				DepositType = DepositType.Equipment
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		void TreeDepositRefundItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeDepositRefundItems.GetSelectedObjects();
			buttonDeleteDeposit.Sensitive = items.Any();
		}
	}
}
