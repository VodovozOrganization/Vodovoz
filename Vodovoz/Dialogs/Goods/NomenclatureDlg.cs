using System;
using System.Collections.Generic;
using NLog;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSValidation;
using QSWidgetLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class NomenclatureDlg : OrmGtkDialogBase<Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public NomenclatureDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature> ();
			TabName = "Новая номенклатура";
			ConfigureDlg ();
		}

		public NomenclatureDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Nomenclature> (id);
			ConfigureDlg ();
		}

		public NomenclatureDlg (Nomenclature sub) : this (sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			notebook1.ShowTabs = false;
			spinWeight.Sensitive = false;
			spinVolume.Sensitive = false;
			entryName.IsEditable = true;
			radioInfo.Active = true;

			enumVAT.ItemsEnum = typeof(VAT);
			enumVAT.Binding.AddBinding(Entity, e => e.VAT, w => w.SelectedItem).InitializeFromSource();

			enumType.ItemsEnum = typeof(NomenclatureCategory);
			enumType.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryOfficialName.Binding.AddBinding(Entity, e => e.OfficialName, w => w.Text).InitializeFromSource();
			var parallel = new ParallelEditing (yentryOfficialName);
			parallel.SubscribeOnChanges (entryName);
			parallel.GetParallelTextFunc = GenerateOfficialName;

			ycheckRentPriority.Binding.AddBinding(Entity, e => e.RentPriority, w => w.Active).InitializeFromSource();
			checkNotReserve.Binding.AddBinding (Entity, e => e.DoNotReserve, w => w.Active).InitializeFromSource ();
			checkHide.Binding.AddBinding (Entity, e => e.Hide, w => w.Active).InitializeFromSource ();
			entryCode1c.Binding.AddBinding (Entity, e => e.Code1c, w => w.Text).InitializeFromSource();
			yspinSumOfDamage.Binding.AddBinding(Entity, e => e.SumOfDamage, w => w.ValueAsDecimal).InitializeFromSource();
			spinWeight.Binding.AddBinding (Entity, e => e.Weight, w => w.Value).InitializeFromSource ();
			spinVolume.Binding.AddBinding(Entity, e => e.Volume, w => w.Value).InitializeFromSource();

			referenceUnit.SubjectType = typeof (MeasurementUnits);
			referenceUnit.Binding.AddBinding (Entity, n => n.Unit, w => w.Subject).InitializeFromSource ();
			yentryrefEqupmentType.SubjectType = typeof(EquipmentType);
			yentryrefEqupmentType.Binding.AddBinding(Entity, e => e.Type, w => w.Subject).InitializeFromSource();
			referenceColor.SubjectType = typeof(EquipmentColors);
			referenceColor.Binding.AddBinding(Entity, e => e.Color, w => w.Subject).InitializeFromSource();
			referenceWarehouse.SubjectType = typeof (Warehouse);
			referenceWarehouse.Binding.AddBinding (Entity, n => n.Warehouse, w => w.Subject).InitializeFromSource ();
			referenceRouteColumn.SubjectType = typeof (Domain.Logistic.RouteColumn);
			referenceRouteColumn.Binding.AddBinding (Entity, n => n.RouteListColumn, w => w.Subject).InitializeFromSource ();
			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceManufacturer.Binding.AddBinding(Entity, e => e.Manufacturer, w => w.Subject).InitializeFromSource();

			yentryShortName.Binding.AddBinding(Entity, e => e.ShortName, w => w.Text, new NullToEmptyStringConverter()).InitializeFromSource();
			yentryShortName.MaxLength = 20;

			ConfigureInputs (Entity.Category);

			pricesView.UoWGeneric = UoWGeneric;
			if (UoWGeneric.Root.NomenclaturePrice == null)
				UoWGeneric.Root.NomenclaturePrice = new List<NomenclaturePrice> ();
			pricesView.Prices = UoWGeneric.Root.NomenclaturePrice;
		}

		string GenerateOfficialName (object arg)
		{
			var widget = arg as Gtk.Entry;
			return widget.Text;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Nomenclature> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем номенклатуру...");
			pricesView.SaveChanges ();
			UoWGeneric.Save ();
			return true;
		}

		protected void OnEnumTypeChanged (object sender, EventArgs e)
		{
			ConfigureInputs (Entity.Category);
		}

		protected void ConfigureInputs (NomenclatureCategory selected)
		{
			radioEuqpment.Sensitive = selected == NomenclatureCategory.equipment;
			spinWeight.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent || selected == NomenclatureCategory.deposit);
			spinVolume.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent || selected == NomenclatureCategory.deposit);
			labelManufacturer.Sensitive = referenceManufacturer.Sensitive = (selected == NomenclatureCategory.equipment);
			labelColor.Sensitive = referenceColor.Sensitive = (selected == NomenclatureCategory.equipment);
			labelClass.Sensitive = yentryrefEqupmentType.Sensitive = (selected == NomenclatureCategory.equipment);
			labelModel.Sensitive = entryModel.Sensitive = (selected == NomenclatureCategory.equipment);
			labelSerial.Sensitive = checkSerial.Sensitive = (selected == NomenclatureCategory.equipment);
			labelRentPriority.Sensitive = ycheckRentPriority.Sensitive = (selected == NomenclatureCategory.equipment);
			labelReserve.Sensitive = checkNotReserve.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent || selected == NomenclatureCategory.deposit);

			if (Entity.Category == NomenclatureCategory.equipment)
				Entity.Serial = true;
		}

		protected void OnRadioPriceToggled (object sender, EventArgs e)
		{
			if (radioPrice.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnRadioInfoToggled (object sender, EventArgs e)
		{
			if (radioInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioEuqpmentToggled(object sender, EventArgs e)
		{
			if (radioEuqpment.Active)
				notebook1.CurrentPage = 1;
		}
	}
}

