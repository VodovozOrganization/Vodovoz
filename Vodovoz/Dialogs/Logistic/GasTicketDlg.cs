using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using System.Linq;
using QSValidation;

namespace Vodovoz
{
	public partial class GazTicketDlg : OrmGtkDialogBase<GazTicket>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public GazTicketDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<GazTicket>();
			ConfigureDlg ();
		}

		public GazTicketDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<GazTicket> (id);
			ConfigureDlg ();
		}

		public GazTicketDlg (GazTicket sub) : this (sub.Id) {}

		private void ConfigureDlg ()
		{
			comboFuelType.SetRenderTextFunc<FuelType>(x=>x.Name);
			comboFuelType.ItemsList = UoW.GetAll<FuelType>().ToList();

			yentryName.Binding.AddBinding 		(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			comboFuelType.Binding.AddBinding    (Entity, e => e.FuelType, w => w.SelectedItem).InitializeFromSource();
			yspinbuttonLitres.Binding.AddBinding(Entity, e => e.Liters, w => w.ValueAsInt).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<GazTicket> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем график доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}

