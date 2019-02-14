using System;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryOrderEditDlg : EntityDialogBase<Order>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();

		public SelfDeliveryOrderEditDlg(Order sub) : this(sub.Id)
		{
		}

		public SelfDeliveryOrderEditDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
			ConfigureDlg();
		}


		void ConfigureDlg()
		{
			referenceClient.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			referenceClient.Sensitive = false;

			var colorWhite = new Color(0xff, 0xff, 0xff);
			var colorLightRed = new Color(0xff, 0x66, 0x66);
			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderItem>()
				.AddColumn("Номенклатура")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.Editing(false)
					.WidthChars(10)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Кол-во по факту")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ActualCount)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) => c.Editable = node.CanEditAmount)
					.WidthChars(10)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : "")
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Editable = node.CanEditPrice())
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS))
					.AddSetter((c, n) => c.Visible = Entity.PaymentType == PaymentType.cashless)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.DiscountForPreview).Editing(true)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)n.Price * n.CurrentCount, 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?").AddToggleRenderer(x => x.IsDiscountInMoney)
					.Editing()
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(OrderRepository.GetDiscountReasons(UoW))
					.AddSetter((c, n) => c.Editable = n.Discount > 0)
					.AddSetter(
						(c, n) => c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null
						? colorLightRed
						: colorWhite
					)
				.AddColumn("Доп. соглашение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.AgreementString)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			ytreeviewItems.ItemsDataSource = Entity.ObservableOrderItems;

			ytreeviewEquipments.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").SetDataProperty(node => node.FullNameString)
				.AddColumn("Направление").SetDataProperty(node => node.DirectionString)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(false)
				.AddColumn("Кол-во по факту")
					.AddNumericRenderer(node => node.ActualCount, false).Editing(true)
					.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
					.Editing(false)
				.AddColumn("Причина").AddEnumRenderer(node => node.DirectionReason, true)
					.AddSetter((c, n) => {
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
					})
				.Editing(false)
				.AddColumn("")
				.Finish();
			ytreeviewEquipments.ItemsDataSource = Entity.ObservableOrderEquipments;

			ytreeviewDepositReturns.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип").SetDataProperty(node => node.DepositTypeString)
				.AddColumn("Название").AddTextRenderer(node => node.EquipmentNomenclature != null ? node.EquipmentNomenclature.Name : "")
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(false)
				.AddColumn("Кол-во по факту").AddNumericRenderer(node => node.ActualCount).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(true)
				.AddColumn("Цена").AddNumericRenderer(node => node.Deposit).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total)
				.RowCells()
				.Finish();
			ytreeviewDepositReturns.ItemsDataSource = Entity.ObservableOrderDepositItems;
		}

		public override bool Save()
		{
			var valid = new QSValidator<Order>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем ...");
			//Entity. проверить оплату и погрузку();
			//UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}
	}
}
