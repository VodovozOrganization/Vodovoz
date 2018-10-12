using System;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using QSTDI;
using QSValidation;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories;
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
			undeliveryView.BeforeSaving();
			if(!CanCreateUndelivery()) {
				this.OnCloseTab(false);
				if(DlgSaved != null)
					DlgSaved(this, new UndeliveryOnOrderCloseEventArgs(undelivery));
				return;
			}
			UoW.Save(undelivery);
			OnCloseTab(false);
			if(DlgSaved != null)
				DlgSaved(this, new UndeliveryOnOrderCloseEventArgs(undelivery));
		}

		/// <summary>
		/// Проверка на возможность создания нового недовоза
		/// </summary>
		/// <returns><c>true</c>, если можем создать, <c>false</c> если создать недовоз не можем,
		/// при этом добавляется автокомментарий к существующему недовозу с содержимым
		/// нового (но не добавленного) недовоза.</returns>
		bool CanCreateUndelivery()
		{
			var otherUndelivery = UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, order).FirstOrDefault();
			if(otherUndelivery == null)
				return true;
			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, undelivery.GetUndeliveryInfo());
			return false;
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
