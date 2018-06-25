using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repository;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderEquipmentItemsView : WidgetOnDialogBase
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

		public OrderEquipmentItemsView()
		{
			this.Build();
		}

		//public OrderEquipmentItemsView(int id)
		//{
		//	this.Build();
		//	UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
		//	ConfigureDlg();
		//}

		//public OrderEquipmentItemsView(Order sub) : this(sub.Id)
		//{ }

		public void Configure(IUnitOfWork uow, Order order, bool scrolled = false)
		{
			Order = order;
			this.uow = uow;

			buttonDeleteEquipment.Sensitive = false;

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightYellow = new Gdk.Color(0xe1, 0xd6, 0x70);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").SetDataProperty(node => node.FullNameString)
				.AddColumn("Направление").SetDataProperty(node => node.DirectionString)
				.AddColumn("Причина").AddEnumRenderer(
					node => node.DirectionReason
					, true
				).AddSetter((c, n) => {
					if(n.Direction == Domain.Orders.Direction.Deliver) {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "В аренду";
								break;
							case DirectionReason.Repair:
								c.Text = "Из ремонта";
								break;
							case DirectionReason.Cleaning:
								c.Text = "После санобработки";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "Из ремонта и санобработки";
								break;
							default:
								break;
						}
					} else {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "Закрытие аренды";
								break;
							case DirectionReason.Repair:
								c.Text = "В ремонт";
								break;
							case DirectionReason.Cleaning:
								c.Text = "На санобработку";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "В ремонт и санобработку";
								break;
							default:
								break;
						}
					}
				}).HideCondition(HideItemFromDirectionReasonComboInEquipment)
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable = n.Nomenclature?.Category == NomenclatureCategory.equipment && n.Reason != Reason.Rent;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = (n.Nomenclature?.Category == NomenclatureCategory.equipment
									   && n.DirectionReason == DirectionReason.None)
						? colorLightRed
						: colorWhite;
				})



				.AddColumn("Кол-во")
				.AddNumericRenderer(node => node.Count).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
				.AddTextRenderer(node => String.Format("({0})", node.ReturnedCount))
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable = n.Nomenclature?.Category == NomenclatureCategory.equipment;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = colorWhite;
					if(n.Nomenclature?.Category == NomenclatureCategory.equipment
					  && n.OwnType == OwnTypes.None) {
						c.BackgroundGdk = colorLightRed;
					}
				})
				.AddColumn("")
				.Finish();

			treeEquipment.ItemsDataSource = Order.ObservableOrderEquipments;
			//treeDepositRefundItems.ItemsDataSource = Order.ObservableOrderDepositItems;
			treeEquipment.Selection.Changed += TreeEquipment_Selection_Changed;
			//treeDepositRefundItems.Selection.Changed += TreeDepositRefundItems_Selection_Changed;
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item) {
				case DirectionReason.None:
					return true;
				case DirectionReason.Rent:
					return node.Direction == Domain.Orders.Direction.Deliver;
				case DirectionReason.Repair:
				case DirectionReason.Cleaning:
				case DirectionReason.RepairAndCleaning:
				default:
					return false;
			}
		}

		void TreeEquipment_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeEquipment.GetSelectedObjects();

			if(!items.Any())
				return;

			buttonDeleteEquipment.Sensitive = items.Any();
		}

		protected void OnButtonDeleteEquipmentClicked(object sender, EventArgs e)
		{
			Order.DeleteEquipment(treeEquipment.GetSelectedObject() as OrderEquipment);
			//при удалении номенклатуры выделение снимается и при последующем удалении exception
			//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
			buttonDeleteEquipment.Sensitive = treeEquipment.GetSelectedObject() != null;
		}

		protected void OnButtonAddEquipmentToClientClicked(object sender, EventArgs e)
		{
			if(Order.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoW);
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
			nomenclatureFilter.DefaultSelectedCategory = NomenclatureCategory.equipment;
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование к клиенту";
			SelectDialog.ObjectSelected += NomenclatureToClient;
			SelectDialog.ShowFilter = true;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);

			/*

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



			*/

		}

		void NomenclatureToClient(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclatureToClient(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.Root.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
			nomenclatureFilter.DefaultSelectedCategory = NomenclatureCategory.equipment;
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование от клиента";
			SelectDialog.ObjectSelected += NomenclatureFromClient;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);
		}






















	}
}
