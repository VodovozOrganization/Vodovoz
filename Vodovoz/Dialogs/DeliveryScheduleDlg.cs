using System;
using NLog;
using QSOrmProject;
using QSValidation;
using QSWidgetLib;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DeliveryScheduleDlg : OrmGtkDialogBase<DeliverySchedule>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public DeliveryScheduleDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<DeliverySchedule>();
			TabName = "Новый график доставки";
			ConfigureDlg ();
		}

		public DeliveryScheduleDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DeliverySchedule> (id);
			ConfigureDlg ();
		}

		public DeliveryScheduleDlg (DeliverySchedule sub) : this (sub.Id) {}

		private void ConfigureDlg ()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryFrom.Binding.AddBinding(Entity, e => e.From, w => w.Time).InitializeFromSource();
			entryTo.Binding.AddBinding(Entity, e => e.To, w => w.Time).InitializeFromSource();

			var parallel = new ParallelEditing (entryName);
			parallel.SubscribeOnChanges (entryFrom);
			parallel.SubscribeOnChanges (entryTo);
			parallel.GetParallelTextFunc = NameCreateFunc;
		}

		string NameCreateFunc (object arg)
		{
			return String.Format ("{0}-{1}", VeryShortTime (entryFrom.Time), VeryShortTime (entryTo.Time));
		}

		string VeryShortTime (TimeSpan time)
		{
			return (time.Minutes == 0) ? String.Format ("{0}", time.Hours) : String.Format ("{0}:{1}", time.Hours, time.Minutes);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<DeliverySchedule> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем график доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}

