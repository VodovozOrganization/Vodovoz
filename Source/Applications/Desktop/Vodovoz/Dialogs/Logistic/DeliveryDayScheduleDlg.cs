﻿using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Logistic;
using QS.Project.Services;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class DeliveryDayScheduleDlg : QS.Dialog.Gtk.EntityDialogBase<DeliveryDaySchedule>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public DeliveryDayScheduleDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<DeliveryDaySchedule>();
			ConfigureDlg();

		}

		public DeliveryDayScheduleDlg(int id)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<DeliveryDaySchedule>(id);
			ConfigureDlg();
		}

		public DeliveryDayScheduleDlg(DeliveryDaySchedule sub) : this (sub.Id) { }

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ytreeShifts.CreateFluentColumnsConfig<DeliveryShift>()
				.AddColumn("Название").AddTextRenderer(p => p.Name)
				.AddColumn("Диапазон времени").AddTextRenderer(p => p.DeliveryTime)
				.Finish();
			ytreeShifts.ItemsDataSource = Entity.ObservableShifts;
			ytreeShifts.Selection.Changed += Selection_Changed;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveShift.Sensitive = ytreeShifts.Selection.CountSelectedRows() > 0;
		}

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info("Сохраняем {0}...", DomainHelper.GetSubjectNames(Entity).Nominative);
			UoWGeneric.Save();
			logger.Info("Ок");
			return true;
		}

		protected void OnButtonAddShiftClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(DeliveryShift));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;

			OpenSlaveTab(SelectDialog);
		}

		void SelectDialog_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Entity.AddShift(e.Subject as DeliveryShift);
		}

		protected void OnButtonRemoveShiftClicked(object sender, EventArgs e)
		{
			Entity.RemoveShift(ytreeShifts.GetSelectedObject<DeliveryShift>());
		}
	}
}
