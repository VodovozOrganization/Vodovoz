using System;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using System.IO;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EquipmentDlg : OrmGtkDialogBase<Equipment>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		//FIXME Возможно нужно удалить конструктор, так как создание нового оборудования отсюда должно быть закрыто.
		public EquipmentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Equipment>();
			ConfigureDlg ();
		}

		public EquipmentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Equipment> (id);
			ConfigureDlg ();
			FillLocation();
		}

		public EquipmentDlg (Equipment sub): this(sub.Id) {}

		private void ConfigureDlg ()
		{
			notebook1.ShowTabs = false;
			radiobuttonInfo.Active = true;
			datatable1.DataSource = subjectAdaptor;
			referenceNomenclature.SubjectType = typeof(Nomenclature);
			referenceNomenclature.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.equipment))
				.Add (Restrictions.Eq ("Serial", true));
			ydatepickerWarrantyEnd.Binding.AddBinding (UoWGeneric.Root, 
				equipment => equipment.WarrantyEndDate, 
				widget => widget.DateOrNull
			);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Equipment> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			
			logger.Info ("Сохраняем оборудование...");
			UoWGeneric.Save();
			return true;
		}

		protected void OnRadiobuttonInfoToggled (object sender, EventArgs e)
		{
			if (radiobuttonInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadiobuttonStickerToggled (object sender, EventArgs e)
		{
			if (radiobuttonSticker.Active) {
				if(UoWGeneric.HasChanges)
				{
					if(CommonDialogs.SaveBeforePrint (typeof(Equipment), "наклейки"))
					{
						UoWGeneric.Save ();
					}
					else if(UoWGeneric.IsNew)
					{
						radiobuttonInfo.Active = true;
						return;
					}
				}
				notebook1.CurrentPage = 1;
				PreparedReport ();
			}
		}

		void PreparedReport()
		{
			string param = "equipment_id=" + UoWGeneric.Root.Id +
				"&dup=0";
			string reportPath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), "Reports", "Equipment" + ".rdl");
			reportviewerSticker.LoadReport (new Uri (reportPath), param, QSMain.ConnectionString);
		}

		void FillLocation()
		{
			var location = Repository.EquipmentRepository.GetLocation(UoW, Entity);
			labelWhere.LabelProp = location.Title;
		}
	}
}

