using System;
using System.Collections.Generic;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class NomenclatureDlg : OrmGtkDialogBase<Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public NomenclatureDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature>();
			TabName = "Новая номенклатура";
			ConfigureDlg ();
		}

		public NomenclatureDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Nomenclature> (id);
			ConfigureDlg ();
		}

		public NomenclatureDlg (Nomenclature sub) : this (sub.Id){}

		private void ConfigureDlg ()
		{
			notebook1.ShowTabs = false;
			spinWeight.Sensitive = false;
			entryName.IsEditable = true;
			datatable1.DataSource = subjectAdaptor;
			datatable2.DataSource = subjectAdaptor;
			enumType.DataSource = subjectAdaptor;
			enumVAT.DataSource = subjectAdaptor;
			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceColor.SubjectType = typeof(EquipmentColors);
			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceType.SubjectType = typeof(EquipmentType);
			entryreferenceRouteColumn.PropertyMapping<Nomenclature> (n => n.RouteListColumn);
			ConfigureInputs ((NomenclatureCategory)enumType.Active);
			pricesView.UoWGeneric = UoWGeneric;
			if (UoWGeneric.Root.NomenclaturePrice == null)
				UoWGeneric.Root.NomenclaturePrice = new List<NomenclaturePrice> ();
			pricesView.Prices = UoWGeneric.Root.NomenclaturePrice;
			radioInfo.Active = true;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Nomenclature> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем номенклатуру...");
			pricesView.SaveChanges ();
			UoWGeneric.Save();
			return true;
		}

		protected void OnEnumTypeChanged (object sender, EventArgs e)
		{
			ConfigureInputs ((NomenclatureCategory)enumType.Active);
		}

		protected void ConfigureInputs (NomenclatureCategory selected)
		{
			spinWeight.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent || selected == NomenclatureCategory.deposit);
			labelManufacturer.Sensitive = referenceManufacturer.Sensitive = (selected == NomenclatureCategory.equipment);
			labelColor.Sensitive = referenceColor.Sensitive = (selected == NomenclatureCategory.equipment);
			labelClass.Sensitive = referenceType.Sensitive = (selected == NomenclatureCategory.equipment);
			labelModel.Sensitive = entryModel.Sensitive = (selected == NomenclatureCategory.equipment);
			labelSerial.Sensitive = checkSerial.Sensitive = (selected == NomenclatureCategory.equipment);
			labelReserve.Sensitive = checkNotReserve.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent || selected == NomenclatureCategory.deposit);
		}

		protected void OnRadioPriceToggled (object sender, EventArgs e)
		{
			if (radioPrice.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioInfoToggled (object sender, EventArgs e)
		{
			if (radioInfo.Active)
				notebook1.CurrentPage = 0;
		}
	}
}

