﻿using Autofac;
using Gamma.Utilities;
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
		private OrderStatus _oldOrderStatus;

		UndeliveredOrder undelivery;
		Order order;

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

			_oldOrderStatus = order.OrderStatus;

			_undeliveredOrderViewModel = Startup.AppDIContainer.BeginLifetimeScope().Resolve<UndeliveredOrderViewModel>(
				new TypedParameter(typeof(UndeliveredOrder), undelivery),
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(ITdiTab), this as TdiTabBase));

			undeliveryView.WidgetViewModel = _undeliveredOrderViewModel;

			_undeliveredOrderViewModel.IsSaved += IsSaved;
		}

		private bool IsSaved() => Save(false, true);

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> DlgSaved;

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
		}

		public bool Save(bool needClose = true, bool forceSave = false)
		{
			if(HasOrderStatusExternalChangesOrCancellationImpossible(out OrderStatus actualOrderStatus))
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					$"Статус заказа был кем-то изменён на статус \"{actualOrderStatus.GetEnumTitle()}\" с момента открытия диалога, теперь отмена невозможна.");

				return false;
			}

			UoW.Session.Refresh(undelivery.OldOrder);

			var saved = SaveUndelivery(needClose, forceSave);

			if(!saved && _addedCommentToOldUndelivery)
			{
				DlgSaved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(undelivery, needClose));
				return false;
			}
			if(!saved)
			{
				return false;
			}

			if(undelivery.NewOrder != null
			   && undelivery.OrderTransferType == TransferType.AutoTransferNotApproved
			   && undelivery.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			DlgSaved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(undelivery, needClose));

			return true;
		}

		private bool HasOrderStatusExternalChangesOrCancellationImpossible(out OrderStatus actualOrderStatus)
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Проверка актуального статуа заказа"))
			{
				actualOrderStatus = uow.GetById<Order>(order.Id).OrderStatus;
			}

			var hasOrderStatusChanges = _oldOrderStatus != actualOrderStatus;
			var isOrderStatusForbiddenForCancellation = !_orderRepository.GetStatusesForOrderCancelation().Contains(actualOrderStatus);
			var isSelfDeliveryOnLoadingOrder = order.SelfDelivery && actualOrderStatus == OrderStatus.OnLoading;

			return (isOrderStatusForbiddenForCancellation && !isSelfDeliveryOnLoadingOrder)
				|| hasOrderStatusChanges;
		}

		private bool SaveUndelivery(bool needClose = true, bool forceSave = false)
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(undelivery))
			{
				return false;
			}

			_undeliveredOrderViewModel.BeforeSaveCommand.Execute();

			if(!CanCreateUndelivery() && !forceSave)
			{
				OnCloseTab(false);
				return false;
			}
			UoW.Save(undelivery);
			
			if(needClose)
			{
				OnCloseTab(false);
			}

			return true;
		}

		private void ProcessSmsNotification()
		{
			var smsNotifier = new SmsNotifier(ServicesConfig.UnitOfWorkFactory, _smsNotifierParametersProvider);
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

		public override void Destroy()
		{
			_undeliveredOrderViewModel.IsSaved -= IsSaved;
			_undeliveredOrderViewModel.Dispose();
			base.Destroy();
		}
	}

	public class UndeliveryOnOrderCloseEventArgs : EventArgs
	{
		public UndeliveredOrder UndeliveredOrder { get; private set; }
		public bool NeedClose { get; }

		public UndeliveryOnOrderCloseEventArgs(UndeliveredOrder undeliveredOrder, bool needClose = true)
		{
			NeedClose = needClose;
			UndeliveredOrder = undeliveredOrder;
		}
	}
}
