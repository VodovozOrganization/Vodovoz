using System;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveryOnOrderCloseDlg : QS.Dialog.Gtk.SingleUowTabBase
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = new UndeliveredOrdersRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();

		UndeliveredOrder undelivery;
		Order order;

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
			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			
			undelivery = new UndeliveredOrder{
				UoW = UoW,
				Author = currentEmployee,
				EmployeeRegistrator = currentEmployee,
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
			var otherUndelivery = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, order).FirstOrDefault();
			if(otherUndelivery == null)
				return true;
			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, undelivery.GetUndeliveryInfo(_orderRepository));
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
