using Comparer;
using CustomerNotifications.Contracts;
using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using Edo.Documents.Services;
using EdoService.Library;
using Gamma.Binding.Core;
using Gamma.Utilities;
using Notifications.Infrastructure;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Application.Orders;
using Vodovoz.Core.Application.Orders.Services.OrderCancellation;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors.Logistics;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Orders;
using VodovozBusiness.NotificationSenders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoDiscrepancyViewModel : DialogViewModelBase, IDisposable
	{
        private readonly IUnitOfWork _uow;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoService _edoService;
		private readonly IInteractiveService _interactiveService;
		private readonly IUpdDocumentBuilder _updDocumentBuilder;
		private readonly INotificationPublisher<CustomerNotificationDomainEvent> _notificationPublisher;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IEmployeeService _employeeService;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListChangesNotificationSender _routeListChangesNotificationSender;
		private readonly IDiffFormatter _diffFormatter;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly EdoDiscrepancyNode _edoDiscrepancy;
		private DocumentEdoTask _documentEdoTask;
		private bool _onlyDiffs;
		private IReadOnlyList<PropertyDifference> _differences;
		private List<UpdCompareViewModel> _updCompareFields = new List<UpdCompareViewModel>();
		private bool _actionInProgress;
		private bool _actionFinished;
        private IUnitOfWork _cancelOrderUow;

		public EdoDiscrepancyViewModel(
			int edoDocumentId,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			IEdoService edoService,
			IInteractiveService interactiveService,
			IUpdDocumentBuilder updDocumentBuilder,
			INotificationPublisher<CustomerNotificationDomainEvent> notificationPublisher,
			IOrderContractUpdater orderContractUpdater,
			IEmployeeService employeeService,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICallTaskWorker callTaskWorker,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			IRouteListChangesNotificationSender routeListChangesNotificationSender,
			IDiffFormatter diffFormatter,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigationManager
		) : base(navigationManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_updDocumentBuilder = updDocumentBuilder ?? throw new ArgumentNullException(nameof(updDocumentBuilder));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListChangesNotificationSender = routeListChangesNotificationSender ?? throw new ArgumentNullException(nameof(routeListChangesNotificationSender));
			_diffFormatter = diffFormatter ?? throw new ArgumentNullException(nameof(diffFormatter));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_uow = _unitOfWorkFactory.CreateWithoutRoot();

			if(!_currentPermissionService.ValidatePresetPermission(EdoPermissions.CanWorkWithEdoDiscrepancyJournal))
			{
				throw new AbortCreatingPageException(
					"Для работы с журналом расхождений ЭДО недостаточно прав.",
					"Проверка прав"
				);
			}

			_edoDiscrepancy = _edoRepository.GetEdoDiscrepancy(_uow, edoDocumentId);
			if(_edoDiscrepancy == null)
			{
				throw new AbortCreatingPageException(
					$"Для ЭДО документа №{edoDocumentId} не найдено расхождений",
					"Не найдено расхождений",
					ImportanceLevel.Info
				);
			}

			_documentEdoTask = _uow.GetById<DocumentEdoTask>(_edoDiscrepancy.DocumentEdoTaskId);

			OrderId = _edoDiscrepancy.OrderId.ToString();
			DeliveryDate = _edoDiscrepancy.DeliveryDate.ToString("dd.MM.yyyy");
			OrderStatus = _edoDiscrepancy.OrderStatus.GetEnumTitle();
			ErpDocflowId = _edoDiscrepancy.ErpDocflowId.ToString();
			ErpDocflowTime = _edoDiscrepancy.ErpDocflowTime.ToString("dd.MM.yyyy HH:mm:ss");
			ErpDocflowStatus = _edoDiscrepancy.ErpDocflowStatus.GetEnumTitle();
			TaxcomDocflowId = _edoDiscrepancy.TaxcomDocflowId.ToString();
			TaxcomDocflowStatus = _edoDiscrepancy.TaxcomDocflowStatus.GetEnumTitle();
			TaxcomStatusTime = _edoDiscrepancy.TaxcomStatusTime.ToString("dd.MM.yyyy HH:mm:ss");

			DetermineDiscrepancy();

			Title = $"Расхождение ЭДО документа №{ErpDocflowId}";

			CompareUpd();
			ShowUpdCompareFields();

			ResendCommand = new BusyCommand("Переотправить УПД", Resend, () => !_actionInProgress && !ActionFinished);
			CancelOrderCommand = new BusyCommand("Отменить заказ", CancelOrder, () => !_actionInProgress && !ActionFinished);
		}

		public string OrderId { get; }
		public string DeliveryDate { get; }
		public string OrderStatus { get; }
		public string ErpDocflowId { get; }
		public string ErpDocflowStatus { get; }
		public string TaxcomDocflowId { get; }
		public string ErpDocflowTime { get; }
		public string TaxcomDocflowStatus { get; }
		public string TaxcomStatusTime { get; }
		public string TaxcomClientMessage { get; private set; }
		public string Discrepancy { get; private set; }

		public virtual bool OnlyDiffs
		{
			get => _onlyDiffs;
			set
			{
				if(SetField(ref _onlyDiffs, value))
				{
					ShowUpdCompareFields();
				}
			}
		}

		public virtual List<UpdCompareViewModel> UpdCompareFields
		{
			get => _updCompareFields;
			set => SetField(ref _updCompareFields, value);
		}

		public BusyCommand ResendCommand { get; }
		public BusyCommand CancelOrderCommand { get; }

		public virtual bool ActionInProgress
		{
			get => _actionInProgress;
			private set
			{
				if(SetField(ref _actionInProgress, value))
				{
					ResendCommand.RaiseCanExecuteChanged();
					CancelOrderCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public virtual bool ActionFinished
		{
			get => _actionFinished;
			set
			{
				if(SetField(ref _actionFinished, value))
				{
					ResendCommand.RaiseCanExecuteChanged();
					CancelOrderCommand.RaiseCanExecuteChanged();
				}
			}
		}

		private void Resend()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				try
				{
					ActionInProgress = true;

					if(!_interactiveService.Question(
						"Уверены что хотите переотправить УПД?", 
						"Подтверждение"))
					{
						return;
					}

					var result = _edoService.ResendDocumentWithDiscrepancy(uow, _edoDiscrepancy);
					if(result.IsSuccess)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Info, $"Заявка на переотправку УПД отправлена");
						ActionFinished = true;
					}
					else
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, $"Не удалось переотправить УПД: {result.GetErrorsString()}");
					}
				}
				finally
				{
					ActionInProgress = false;
				}
			}
		}

		private void CancelOrder()
		{
			_cancelOrderUow?.Dispose();
			_cancelOrderUow = _unitOfWorkFactory.CreateWithoutRoot();

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_cancelOrderUow, _edoDiscrepancy.OrderId);
			if(routeListItem?.RouteList?.Status == RouteListStatus.Closed)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"МЛ по текущему заказу закрыт. Обратитесь в кассу для дальнейших изменений",
					"Отмена заказа"
				);
				return;
			}

			if(!_interactiveService.Question(
				"Уверены что хотите отменить заказ?",
				"Подтверждение"))
			{
				return;
			}

			ActionInProgress = true;
			try
			{
				var orderCancellationPermit = new OrderCancellationPermit
				{
					Type = OrderCancellationPermitType.AllowCancelOrder,
					DocflowCancellationOfferConfirmation = true,
					OrderCancellationConfirmation = true
				};

				var page = NavigationManager.OpenViewModel<UndeliveryViewModel>(this, OpenPageOptions.AsSlave, (vm) => {
					vm.Initialize(_cancelOrderUow, _edoDiscrepancy.OrderId, cancellationPermit: orderCancellationPermit);
					vm.Saved += OnUndeliveryViewModelSaved;
				});
				page.PageClosed += UndeliveryPageClosed;
			}
			catch(Exception ex)
			{
				ActionInProgress = false;
				throw;
			}
		}

		private void UndeliveryPageClosed(object sender, PageClosedEventArgs e)
		{
			ActionInProgress = false;
		}

		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			var order = _cancelOrderUow.Session.Get<Order>(_edoDiscrepancy.OrderId);
			var currentEmployee = _employeeService.GetEmployeeForCurrentUser(_cancelOrderUow);

			order.SetUndeliveredStatus(_cancelOrderUow, _routeListService, _nomenclatureSettings, _callTaskWorker,
				needCreateDeliveryFreeBalanceOperation: true);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_cancelOrderUow, order.Id);
			if(routeListItem != null)
			{
				routeListItem.StatusLastUpdate = DateTime.Now;
				routeListItem.SetOrderActualCountsToZeroOnCanceled();
				_cancelOrderUow.Save(routeListItem);

				var notificationRequest = new NotificationRouteListChangesRequest
				{
					OrderId = e.UndeliveredOrder.OldOrder.Id,
					PushNotificationDataEventType = PushNotificationDataEventType.RouteListContentChanged
				};

				var notificationResult = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

				if(!notificationResult.IsSuccess)
				{
					var errorsString = string.Join(", ",
						notificationResult.Errors
						.Where(x => x.Code == RouteListErrors.RouteListItem.TransferTypeNotSet)
						.Select(x => x.Message)
					);
					var message = $"Не удалось отправить уведомление об изменениях в МЛ. Ошибки: {errorsString}";
						
					_interactiveService.ShowMessage(
						ImportanceLevel.Error,
						message,
						"Уведомление об изменениях в МЛ"
					);
				}
			}
			else
			{
				order.SetActualCountsToZeroOnCanceled();
			}

			order.SaveEntity(_cancelOrderUow, _orderContractUpdater, currentEmployee, _orderDailyNumberController, _paymentFromBankClientController);

			if(e.UndeliveredOrder.NewOrder != null)
			{
				var customerOrderRescheduledEvent = new CustomerNotificationDomainEvent(
					CustomerNotificationEventType.OrderRescheduled,
					e.UndeliveredOrder.OldOrder.OnlineOrder?.Source,
					e.UndeliveredOrder.OldOrder.OnlineOrder?.Id,
					e.UndeliveredOrder.OldOrder.Id,
					e.UndeliveredOrder.NewOrder.Id,
					e.UndeliveredOrder.UndeliveryDetalization?.Name
				);
				_notificationPublisher.TryPublish(_cancelOrderUow, customerOrderRescheduledEvent);
			}

			var result = _edoService.CancelOrderWithEdoDiscrepancy(_cancelOrderUow, _edoDiscrepancy, order);
			if(result.IsSuccess)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, $"Заказ №{_edoDiscrepancy.OrderId} отменен");
				ActionFinished = true;
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Не удалось отменить заказ: {result.GetErrorsString()}");
			}
		}

		private void DetermineDiscrepancy()
		{
			switch(_edoDiscrepancy.TaxcomDocflowStatus)
			{
				case EdoDocFlowStatus.Warning:
					Discrepancy = "Клиент отправил документ на уточнение";
					TaxcomClientMessage = _edoDiscrepancy.CorrectionNoticeMessage;
					break;
				case EdoDocFlowStatus.CompletedWithDivergences:
					Discrepancy = "Клиент принял документ с расхождениями";
					TaxcomClientMessage = _edoDiscrepancy.DiscrepancyMessage;
					break;
				case EdoDocFlowStatus.NotAccepted:
					Discrepancy = "Клиент не принял документ";
					break;
				case EdoDocFlowStatus.WaitingForCancellation:
				case EdoDocFlowStatus.Cancelled:
					Discrepancy = "Клиент отправил документ на аннулирование";
					TaxcomClientMessage = _edoDiscrepancy.CancellationOfferMessage;
					break;
				default:
					throw new AbortCreatingPageException(
						$"Не удалось определить расхождение в ЭДО документе №{_edoDiscrepancy.ErpDocflowId}",
						"Не найдено расхождение",
						ImportanceLevel.Warning
					);
			}
		}

		private void CompareUpd()
		{
			var currentUpd = _updDocumentBuilder.GetUpdDocumentWithoutCodes(_documentEdoTask);
			var sentUpd = _documentEdoTask.Upd;

			if(sentUpd == null)
			{
				return;
			}

			var organizationComparer = new ObjectPropertyComparer<UpdOrganization>()
				.DefaultInclude()
				.Build();

			var inventPositionComparer = new ObjectPropertyComparer<UpdInventPosition>()
				.DefaultInclude()
				.Exclude(x => x.Id)
				.Build();

			var paymentComparer = new ObjectPropertyComparer<UpdPayment>()
				.DefaultInclude()
				.Exclude(x => x.Id)
				.Build();

			var comparer = new ObjectPropertyComparer<EdoUpdDocument>()
				.DefaultInclude()
				.Exclude(x => x.Id)
				.Exclude(x => x.Guid)
				.CompareObject(x => x.Seller, organizationComparer)
				.CompareObject(x => x.Shipper, organizationComparer)
				.CompareObject(x => x.Customer, organizationComparer)
				.CompareObject(x => x.Consignee, organizationComparer)
				.CompareObject(x => x.InventPositions, inventPositionComparer)
				.CompareObject(x => x.Payments, paymentComparer)
				.Build();

			_differences = comparer.Compare(sentUpd, currentUpd, _diffFormatter);
		}

		private void ShowUpdCompareFields()
		{
			if(_differences == null)
			{
				UpdCompareFields = new List<UpdCompareViewModel>();
				return;
			}

			UpdCompareFields = _differences
				.Where(x => _onlyDiffs ? x.HasDiff : true)
				.Select(x => new UpdCompareViewModel(x, _onlyDiffs))
				.ToList();
		}

		public void Dispose()
		{
			_uow?.Dispose();
			_cancelOrderUow?.Dispose();
		}
	}
}
