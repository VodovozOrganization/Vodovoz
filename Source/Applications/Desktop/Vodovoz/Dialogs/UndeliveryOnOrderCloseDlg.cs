using Autofac;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using System;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveryOnOrderCloseDlg : QS.Dialog.Gtk.SingleUowTabBase
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = new UndeliveredOrdersRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly ISmsNotifierParametersProvider _smsNotifierParametersProvider = new BaseParametersProvider(new ParametersProvider());
		private bool _addedCommentToOldUndelivery;

		UndeliveredOrder undelivery;
		Order order;

		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController =
			new RouteListAddressKeepingDocumentController(new EmployeeRepository(),
				new NomenclatureParametersProvider(new ParametersProvider()));

		private UndeliveredOrderViewModel _undeliveredOrderViewModel;

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

			_undeliveredOrderViewModel = Startup.AppDIContainer.BeginLifetimeScope().Resolve<UndeliveredOrderViewModel>(
				new TypedParameter(typeof(UndeliveredOrder), undelivery),
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(ITdiTab), this as TdiTabBase));
			undeliveryView.WidgetViewModel = _undeliveredOrderViewModel;
			
		}

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> DlgSaved;

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if(HasOrderStatusExternalChangesAndCancellationImpossible(undelivery.OldOrder, out OrderStatus actualOrderStatus))
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					$"Статус заказа был кем-то изменён на статус \"{actualOrderStatus.GetEnumTitle()}\" с момента открытия диалога, теперь отмена невозможна.");

				return;
			}

			UoW.Session.Refresh(undelivery.OldOrder);

			var saved = Save();

			if(!saved && _addedCommentToOldUndelivery)
			{
				DlgSaved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(undelivery));
				return;
			}
			if(!saved)
			{
				return;
			}

			if(undelivery.NewOrder != null
				&& undelivery.OrderTransferType == TransferType.AutoTransferNotApproved
				&& undelivery.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			DlgSaved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(undelivery));
		}

		private bool HasOrderStatusExternalChangesAndCancellationImpossible(Order order, out OrderStatus actualOrderStatus)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Проверка актуального статуа заказа"))
			{
				actualOrderStatus = uow.GetById<Order>(order.Id).OrderStatus;
			}

			var hasOrderStatusChanges = order.OrderStatus != actualOrderStatus;
			var isOrderStatusForbiddenForCancellation = !_orderRepository.GetStatusesForOrderCancelation().Contains(actualOrderStatus);
			var isSelfDeliveryOnLoadingOrder = order.SelfDelivery && actualOrderStatus == OrderStatus.OnLoading;

			return hasOrderStatusChanges
			       && isOrderStatusForbiddenForCancellation
			       && !isSelfDeliveryOnLoadingOrder;
		}

		private bool Save()
		{
			var valid = new QSValidator<UndeliveredOrder>(undelivery);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
			{
				return false;
			}

			_undeliveredOrderViewModel.BeforeSave();

			if(!CanCreateUndelivery())
			{
				OnCloseTab(false);
				return false;
			}
			UoW.Save(undelivery);
			OnCloseTab(false);
			return true;
		}

		private void ProcessSmsNotification()
		{
			var smsNotifier = new SmsNotifier(_smsNotifierParametersProvider);
			smsNotifier.NotifyUndeliveryAutoTransferNotApproved(undelivery, UoW);
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
			{
				return true;
			}

			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, undelivery.GetUndeliveryInfo(_orderRepository));
			_addedCommentToOldUndelivery = true;
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
