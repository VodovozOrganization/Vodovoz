using System;
using Gtk;
using QSOrmProject;
using QSTDI;
using QSValidation;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveryOnOrderCloseDlg : TdiTabBase
	{
		IUnitOfWork uow;
		UndeliveredOrder undelivery;
		Order order;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public UndeliveryOnOrderCloseDlg()
		{
			this.Build();
			TabName = "Новый недовоз";
		}

		public UndeliveryOnOrderCloseDlg(Order order, IUnitOfWork uow) : this()
		{
			UoW = uow;
			this.order = order;
			ConfigureDlg();
		}

		public void ConfigureDlg()
		{
			undelivery = new UndeliveredOrder{
				UoW = UoW,
				Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW),
				EmployeeRegistrator = EmployeeRepository.GetEmployeeForCurrentUser(UoW),
				TimeOfCreation = DateTime.Now,
				UndeliveryStatus = UndeliveryStatus.InProcess,
				OldOrder = order
			};
			undeliveryView.ConfigureDlg(UoW, undelivery);
		}

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> DlgSaved;

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			var valid = new QSValidator<UndeliveredOrder>(undelivery);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;
			undeliveryView.SaveChanges();
			UoW.Save(undelivery);
			OnCloseTab(false);
			if(DlgSaved != null)
				DlgSaved(this, new UndeliveryOnOrderCloseEventArgs(undelivery));
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(true);
		}
	}

	public class UndeliveryOnOrderCloseEventArgs : EventArgs
	{
		public UndeliveredOrder UndeliveredOrder { get; private set; }

		public UndeliveryOnOrderCloseEventArgs(UndeliveredOrder undeliveredOrder)
		{
			UndeliveredOrder = undeliveredOrder;
		}
	}
}
