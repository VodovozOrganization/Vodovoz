using System;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSValidation;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveredOrderDlg : OrmGtkDialogBase<UndeliveredOrder>, ITdiTabAddedNotifier
	{
		UndeliveryStatus initialStatus;

		public UndeliveredOrderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<UndeliveredOrder>();
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.EmployeeRegistrator = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать недовозы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			initialStatus = UoWGeneric.Root.UndeliveryStatus = UndeliveryStatus.InProcess;
			TabName = "Новый недовоз";
			Entity.TimeOfCreation = DateTime.Now;
			ConfigureDlg();
		}

		public UndeliveredOrderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<UndeliveredOrder>(id);
			initialStatus = Entity.UndeliveryStatus;
			ConfigureDlg();
		}

		public UndeliveredOrderDlg(UndeliveredOrder sub) : this(sub.Id)
		{ }

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			undeliveryView.OnTabAdded();
		}

		public void ConfigureDlg()
		{
			undeliveryView.ConfigureDlg(UoW, Entity, initialStatus);
		}

		public override bool Save()
		{
			var valid = new QSValidator<UndeliveredOrder>(Entity);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;
			Entity.OldOrder.SetUndeliveredStatus(Entity.GuiltySide);
			undeliveryView.SaveChanges();
			UoWGeneric.Save();
			return true;
		}
	}
}
