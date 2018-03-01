using System;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repository;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DepositRefundItemsView : WidgetOnDialogBase
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public Order Order { get; set; }

		public DepositRefundItemsView()
		{
			this.Build();
		}

		public void Configure(IUnitOfWork uow, Order order, bool scrolled = false)
		{
			Order = order;
			this.uow = uow;

			treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип").SetDataProperty(node => node.DepositTypeString)
				.AddColumn("Название").AddTextRenderer(node => node.EquipmentNomenclature != null ? node.EquipmentNomenclature.Name : "")
				.AddColumn("Количество").AddNumericRenderer(node => node.Count).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(true)
				.AddColumn("Цена").AddNumericRenderer(node => node.Deposit).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Visible = n.PaymentDirection == PaymentDirection.ToClient)
				.Finish();

			treeDepositRefundItems.ItemsDataSource = Order.ObservableOrderDepositItems;
			treeDepositRefundItems.Selection.Changed += TreeDepositRefundItems_Selection_Changed;

			if(scrolled) {
				scrolledwindow2.VscrollbarPolicy = PolicyType.Always;
			}else {
				scrolledwindow2.VscrollbarPolicy = PolicyType.Never;
			}
		}

		protected void OnButtonNewBottleDepositClicked(object sender, EventArgs e)
		{
			OrderDepositItem newDepositItem = new OrderDepositItem {
				Order = Order,
				PaymentDirection = PaymentDirection.ToClient,
				DepositType = DepositType.Bottles
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		protected void OnButtonDeleteDepositClicked(object sender, EventArgs e)
		{
			Order.ObservableOrderDepositItems.Remove(treeDepositRefundItems.GetSelectedObject() as OrderDepositItem);
		}

		protected void OnButtonNewEquipmentDepositClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog =
				new OrmReference(typeof(Nomenclature),
								 UoW,
								 NomenclatureRepository.NomenclatureEquipmentsQuery()
								 .GetExecutableQueryOver(UoW.Session)
								 .RootCriteria
								);

			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование";
			SelectDialog.FilterClass = typeof(NomenclatureEquipTypeFilter);
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void SelectDialog_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var selectedNode = (Nomenclature)e.Subject;
			OrderDepositItem newDepositItem = new OrderDepositItem {
				Order = Order,
				EquipmentNomenclature = selectedNode,
				PaymentDirection = PaymentDirection.ToClient,
				DepositType = DepositType.Equipment
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		void TreeDepositRefundItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeDepositRefundItems.GetSelectedObjects();
			buttonDeleteDeposit.Sensitive = items.Length > 0;
		}
	}
}
