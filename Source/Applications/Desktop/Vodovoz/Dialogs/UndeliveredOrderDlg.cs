using Autofac;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using System;
using System.Linq;
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
using Vodovoz.ViewModels.Widgets;

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
		private UndeliveredOrderViewModel _undeliveredOrderViewModel;

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
			_undeliveredOrderViewModel.OldOrderSelectCommand.Execute();
		}

		public void ConfigureDlg(bool isForSalesDepartment = false)
		{
			if(isForSalesDepartment)
			{
				var salesDepartmentId = _subdivisionParametersProvider.GetSalesSubdivisionId();
				UndeliveredOrder.InProcessAtDepartment = UoW.GetById<Subdivision>(salesDepartmentId);
			}

			_undeliveredOrderViewModel = Startup.AppDIContainer.BeginLifetimeScope().Resolve<UndeliveredOrderViewModel>(
				new TypedParameter(typeof(UndeliveredOrder), UndeliveredOrder),
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(ITdiTab), this as TdiTabBase));

			undeliveryView.WidgetViewModel = _undeliveredOrderViewModel;

			_undeliveredOrderViewModel.IsSaved += IsSaved;

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

		private bool IsSaved() => Save(false);

		void SetAccessibilities(){
			unOrderCmntView.Visible = UndeliveredOrder.Id > 0;
		}

		private bool Save(bool needClose = true)
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(UndeliveredOrder))
			{
				return false;
			}

			if(UndeliveredOrder.Id == 0)
			{
				UndeliveredOrder.OldOrder.SetUndeliveredStatus(UoW, _baseParametersProvider, CallTaskWorker);
			}

			_undeliveredOrderViewModel.BeforeSaveCommand.Execute();

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
			_undeliveredOrderViewModel.IsSaved -= IsSaved;
			_undeliveredOrderViewModel.Dispose();
			UoW?.Dispose();
			base.Destroy();
		}
	}
}
