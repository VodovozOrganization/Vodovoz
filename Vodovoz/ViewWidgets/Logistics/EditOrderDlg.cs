using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gtk;
using NHibernate.Proxy;
using NLog;
using QSDocTemplates;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSTDI;
using QSValidation;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Client;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EditOrderDlg : OrmGtkDialogBase<Order>
	{
		public EditOrderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
			ConfigureDlg();
		}

		public EditOrderDlg(Order sub) : this(sub.Id)
		{ }

		public void ConfigureDlg()
		{
			/*Entity.ObservableOrderItems.ElementAdded += Entity_ObservableOrderItems_ElementAdded;
			Entity.ObservableOrderEquipments.ElementAdded += Entity_ObservableOrderEquipments_ElementAdded;

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;*/

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightYellow = new Gdk.Color(0xe1, 0xd6, 0x70);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderItem>()
				.AddColumn("Номенклатура")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
					.AddTextRenderer(node => (node.CanShowReturnedCount) ? String.Format("({0})", node.ReturnedCount) : "")
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : "")
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
				.AddSetter((c, node) => c.Editable = node.CanEditPrice())
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) => {
						c.ForegroundGdk = colorBlack;
						if(node.AdditionalAgreement == null) {
							return;
						}
						AdditionalAgreement aa = node.AdditionalAgreement.Self;
						if(aa is WaterSalesAgreement &&
						  (aa as WaterSalesAgreement).IsFixedPrice) {
							c.ForegroundGdk = colorGreen;
						} else if(node.IsUserPrice &&
						  Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
							c.ForegroundGdk = colorBlue;
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS))
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Скидка %")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Discount)
					.Adjustment(new Adjustment(0, 0, 100, 1, 100, 1)).Editing(true)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(UoW.GetAll<DiscountReason>()
							   .ToList()).AddSetter((c, n) => c.Editable = n.Discount > 0)
				.AddColumn("Доп. соглашение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.AgreementString)
				.RowCells()
					.XAlign(0.5f)
				.Finish();

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").SetDataProperty(node => node.FullNameString)
				.AddColumn("Направление").SetDataProperty(node => node.DirectionString)
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
				.AddColumn("")
				.Finish();


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

		public override bool Save()
		{
			UoWGeneric.Save();
			return true;
		}

	}

}