using System;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveredOrderDlg : QS.Dialog.Gtk.SingleUowTabBase, ITdiTabAddedNotifier
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = new UndeliveredOrdersRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider = new SubdivisionParametersProvider(new ParametersProvider());

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> DlgSaved;
		public event EventHandler<EventArgs> CommentAdded;
		UndeliveredOrder UndeliveredOrder { get; set; }

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						_orderRepository,
						_employeeRepository,
						_baseParametersProvider,
						ServicesConfig.CommonServices.UserService,
						ErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public UndeliveredOrderDlg(bool isForSalesDepartment = false)
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithNewRoot<UndeliveredOrder>();
			UndeliveredOrder = UoW.RootObject as UndeliveredOrder;
			UndeliveredOrder.Author = UndeliveredOrder.EmployeeRegistrator = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(UndeliveredOrder.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать недовозы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			
			TabName = "Новый недовоз";
			UndeliveredOrder.TimeOfCreation = DateTime.Now;
			ConfigureDlg(isForSalesDepartment);
		}

		public UndeliveredOrderDlg(int id, bool isForSalesDepartment = false)
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateForRoot<UndeliveredOrder>(id);
			UndeliveredOrder = UoW.RootObject as UndeliveredOrder;
			TabName = UndeliveredOrder.Title;
			ConfigureDlg(isForSalesDepartment);
		}

		public UndeliveredOrderDlg(UndeliveredOrder sub) : this(sub.Id)
		{ }

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			//undeliveryView.OnTabAdded();
		}

		public void ConfigureDlg(bool isForSalesDepartment = false)
		{
			if(isForSalesDepartment)
			{
				var salesDepartmentId = _subdivisionParametersProvider.GetSalesSubdivisionId();
				UndeliveredOrder.InProcessAtDepartment = UoW.GetById<Subdivision>(salesDepartmentId);
			}

			//undeliveryView.ConfigureDlg(UoW, UndeliveredOrder);
			//undeliveryView.isSaved += () => Save(false);
			SetAccessibilities();
			if(UndeliveredOrder.Id > 0) {//если недовоз новый, то не можем оставлять комментарии
				IUnitOfWork UoWForComments = UnitOfWorkFactory.CreateWithoutRoot();
				unOrderCmntView.Configure(UoWForComments, UndeliveredOrder, CommentedFields.Reason);
				unOrderCmntView.CommentAdded += (sender, e) => CommentAdded?.Invoke(sender, e);
				this.Destroyed += (sender, e) =>
				{
					UoWForComments?.Dispose();
				};
			}
		}

		void SetAccessibilities(){
			unOrderCmntView.Visible = UndeliveredOrder.Id > 0;
		}

		private bool Save(bool needClose = true)
		{
			var valid = new QSValidator<UndeliveredOrder>(UndeliveredOrder);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
			{
				return false;
			}

			if(UndeliveredOrder.Id == 0)
			{
				UndeliveredOrder.OldOrder.SetUndeliveredStatus(UoW, _baseParametersProvider, CallTaskWorker);
			}
			//undeliveryView.BeforeSaving();
			//случай, если создавать новый недовоз не нужно, но нужно обновить старый заказ
			if(!CanCreateUndelivery())
			{
				UoW.Save(UndeliveredOrder.OldOrder);
				UoW.Commit();
				this.OnCloseTab(false);
				return false;
			}

			UoW.Save(UndeliveredOrder);
			if(UndeliveredOrder.NewOrder != null
			   && UndeliveredOrder.OrderTransferType == TransferType.AutoTransferNotApproved
			   && UndeliveredOrder.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			if(needClose)
			{
				this.OnCloseTab(false);
			}
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
			var otherUndelivery =
				_undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, UndeliveredOrder.OldOrder).FirstOrDefault();
			if(otherUndelivery == null)
				return true;
			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, UndeliveredOrder.GetUndeliveryInfo(_orderRepository));
			return false;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if(Save())
			{
				DlgSaved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(UndeliveredOrder));
			}
		}

		private void ProcessSmsNotification()
		{
			SmsNotifier smsNotifier = new SmsNotifier(_baseParametersProvider);
			smsNotifier.NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			this.OnCloseTab(true);
		}
		
		public void UnsubscribeAll()
		{
			CommentAdded = null;
			DlgSaved = null;
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}
