using System;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

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
		}

		public EquipmentDlg (Equipment sub): this(sub.Id) {}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			referenceNomenclature.SubjectType = typeof(Nomenclature);
			referenceNomenclature.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.equipment))
				.Add (Restrictions.Eq ("Serial", true));
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
	}
}

