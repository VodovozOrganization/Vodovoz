using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using QSWidgetLib;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class DeliveryShiftDlg : QS.Dialog.Gtk.EntityDialogBase<DeliveryShift>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public DeliveryShiftDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<DeliveryShift>();
			ConfigureDlg();
		}

		public DeliveryShiftDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DeliveryShift>(id);
			ConfigureDlg();
		}

		public DeliveryShiftDlg(DeliveryShift sub) : this (sub.Id) { }

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryFrom.Binding.AddBinding(Entity, e => e.StartTime, w => w.Time).InitializeFromSource();
			entryTo.Binding.AddBinding(Entity, e => e.EndTime, w => w.Time).InitializeFromSource();

			var parallel = new ParallelEditing(entryName);
			parallel.SubscribeOnChanges(entryFrom);
			parallel.SubscribeOnChanges(entryTo);
			parallel.GetParallelTextFunc = NameCreateFunc;
		}

		string NameCreateFunc(object arg)
		{
			return String.Format("{0}-{1}", VeryShortTime(entryFrom.Time), VeryShortTime(entryTo.Time));
		}

		string VeryShortTime(TimeSpan time)
		{
			return (time.Minutes == 0) ? String.Format("{0}", time.Hours) : String.Format("{0}:{1}", time.Hours, time.Minutes);
		}

		public override bool Save()
		{
			var valid = new QSValidator<DeliveryShift>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем смену...");
			UoWGeneric.Save();
			logger.Info("Ок");
			return true;
		}
	}
}
