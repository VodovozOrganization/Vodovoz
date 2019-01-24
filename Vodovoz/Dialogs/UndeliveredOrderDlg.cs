using System;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Tdi;
using QSValidation;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveredOrderDlg : QS.Dialog.Gtk.SingleUowTabBase, ITdiTabAddedNotifier
	{
		public event EventHandler<UndeliveryOnOrderCloseEventArgs> DlgSaved;
		public event EventHandler<EventArgs> CommentAdded;
		UndeliveredOrder UndeliveredOrder { get; set; }

		public UndeliveredOrderDlg()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithNewRoot<UndeliveredOrder>();
			UndeliveredOrder = UoW.RootObject as UndeliveredOrder;
			UndeliveredOrder.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			UndeliveredOrder.EmployeeRegistrator = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(UndeliveredOrder.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать недовозы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			TabName = "Новый недовоз";
			UndeliveredOrder.TimeOfCreation = DateTime.Now;
			ConfigureDlg();
		}

		public UndeliveredOrderDlg(int id)
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateForRoot<UndeliveredOrder>(id);
			UndeliveredOrder = UoW.RootObject as UndeliveredOrder;
			TabName = UndeliveredOrder.Title;
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
			undeliveryView.ConfigureDlg(UoW, UndeliveredOrder);
			SetAccessibilities();
			if(UndeliveredOrder.Id > 0) {//если недовоз новый, то не можем оставлять комментарии
				IUnitOfWork UoWForComments = UnitOfWorkFactory.CreateWithoutRoot();
				unOrderCmntView.Configure(UoWForComments, UndeliveredOrder, CommentedFields.Reason);
				unOrderCmntView.CommentAdded += (sender, e) => CommentAdded(sender, e);
				this.Destroyed += (sender, e) => {
					if(UoWForComments != null)
						UoWForComments.Dispose();
				};
			}
		}

		void SetAccessibilities(){
			unOrderCmntView.Visible = UndeliveredOrder.Id > 0;
		}

		public virtual bool Save()
		{
			var valid = new QSValidator<UndeliveredOrder>(UndeliveredOrder);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;
			if(UndeliveredOrder.Id == 0) {
				UndeliveredOrder.OldOrder.SetUndeliveredStatus();
			}
			undeliveryView.BeforeSaving();
			//случай, если создавать новый недовоз не нужно, но нужно обновить старый заказ
			if(!CanCreateUndelivery()){
				UoW.Save(UndeliveredOrder.OldOrder);
				UoW.Commit();
				this.OnCloseTab(false);
				return false;
			}

			UoW.Save(UndeliveredOrder);
			this.OnCloseTab(false);
			return true;
		}

		/// <summary>
		/// Проверка на возможность создания нового недовоза
		/// </summary>
		/// <returns><c>true</c>, если можем создать, <c>false</c> если создать недовоз не можем,
		/// при этом добавляется автокомментарий к существующему недовозу с содержимым
		/// нового (но не добавленного) недовоза.</returns>
		bool CanCreateUndelivery()
		{
			if(UndeliveredOrder.Id > 0)
				return true;
			var otherUndelivery = UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, UndeliveredOrder.OldOrder).FirstOrDefault();
			if(otherUndelivery == null)
				return true;
			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, UndeliveredOrder.GetUndeliveryInfo());
			return false;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
			if(DlgSaved != null)
				DlgSaved(this, new UndeliveryOnOrderCloseEventArgs(UndeliveredOrder));
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			this.OnCloseTab(true);
		}
	}
}
