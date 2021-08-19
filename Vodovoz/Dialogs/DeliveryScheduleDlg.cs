using System;
using System.Linq;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using QSWidgetLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace Vodovoz
{
	public partial class DeliveryScheduleDlg : QS.Dialog.Gtk.EntityDialogBase<DeliverySchedule>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository = new DeliveryScheduleRepository();

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
            ycheckIsArchive.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

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
			var all =_deliveryScheduleRepository.All(UoWGeneric);
			var notArchivedList = all.Where(ds => ds.IsArchive == false && ds.From == Entity.From && ds.To == Entity.To).ToList();
			if (notArchivedList.Any() && UoWGeneric.Root.IsArchive == false)
			{//при архивировании интервала эти проверки не нужны
				//есть вероятность, что среди активных интервалов есть дубликаты, так что берем первый
				var active = notArchivedList.First();
				MessageDialogHelper.RunWarningDialog("Уже существует интервал с таким же периодом.\n" +
				                                     "Создание нового интервала невозможно.\n" +
				                                     "Существующий интервал:\n" +
				                                     $"Код: {active.Id}\n" +
				                                     $"Название: {active.Name}\n" +
				                                     $"Период: {active.DeliveryTime}\n");
				return false; // нашли активный
			}
			
			var archivedList = all.Where(ds => ds.IsArchive && ds.From == Entity.From && ds.To == Entity.To).ToList();
			if (archivedList.Any() && UoWGeneric.Root.IsArchive == false)
			{//при архивировании интервала эти проверки не нужны
				//т.к. интервалы нельзя удалять, архивными могут быть несколько, так что берем первый
				var archived = archivedList.First();
				if(MessageDialogHelper.RunQuestionDialog("Уже существует архивный интервал с таким же периодом.\n" +
				                                         "Создание нового интервала невозможно.\n" +
				                                         "Разархивировать существующий интервал?"))
				{//отменяем изменения текущей сущности интервала и обновляем найденный архивный
					UoWGeneric.Delete(UoWGeneric.Root);
					archived.IsArchive = false;
					UoWGeneric.Save(archived);
					UoWGeneric.Commit();
					MessageDialogHelper.RunInfoDialog("Разархивирован интервал:\n" +
					                                  $"Код: {archived.Id}\n" +
					                                  $"Название: {archived.Name}\n" +
					                                  $"Период: {archived.DeliveryTime}\n");
				}
				return false; // нашли/разархивировали старый
			}
			
			var valid = new QSValidator<DeliverySchedule> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем график доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}