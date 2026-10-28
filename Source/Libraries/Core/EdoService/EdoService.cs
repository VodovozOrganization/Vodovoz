using Core.Infrastructure;
using Edo.Admin;
using Edo.Contracts.Messages.Events;
using Edo.Transport;
using EdoService.Library.Factories;
using Gamma.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Client;
using Taxcom.Docflow.Utility;
using TransactionalOutbox.Domain;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Errors.Orders;
using Vodovoz.Services;
using VodovozBusiness.Errors.Edo;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Edo;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using EdoContainer = Vodovoz.Domain.Orders.Documents.EdoContainer;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;
using Order = Vodovoz.Domain.Orders.Order;

namespace EdoService.Library
{
	public class EdoService : IEdoService
	{
		private readonly ILogger<EdoService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly MessageService _messageService;
		private readonly IUserService _userService;
		private readonly IEmployeeService _employeeService;
		private readonly EdoCancellationService _edoCancellationService;
		private readonly ITaxcomApiFactory _taxcomApiFactory;
		private readonly IGenericRepository<FormalEdoRequest> _edoRequestRepository;
		private readonly IGenericRepository<OrderEdoTask> _edoTaskRepository;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountEntityController;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;
		private readonly IOrderEdoTaskCreatedEventPublisher _orderEdoTaskCreatedEventPublisher;
		private readonly IEnumerable<IInformalEdoRequestFactory> _requestFactories;
		private readonly IManualEdoRequestFactory _manualEdoRequestFactory;
		private readonly IBus _bus;

		private static EdoDocFlowStatus[] _successfulEdoStatuses => new[]
		{
			EdoDocFlowStatus.Succeed,
			EdoDocFlowStatus.InProgress
		};

		private static EdoDocumentStatus[] _resendableEdoDocumentStatuses => new[]
		{
			EdoDocumentStatus.Cancelled,
			EdoDocumentStatus.Error
		};

		private static EdoDocFlowStatus[] _resolvableDiscrepancyStatuses => new[]
		{
			EdoDocFlowStatus.Warning,
			EdoDocFlowStatus.CompletedWithDivergences,
			EdoDocFlowStatus.NotAccepted,
			EdoDocFlowStatus.WaitingForCancellation,
			EdoDocFlowStatus.Cancelled
		};

		private static EdoDocumentStatus[] _resendableWithCancellationEdoDocumentStatuses => new[]
		{
			EdoDocumentStatus.InProgress,
			EdoDocumentStatus.Sent
		};

		private static EdoDocumentStatus[] _resendWithOriginalCodesStatuses => new[]
		{
			EdoDocumentStatus.Warning,
			EdoDocumentStatus.CompletedWithDivergences
		};

		private static string[] _resendViaOrderDocumentSendEventProblemSources => new[]
		{
			"Custom.TaxcomDocumentSendingFail"
		};

		public EdoService(
			ILogger<EdoService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IEdoRepository edoRepository,
			MessageService messageService,
			IUserService userService,
			IEmployeeService employeeService,
			EdoCancellationService edoCancellationService,
			ITaxcomApiFactory taxcomApiFactory,
			IGenericRepository<FormalEdoRequest> edoRequestRepository,
			IGenericRepository<OrderEdoTask> edoTaskRepository,
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountEntityController,
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher,
			IOrderEdoTaskCreatedEventPublisher orderEdoTaskCreatedEventPublisher,
			IEnumerable<IInformalEdoRequestFactory> requestFactories,
			IManualEdoRequestFactory manualEdoRequestFactory,
			IBus bus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));
			_taxcomApiFactory = taxcomApiFactory ?? throw new ArgumentNullException(nameof(taxcomApiFactory));
			_edoRequestRepository = edoRequestRepository ?? throw new ArgumentNullException(nameof(edoRequestRepository));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
			_counterpartyEdoAccountEntityController =
				counterpartyEdoAccountEntityController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountEntityController));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
			_orderEdoTaskCreatedEventPublisher = orderEdoTaskCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(orderEdoTaskCreatedEventPublisher));
			_requestFactories = requestFactories ?? throw new ArgumentNullException(nameof(requestFactories));
			_manualEdoRequestFactory = manualEdoRequestFactory ?? throw new ArgumentNullException(nameof(manualEdoRequestFactory));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		public Result ResendEdoDocumentForOrder(OrderEntity order)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var task = _edoRepository.GetEdoTaskByOrder(uow, order.Id).FirstOrDefault();
				return ResendEdoDocument(uow, task.Id);
			}
		}

		public Result<string> ResendEdoDocumentForOrder(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				return ResendEdoDocument(uow, taskId);
			}
		}

		public Result<string> ResendEdoDocumentWithOriginalCodes(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку с исходными кодами"))
			{
				var edoTask = GetEdoTaskWithPessimisticLock(uow, taskId);
				if(edoTask is null)
				{
					return Result.Failure<string>(EdoErrors.NoCancelledEdoTaskForResend);
				}

				var order = GetOrderByTaskId(uow, taskId);
				var orderValidationResult = ValidateOrderForResend(order);
				if(orderValidationResult.IsFailure)
				{
					return Result.Failure<string>(orderValidationResult.Errors);
				}

				var existingCancellationRequest = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.FirstOrDefault(x => x.OriginalEdoTask.Id == taskId);

				if(existingCancellationRequest != null)
				{
					if(existingCancellationRequest.Status != EdoResendAfterTrueMarkCancellationStatus.CancellationFailed)
					{
						return Result.Success(
							existingCancellationRequest.Status == EdoResendAfterTrueMarkCancellationStatus.Completed
								? "Переотправка документа уже запущена"
								: "Документ уже находится в очереди на отмену вывода кодов из оборота и переотправку");
					}

					existingCancellationRequest.RetryCancellation();
					uow.Save(existingCancellationRequest);
					uow.Commit();

					return Result.Success("Повторная отмена вывода кодов из оборота поставлена в очередь");
				}

				var orderDocument = _edoRepository.GetOrderEdoDocumentByTaskId(uow, taskId);
				if(orderDocument?.DiscrepancyState == EdoDiscrepancyState.Active)
				{
					return Result.Failure<string>(EdoErrors.DocflowWithDiscrepancyCanResendOnlyInDiscrepancyDialog);
				}

				var withdrawalTaskIds = GetWithdrawalTaskIdsForBaseTask(uow, taskId);
				var withdrawalDocuments = GetSuccessfulWithdrawalDocumentsForTask(uow, order.Id, withdrawalTaskIds);

				if(withdrawalDocuments.Length == 0)
				{
					return Result.Failure<string>(EdoErrors.SuccessfulWithdrawalForResendNotFound);
				}

				if(withdrawalDocuments.Length > 1)
				{
					return Result.Failure<string>(EdoErrors.MultipleSuccessfulWithdrawalsForResendFound);
				}

				var checkOtherRequestsResult = CheckOtherRequests(
					uow,
					edoTask.FormalEdoRequest,
					taskId,
					withdrawalTaskIds,
					includeRequestsWithoutTask: true);
				if(checkOtherRequestsResult.IsFailure)
				{
					return Result.Failure<string>(checkOtherRequestsResult.Errors);
				}

				var checkOtherTasksResult = CheckOtherTasks(uow, edoTask, taskId, withdrawalTaskIds);
				if(checkOtherTasksResult.IsFailure)
				{
					return Result.Failure<string>(checkOtherTasksResult.Errors);
				}

				var productCodes = TrueMarkProductCodeFactory.CreateCodesFromTask(edoTask);
				var resendEdoRequest = _manualEdoRequestFactory.Create(uow, order, productCodes);
				var withdrawalDocument = withdrawalDocuments.Single();

				CancelEdoTaskWithReason(uow, edoTask);
				uow.Save(resendEdoRequest);

				var cancellationRequest = new EdoResendAfterTrueMarkCancellationRequest
				{
					Order = order,
					OriginalEdoTask = edoTask,
					ResendEdoRequest = resendEdoRequest,
					WithdrawalDocument = withdrawalDocument,
					Status = EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation,
					CreationTime = DateTime.Now,
					LastUpdateTime = DateTime.Now
				};

				uow.Save(cancellationRequest);
				uow.Commit();

				return Result.Success("Документ поставлен в очередь на переотправку после отмены вывода кодов из оборота в ЧЗ");
			}
		}

		public Result<string> ResendEdoDocumentWithCancellation(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Переотправляем действующий УПД с аннулированием документооборота"))
			{
				var edoTask = GetEdoTaskWithPessimisticLock(uow, taskId);
				if(!(edoTask is DocumentEdoTask documentTask))
				{
					return Result.Failure<string>(EdoErrors.NoEdoTask);
				}

				var order = documentTask.FormalEdoRequest?.Order;
				var orderValidationResult = ValidateOrderForResend(order);
				if(orderValidationResult.IsFailure)
				{
					return Result.Failure<string>(orderValidationResult.Errors);
				}

				var document = _edoRepository.GetOrderEdoDocumentByTaskId(uow, taskId);
				if(document is null || document.DocumentType != EdoDocumentType.UPD)
				{
					return Result.Failure<string>(EdoErrors.InvalidOutgoingDocumentType);
				}

				if(!_resendableWithCancellationEdoDocumentStatuses.Contains(document.Status))
				{
					return Result.Failure<string>(EdoErrors.CreateResendableEdoDocumentStatuses(
						order.Id,
						_resendableWithCancellationEdoDocumentStatuses));
				}

				var checkOtherRequestsResult = CheckOtherRequests(
					uow,
					documentTask.FormalEdoRequest,
					taskId,
					includeRequestsWithoutTask: true);
				if(checkOtherRequestsResult.IsFailure)
				{
					return Result.Failure<string>(checkOtherRequestsResult.Errors);
				}

				var checkOtherTasksResult = CheckOtherTasks(uow, documentTask, taskId);
				if(checkOtherTasksResult.IsFailure)
				{
					return Result.Failure<string>(checkOtherTasksResult.Errors);
				}

				if(!documentTask.Status.IsIn(EdoTaskStatus.InCancellation, EdoTaskStatus.Cancelled))
				{
					CancelEdoTaskWithReason(uow, documentTask, needCommit: true);

					uow.OpenTransaction();
					uow.Session.Refresh(documentTask, LockMode.Upgrade);
					checkOtherRequestsResult = CheckOtherRequests(
						uow, documentTask.FormalEdoRequest, taskId, includeRequestsWithoutTask: true);
					if(checkOtherRequestsResult.IsFailure)
					{
						return Result.Failure<string>(checkOtherRequestsResult.Errors);
					}

					checkOtherTasksResult = CheckOtherTasks(uow, documentTask, taskId);
					if(checkOtherTasksResult.IsFailure)
					{
						return Result.Failure<string>(checkOtherTasksResult.Errors);
					}
				}

				if(!documentTask.Status.IsIn(EdoTaskStatus.InCancellation, EdoTaskStatus.Cancelled))
				{
					return Result.Failure<string>(EdoErrors.HasProblem);
				}

				ResendDocumentForCancelledEdoTask(uow, order, documentTask, transferCodeReservations: true);

				return Result.Success("Старый документооборот отправлен на аннулирование. УПД отправлен на переотправку.");
			}
		}

		public Result<string> ResendNewEdoTask(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Повторный запуск новой задачи ЭДО"))
			{
				var edoTask = uow.Session.Get<OrderEdoTask>(taskId);
				if(edoTask is null)
				{
					return Result.Failure<string>(new Error(
						"EdoTaskNotFound",
						$"ЭДО задача №{taskId} не найдена"));
				}

				if(edoTask.Status != EdoTaskStatus.New)
				{
					return Result.Failure<string>(new Error(
						"EdoTaskIsNotNew",
						$"Повторный запуск ЭДО задачи №{taskId} доступен только в статусе Новая"));
				}

				var validationResult = ValidateNewEdoTaskStage(edoTask);
				if(validationResult.IsFailure)
				{
					return Result.Failure<string>(validationResult.Errors);
				}

				_orderEdoTaskCreatedEventPublisher.Publish(uow, edoTask);
				uow.Commit();

				return Result.Success("Задача успешно отправлена на повторную обработку");
			}
		}

		/// <inheritdoc />
		public Result<string> ResendBillEdoDocument(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Повторная отправка счета через ЭДО"))
			{
				var request = uow.GetAll<InformalEdoRequest>().FirstOrDefault(x => x.Task.Id == taskId
					&& (x.OrderDocumentType == OrderDocumentType.Bill || x.OrderDocumentType == OrderDocumentType.SpecialBill));
				if(request?.Order == null)
				{
					return Result.Failure<string>(new Error("EdoBillRequestNotFound", "Заявка счета заказа не найдена"));
				}

				if(request.Task.Status == EdoTaskStatus.New)
				{
					request.Author = _employeeService.GetEmployeeForCurrentUser(uow);
					request.Source = EdoRequestSource.Manual;
					uow.Save(request);
					uow.Save(new OutboxMessage(new InformalOrderDocumenTaskCreatedEvent { InformalOrderDocumentTaskId = taskId }));
					uow.Commit();
					return Result.Success("Задача счета отправлена на повторную обработку");
				}

				var document = uow.GetAll<OutgoingInformalEdoDocument>()
					.Where(x => x.InformalDocumentTaskId == taskId)
					.OrderByDescending(x => x.Id)
					.FirstOrDefault();
				if(!CanResendBillEdoDocument(request.Task.Status, document?.Status))
				{
					return Result.Failure<string>(new Error("EdoBillCannotBeResent", "Повторная отправка доступна для счета с ошибкой или аннулированного счета"));
				}

				var order = uow.GetById<Order>(request.Order.Id);
				if(order == null)
				{
					return Result.Failure<string>(new Error("EdoBillOrderNotFound", "Заказ счета не найден"));
				}

				if(!QueueBillRequest(uow, order, DocumentContainerType.Bill, request.OrderDocumentType))
				{
					return Result.Success("Счет уже ожидает подготовки к повторной отправке");
				}

				if(request.Task.Status != EdoTaskStatus.Cancelled)
				{
					request.Task.ProcessTaskCancellation(EdoTaskStatus.Cancelled,
						$"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}");
					uow.Save(request.Task);
				}

				uow.Commit();
				return Result.Success("Счет поставлен на повторную отправку через ЭДО");
			}
		}

		public void AllowResendInClosedAccountingPeriod(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Разрешение переотправки ЭДО в закрытом бухгалтерском периоде"))
			{
				var edoTask = uow.Session.Get<OrderEdoTask>(taskId);
				if(edoTask?.FormalEdoRequest == null)
				{
					return;
				}

				edoTask.FormalEdoRequest.Source = EdoRequestSource.ClosedPeriodPrivilegedResend;
				uow.Save(edoTask.FormalEdoRequest);
				uow.Commit();
			}
		}

		private static Result ValidateNewEdoTaskStage(OrderEdoTask edoTask)
		{
			bool canResume;
			switch (edoTask)
			{
				case DocumentEdoTask documentTask:
					canResume = documentTask.Stage == DocumentEdoTaskStage.New
					            && documentTask.DocumentType == EdoDocumentType.UPD;
					break;
				case ReceiptEdoTask receiptTask:
					canResume = receiptTask.ReceiptStatus == EdoReceiptStatus.New;
					break;
				case TenderEdoTask tenderTask:
					canResume = tenderTask.Stage == TenderEdoTaskStage.New;
					break;
				default:
					canResume = edoTask is SaveCodesEdoTask;
					break;
			}

			return canResume
				? Result.Success()
				: Result.Failure(new Error(
					"EdoTaskResendIsNotSupported",
					$"ЭДО задача №{edoTask.Id} типа {edoTask.TaskType} не может быть повторно запущена в текущем состоянии"));
		}

		private Result<string> ResendEdoDocument(IUnitOfWork uow, int taskId)
		{
			var edoTask = uow.Session.Get<OrderEdoTask>(taskId);
			if(edoTask is null)
			{
				return Result.Failure<string>(EdoErrors.NoCancelledEdoTaskForResend);
			}

			var order = GetOrderByTaskId(uow, taskId);

			if(order.IsUndeliveredStatus)
			{
				return Result.Failure<string>(EdoErrors.IsUndeliveredOrder);
			}

			bool hasDocflow = HasDocflow(uow, edoTask);
			bool hasCancelledDocflow = HasCancelledDocflow(uow, edoTask.Id);

			if(!hasDocflow && edoTask.Status == EdoTaskStatus.Problem
				&& edoTask.As<DocumentEdoTask>()?.Stage == DocumentEdoTaskStage.New)
			{
				var otherTasksResult = CheckOtherTasks(uow, edoTask, taskId);
				if(otherTasksResult.IsFailure)
				{
					return Result.Failure<string>(otherTasksResult.Errors);
				}

				_messageService.PublishTaskCreatedEvent(edoTask).GetAwaiter().GetResult();
				return Result.Success("Задача успешно отправлена на повторную обработку");
			}

			var checkOtherRequestsResult = CheckOtherRequests(uow, edoTask.FormalEdoRequest, taskId);
			if(checkOtherRequestsResult.IsFailure)
			{
				return Result.Failure<string>(checkOtherRequestsResult.Errors);
			}

			var checkOtherTasksResult = CheckOtherTasks(uow, edoTask, taskId);
			if(checkOtherTasksResult.IsFailure)
			{
				return Result.Failure<string>(checkOtherRequestsResult.Errors);
			}

			if(hasCancelledDocflow)
			{
				if(EdoTaskHasBeenCancelled(uow, edoTask))
				{
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
				else
				{
					CancelEdoTaskWithReason(uow, edoTask);
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
			}
			else if(!hasDocflow)
			{
				if(edoTask.Status is EdoTaskStatus.Problem)
				{
					RehandleNewUpdDocumentWithProblem(taskId);
				}
				else if(EdoTaskHasBeenCancelled(uow, edoTask))
				{
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
				else
				{
					CancelEdoTaskWithReason(uow, edoTask);
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
			}
			else
			{
				return Result.Failure<string>(EdoErrors.HasProblem);
			}

			return Result.Success("Успешно переотправлено");
		}

		private static OrderEdoTask GetEdoTaskWithPessimisticLock(IUnitOfWork uow, int taskId)
		{
			uow.OpenTransaction();

			return uow.Session.Get<OrderEdoTask>(taskId, LockMode.Upgrade);
		}

		private Result<string> CheckOtherRequests(
			IUnitOfWork uow,
			FormalEdoRequest request,
			int taskId,
			IEnumerable<int> ignoredTaskIds = null,
			bool includeRequestsWithoutTask = false)
		{
			var ignoredTaskIdsArray = ignoredTaskIds ?? Enumerable.Empty<int>();
			Expression<Func<FormalEdoRequest, bool>> otherRequestsExpression;

			if(includeRequestsWithoutTask)
			{
				otherRequestsExpression = x => x.Order.Id == request.Order.Id
					&& (x.Task == null || x.Task.Id != taskId)
					&& (x.Task == null || !ignoredTaskIdsArray.Contains(x.Task.Id));
			}
			else
			{
				otherRequestsExpression = x => x.Order.Id == request.Order.Id
					&& x.Task.Id != taskId;
			}

			var hasOtherRequests = _edoRequestRepository.GetCount(uow, otherRequestsExpression) > 0;

			if(hasOtherRequests)
			{
				return Result.Failure<string>(new Error("DocumentHasOtherRequests",
					$"Переотправка документа невозможна, т.к. помимо текущего документа " +
					$"по заказу {request.Order.Id} уже есть другая отправка")
				);
			}

			return Result.Success("OK");
		}

		private Result<string> CheckOtherTasks(
			IUnitOfWork uow,
			OrderEdoTask edoTask,
			int taskId,
			IEnumerable<int> ignoredTaskIds = null)
		{
			var ignoredTaskIdsArray = ignoredTaskIds ?? Array.Empty<int>();

			var hasOtherTasks = _edoTaskRepository.GetCount(uow, x =>
				x.FormalEdoRequest.Order.Id == edoTask.FormalEdoRequest.Order.Id
				&& x.Id != taskId
				&& !ignoredTaskIdsArray.Contains(x.Id)
				&& x.Status != EdoTaskStatus.Cancelled
				&& !(x is SaveCodesEdoTask)
			) > 0;

			if(hasOtherTasks)
			{
				return Result.Failure<string>(new Error("DocumentHasOtherTasks",
					$"Переотправка документа невозможна, т.к. помимо текущего документа " +
					$"по заказу {edoTask.FormalEdoRequest.Order.Id} уже есть другая неотмененная задача")
				);
			}
			return Result.Success("OK");
		}

		public Result<string> CancelDocflow(int edoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Создаем запрос на аннулирование в ДО"))
			{
				var edoTask = uow.Session.Get<OrderEdoTask>(edoTaskId);

				CreateEventForEdoTaskCancellation(edoTask);

				return Result.Success($"Задача {edoTask.Id} отправлена на аннулирование");
			}
		}

		public bool HasCancelledDocflow(int edoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Проверка возможности переотправки документа ЭДО"))
			{
				return HasCancelledDocflow(uow, edoTaskId);
			}
		}

		public bool HasDocflow(int edoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Проверка наличия документооборота ЭДО"))
			{
				var edoTask = uow.Session.Get<OrderEdoTask>(edoTaskId);
				if(edoTask is null)
				{
					return false;
				}
				return HasDocflow(uow, edoTask);
			}
		}

		private bool HasCancelledDocflow(IUnitOfWork uow, int edoTaskId)
		{
			var orderDocument = _edoRepository.GetOrderEdoDocumentByTaskId(uow, edoTaskId);

			if(orderDocument != null && CanResendEdoDocument(orderDocument.Status))
			{
				return true;
			}

			return false;
		}

		private bool HasDocflow(IUnitOfWork uow, OrderEdoTask edoTask)
		{
			var orderDocument = _edoRepository.GetOrderEdoDocumentByTaskId(uow, edoTask.Id);

			if(orderDocument != null)
			{
				return true;
			}

			return false;
		}

		private void ResendDocumentForCancelledEdoTask(
			IUnitOfWork uow, 
			OrderEntity order, 
			OrderEdoTask edoTask, 
			bool transferCodeReservations = false
		)
		{
			var productCodes = TrueMarkProductCodeFactory.CreateCodesFromTask(edoTask);
			var request = _manualEdoRequestFactory.Create(uow, order, productCodes);

			if(transferCodeReservations)
			{
				var releasedCodes = false;
				foreach(var oldCode in edoTask.Items.Select(x => x.ProductCode))
				{
					if(oldCode.ResultCode == null)
					{
						continue;
					}

					var newCode = productCodes.FirstOrDefault(x => x.SourceCode != null
						&& x.SourceCode.Id == oldCode.ResultCode.Id);
					if(newCode == null)
					{
						continue;
					}

					newCode.ResultCode = oldCode.ResultCode;
					oldCode.ResultCode = null;
					uow.Save(oldCode);
					releasedCodes = true;
				}

				if(releasedCodes)
				{
					uow.Session.Flush();
				}
			}

			uow.Save(request);
			uow.Commit();

			_edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка документов ЭДО")
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		public Result ResendDocumentWithDiscrepancy(
			IUnitOfWork uow,
			EdoDiscrepancyNode edoDiscrepancyNode
		)
		{
			try
			{
				var order = uow.Session.Get<OrderEntity>(edoDiscrepancyNode.OrderId);
				var sellerEdoAccountId = order.Contract.Organization.TaxcomEdoSettings.EdoAccount;

				var resolveResult = ResolveDiscrepancy(uow, edoDiscrepancyNode, sellerEdoAccountId);
				if(resolveResult.IsFailure)
				{
					return resolveResult;
				}

				var request = GetFormalEdoRequestByTaskId(uow, edoDiscrepancyNode.DocumentEdoTaskId);

				if(request == null)
				{
					return Result.Failure(new Error("", $"Не найден ЭДО запрос на отправку " +
						$"УПД для задачи №{edoDiscrepancyNode.DocumentEdoTaskId}"));
				}

				var edoTask = uow.Session.Get<OrderEdoTask>(edoDiscrepancyNode.DocumentEdoTaskId);
				var currentEmployee = _employeeService.GetEmployeeForCurrentUser(uow);

				// создать новые коды
				var newProductCodes = TrueMarkProductCodeFactory.CreateCodesFromSource(request.ProductCodes);
				// добавить их в новую заявку 
				var newEdoRequest = new ManualEdoRequest
				{
					Order = order,
					Author = currentEmployee,
					DocumentType = EdoDocumentType.UPD,
					Source = EdoRequestSource.Manual,
					Type = CustomerEdoRequestType.Order,
					ProductCodes = new ObservableList<TrueMarkProductCode>(newProductCodes)
				};
				uow.Save(newEdoRequest);

				// отклонить старые
				foreach(var productCode in request.ProductCodes)
				{
					productCode.SourceCodeStatus = SourceProductCodeStatus.Rejected;
					productCode.ResultCode = null;
					uow.Save(productCode);
				}

				// отменить прошлую задачу
				edoTask.Status = EdoTaskStatus.Cancelled;
				edoTask.CancellationReason = $"Переотправка УПД, потому что он был переведен в статус {edoDiscrepancyNode.TaxcomDocflowStatus.GetEnumTitle()}";
				uow.Save(edoTask);

				// сделать отправку новой заявки через outbox
				var reqestCreatedEvent = new EdoRequestCreatedEvent
				{
					Id = newEdoRequest.Id
				};

				uow.Save(new OutboxMessage(reqestCreatedEvent));
				uow.Commit();

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при переотправке документа с расхождениями");
				return Result.Failure(new Error("Ошибка", ex.Message));
			}
		}

		public Result CancelOrderWithEdoDiscrepancy(IUnitOfWork uow, EdoDiscrepancyNode edoDiscrepancyNode, Order order)
		{
			try
			{
				var sellerEdoAccountId = order.Contract.Organization.TaxcomEdoSettings.EdoAccount;

				var resolveResult = ResolveDiscrepancy(uow, edoDiscrepancyNode, sellerEdoAccountId);
				if(resolveResult.IsFailure)
				{
					return resolveResult;
				}

				var request = GetFormalEdoRequestByTaskId(uow, edoDiscrepancyNode.DocumentEdoTaskId);

				if(request == null)
				{
					return Result.Failure(new Error("", $"Не найден ЭДО запрос на отправку " +
						$"УПД для задачи №{edoDiscrepancyNode.DocumentEdoTaskId}"));
				}

				var edoTask = uow.Session.Get<OrderEdoTask>(edoDiscrepancyNode.DocumentEdoTaskId);

				// отклонить старые коды
				foreach(var productCode in request.ProductCodes)
				{
					productCode.SourceCodeStatus = SourceProductCodeStatus.Rejected;
					productCode.ResultCode = null;
					uow.Save(productCode);
				}

				// отменить прошлую задачу
				edoTask.Status = EdoTaskStatus.Cancelled;
				edoTask.CancellationReason = $"Отмена заказа, потому что УПД был переведен в статус {edoDiscrepancyNode.TaxcomDocflowStatus.GetEnumTitle()}";
				uow.Save(edoTask);

				uow.Commit();

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отмене заказа");
				return Result.Failure(new Error("Ошибка", ex.Message));
			}
		}

		private static FormalEdoRequest GetFormalEdoRequestByTaskId(IUnitOfWork uow, int taskId)
		{
			return uow.GetAll<FormalEdoRequest>()
				.FirstOrDefault(x => x.Task.Id == taskId);
		}

		/// <summary>
		/// Переводит расхождение документа в состояние «Разобрано».
		/// Если клиент запросил аннулирование, отправляет принятие ПОА.
		/// Статус документа не меняется: его зафиксирует обработчик статусов документооборота
		/// </summary>
		private Result ResolveDiscrepancy(IUnitOfWork uow, EdoDiscrepancyNode edoDiscrepancyNode, string sellerEdoAccountId)
		{
			if(!_resolvableDiscrepancyStatuses.Contains(edoDiscrepancyNode.TaxcomDocflowStatus))
			{
				throw new NotSupportedException($"Статус {edoDiscrepancyNode.TaxcomDocflowStatus} не поддерживается для расхождений ЭДО");
			}

			var document = uow.Session.Get<OrderEdoDocument>(edoDiscrepancyNode.ErpDocflowId);
			if(document is null)
			{
				return Result.Failure(EdoErrors.DiscrepancyAlreadyResolved);
			}

			// Блокируем документ и перечитываем его состояние, чтобы исключить повторный разбор
			uow.Session.Refresh(document, LockMode.Upgrade);

			if(document.DiscrepancyState != EdoDiscrepancyState.Active)
			{
				return Result.Failure(EdoErrors.DiscrepancyAlreadyResolved);
			}

			document.DiscrepancyState = EdoDiscrepancyState.Resolved;

			if(edoDiscrepancyNode.TaxcomDocflowStatus == EdoDocFlowStatus.WaitingForCancellation)
			{
				document.CancellationRequestTime = DateTime.Now;

				// отправить принятие ПОА
				var cancellationAccept = new AcceptingWaitingForCancellationDocflowEvent
				{
					DocFlowId = edoDiscrepancyNode.TaxcomDocflowId.ToString(),
					EdoAccount = sellerEdoAccountId
				};
				uow.Save(new OutboxMessage(cancellationAccept));
			}

			uow.Save(document);

			return Result.Success();
		}

		private OrderEntity GetOrderByTaskId(IUnitOfWork uow, int taskId)
		{
			var edoTask = uow.Session.Get<DocumentEdoTask>(taskId);
			return edoTask?.FormalEdoRequest?.Order;
		}

		private static Result ValidateOrderForResend(OrderEntity order)
		{
			if(order is null)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			return order.IsUndeliveredStatus ? Result.Failure(EdoErrors.IsUndeliveredOrder) : Result.Success();
		}

		public bool CanResendEdoDocument(EdoDocumentStatus? status) => status.HasValue
			&& _resendableEdoDocumentStatuses.Contains(status.Value);

		/// <inheritdoc />
		public bool CanResendBillEdoDocument(EdoTaskStatus taskStatus, EdoDocumentStatus? documentStatus) =>
			taskStatus == EdoTaskStatus.New
			|| CanResendEdoDocument(documentStatus)
			|| (taskStatus == EdoTaskStatus.Problem && documentStatus == EdoDocumentStatus.NotStarted);

		private static int[] GetWithdrawalTaskIdsForBaseTask(IUnitOfWork uow, int taskId)
		{
			return uow.GetAll<WithdrawalEdoRequest>()
				.Where(x => x.BaseDocumentEdoTask.Id == taskId && x.Task != null)
				.Select(x => x.Task.Id)
				.ToArray();
		}

		private static TrueMarkDocument[] GetSuccessfulWithdrawalDocumentsForTask(
			IUnitOfWork uow,
			int orderId,
			IEnumerable<int> withdrawalTaskIds)
		{
			var withdrawalTaskIdsArray = withdrawalTaskIds ?? Array.Empty<int>();
			var withdrawalDocuments = withdrawalTaskIdsArray.Any()
				? uow.GetAll<TrueMarkDocument>()
					.Where(x => x.WithdrawalEdoTask != null
						&& withdrawalTaskIdsArray.Contains(x.WithdrawalEdoTask.Id)
						&& x.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal
						&& x.IsSuccess
						&& x.Guid != null)
					.ToArray()
				: Array.Empty<TrueMarkDocument>();

			if(withdrawalDocuments.Length > 0)
			{
				return withdrawalDocuments;
			}

			// У документов, созданных до добавления связи с задачей вывода, доступна только привязка к заказу.
			return uow.GetAll<TrueMarkDocument>()
				.Where(x => x.Order.Id == orderId
					&& x.WithdrawalEdoTask == null
					&& x.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal
					&& x.IsSuccess
					&& x.Guid != null)
				.ToArray();
		}

		/// <summary>
		/// Проверяет, была ли отменена ЭДО задача для переотправки документа (при наличии маркированной продукции в заказе)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="edoTask">ЭДО задача</param>
		/// <returns>True, если задача была отменена или в заказе нет КМ, False в противном случае</returns>
		private bool EdoTaskHasBeenCancelled(IUnitOfWork uow, OrderEdoTask edoTask)
		{
			var order = edoTask.FormalEdoRequest?.Order;
			var orderItems = _orderRepository.GetOrderItems(uow, order.Id);
			var hasMarkedProducts = orderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

			if(!hasMarkedProducts)
			{
				return true;
			}

			var cancelledEdoTaskWithRejectedCodes = edoTask.Status is EdoTaskStatus.Cancelled 
				&& edoTask.FormalEdoRequest.ProductCodes.Any(c =>
				c.SourceCodeStatus is SourceProductCodeStatus.Rejected
				&& c.ResultCode != null);

			return cancelledEdoTaskWithRejectedCodes;
		}

		public virtual void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				if(type is DocumentContainerType.Upd)
				{
					var edoDocumentsActions = UpdateEdoDocumentAction(uow, entity, type);

					var orderLastTrueMarkDocument = uow.GetAll<TrueMarkDocument>()
						.Where(x => x.Order.Id == entity.GetId())
						.OrderByDescending(x => x.CreationDate)
						.FirstOrDefault();

					if(orderLastTrueMarkDocument != null
						&& orderLastTrueMarkDocument.Type != TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation)
					{
						edoDocumentsActions.IsNeedToCancelTrueMarkDocument = true;
					}

					var edoTask =
						uow
							.GetAll<BulkAccountingEdoTask>()
							.FirstOrDefault(x => x.FormalEdoRequest.Order.Id == entity.Id);
					
					if(edoTask != null)
					{
						edoTask.Status = EdoTaskStatus.New;
						uow.Save(edoTask);
					}
					
					uow.Save(edoDocumentsActions);
				}
				else
				{
					QueueBillRequest(uow, entity, type);
				}

				uow.Commit();
			}
		}

		private OrderEdoTrueMarkDocumentsActions UpdateEdoDocumentAction(IUnitOfWork uow, IDomainObject entity, DocumentContainerType type)
		{
			var edoDocumentsAction = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(x => x.Order.Id == entity.Id)
					.FirstOrDefault() ?? new OrderEdoTrueMarkDocumentsActions();

			edoDocumentsAction.Order = (Order)entity;
			edoDocumentsAction.IsNeedToResendEdoUpd = true;
			edoDocumentsAction.Created = DateTime.Now;

			return edoDocumentsAction;
		}

		private bool QueueBillRequest(IUnitOfWork uow, IDomainObject entity, DocumentContainerType type,
			OrderDocumentType? orderBillType = null)
		{
			InformalEdoRequest request;
			switch(type)
			{
				case DocumentContainerType.Bill:
					var order = (Order)entity;
					var document = order.OrderDocuments.FirstOrDefault(x =>
						x.Order.Id == order.Id && (orderBillType.HasValue ? x.Type == orderBillType.Value
							: x.Type == OrderDocumentType.Bill || x.Type == OrderDocumentType.SpecialBill))
						?? throw new InvalidOperationException("В заказе отсутствует счет для отправки");
					request = BillEdoRequestFactory.CreatePending(document.Type, document.Id,
						uow.Session.Load<OrderEntity>(order.Id));
					break;
				case DocumentContainerType.BillWSForDebt:
					request = BillEdoRequestFactory.CreatePending(OrderDocumentType.BillWSForDebt, entity.Id);
					break;
				case DocumentContainerType.BillWSForPayment:
					request = BillEdoRequestFactory.CreatePending(OrderDocumentType.BillWSForPayment, entity.Id);
					break;
				case DocumentContainerType.BillWSForAdvancePayment:
					request = BillEdoRequestFactory.CreatePending(OrderDocumentType.BillWSForAdvancePayment, entity.Id);
					break;
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}

			if(uow.GetAll<InformalEdoRequest>().Any(x => x.SourceDocumentId == request.SourceDocumentId
				&& x.OrderDocumentType == request.OrderDocumentType && x.DocumentSnapshot == null
				&& (x.Task == null || x.Task.Status != EdoTaskStatus.Cancelled)))
			{
				return false;
			}

			request.Source = request.Order == null ? EdoRequestSource.OrderWithoutShipment : EdoRequestSource.Manual;
			request.Author = _employeeService.GetEmployeeForCurrentUser(uow);
			uow.Save(request);
			return true;
		}

		public Result ValidateEdoContainers(IList<EdoContainer> edoContainers)
		{
			var errors = new List<Error>();

			foreach(var edoContainer in edoContainers)
			{
				if(_successfulEdoStatuses.Contains(edoContainer.EdoDocFlowStatus))
				{
					errors.Add(EdoErrors.CreateAlreadySuccefullSended(edoContainer));
				}
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateEdoOrderDocument(IUnitOfWork uow, OrderEdoDocument document)
		{
			if(document is null)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var order = _orderRepository.GetOrderByOrderEdoDocumentId(uow, document.Id);

			if(order is null)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var errors = new List<Error>();

			if(!_resendableEdoDocumentStatuses.Contains(document.Status))
			{
				errors.Add(EdoErrors.CreateResendableEdoDocumentStatuses(order.Id, _resendableEdoDocumentStatuses));
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus is OrderPaymentStatus.Paid)
			{
				errors.Add(EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateOrderForDocumentType(OrderEntity order, EdoDocumentType type)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus is OrderPaymentStatus.Paid)
			{
				errors.Add(EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateOutgoingDocument(IUnitOfWork uow, EdoDockflowData dockflowData)
		{
			var type = dockflowData.EdoDocumentType.Value;

			if(dockflowData.OrderId.HasValue == false)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			if(dockflowData.DocFlowId.HasValue == false)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var order = _orderRepository.GetOrder(uow, dockflowData.OrderId.Value);

			var ValidateOrderForDocumentTypeResult = ValidateOrderForDocumentType(order, type);

			if(ValidateOrderForDocumentTypeResult.IsFailure)
			{
				return ValidateOrderForDocumentTypeResult;
			}

			return Result.Success();
		}

		public Result SendDocumentTaskCreatedEvent(EdoTask edoTask)
		{
			_messageService.PublishSendDocumentTaskCreatedEvent(edoTask.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
			
			return Result.Success();
		}

		public void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order)
		{
			const string reason = "Состав заказа был изменен";
			var requestedAt = DateTime.Now;
			var requests = unitOfWork.GetAll<InformalEdoRequest>()
				.Where(x => x.Order.Id == order.Id && x.Time <= requestedAt
					&& (x.OrderDocumentType == OrderDocumentType.Bill || x.OrderDocumentType == OrderDocumentType.SpecialBill))
				.ToList();
			foreach(var request in requests)
			{
				if(request.Task == null)
				{
					request.Task = new OrderDocumentEdoTask { Status = EdoTaskStatus.Cancelled,
						CancellationReason = reason, EndTime = requestedAt };
					unitOfWork.Save(request);
				}
				else if(request.Task.Status != EdoTaskStatus.Cancelled && request.Task.Status != EdoTaskStatus.InCancellation)
				{
					request.Task.CancellationReason = reason;
					unitOfWork.Save(request.Task);
				}
			}

			var containers = _orderRepository.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Where(x => x.Type == DocumentContainerType.Bill && x.Created <= requestedAt
					&& x.DocFlowId.HasValue && x.EdoDocFlowStatus != EdoDocFlowStatus.Warning
					&& x.EdoDocFlowStatus != EdoDocFlowStatus.Error
					&& x.EdoDocFlowStatus != EdoDocFlowStatus.WaitingForCancellation
					&& x.EdoDocFlowStatus != EdoDocFlowStatus.Cancelled);
			foreach(var container in containers)
			{
				unitOfWork.Save(new OutboxMessage(new TaxcomDocflowRequestCancellationEvent
				{
					LegacyBillDocflowId = container.DocFlowId,
					EdoAccount = order.Contract.Organization.TaxcomEdoSettings?.EdoAccount,
					CancellationReason = reason
				}));
			}
			unitOfWork.Commit();
		}

		public Result ValidateOrderForOrderDocument(EdoDocFlowStatus status)
		{
			var errors = new List<Error>();

			if(status is EdoDocFlowStatus.InProgress 
				|| status is EdoDocFlowStatus.Succeed)
			{
				errors.Add(EdoErrors.AlreadySuccefullSended);
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type)
		{
			var orderValidationResult = ValidateOrderForResend(order);
			if(orderValidationResult.IsFailure)
			{
				return orderValidationResult;
			}

			using(var uow = _uowFactory.CreateWithoutRoot("Публикация неформализованной заявки ЭДО"))
			{
				var informalRequest = uow.GetAll<InformalEdoRequest>()
					.FirstOrDefault(r => r.Order.Id == order.Id && r.OrderDocumentType == type);
				
				if(informalRequest is null)
				{
					var factory = _requestFactories.FirstOrDefault(f => f.CanCreateFor(type))
						?? throw new NotSupportedException($"Не найден фабричный метод для типа документа {type}");

					informalRequest = factory.Create(order);
					uow.Save(informalRequest);
				}

				uow.Commit();

                _messageService.PublishInformalEdoRequestCreatedEvent(informalRequest.Id)
                    .GetAwaiter().GetResult();

				return Result.Success();
			}
		}

		public async Task<Result> ResendReceiptDocument(
			int receiptEdoTaskId,
			CancellationToken cancellationToken = default)
		{	
			using(var uow = _uowFactory.CreateWithoutRoot("Переотправка чека"))
			{
				var receiptTask = uow.Session.Get<ReceiptEdoTask>(receiptEdoTaskId);
				if(receiptTask is null)
				{
					return Result.Failure(EdoErrors.HasProblem);
				}

				var order = receiptTask.FormalEdoRequest?.Order;
				if(order is null)
				{
					return Result.Failure(EdoErrors.HasProblem);
				}

				var canResendResult = CanResendReceipt(receiptTask);
				if(canResendResult.IsFailure)
				{
					return canResendResult;
				}

				receiptTask.ReceiptStatus = EdoReceiptStatus.New;

				var productCodes = new ObservableList<TrueMarkProductCode>(
					receiptTask.Items.Select(x => x.ProductCode)
				);

				var request = _manualEdoRequestFactory.Create(uow, order, productCodes);

				CancelEdoTaskWithReason(uow, receiptTask);

				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.SaveAsync(receiptTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				await _edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка чека", cancellationToken);

				return Result.Success();
			}
		}

		public Result RehandleNewReceiptDocumentWithProblem(int receiptEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<ReceiptEdoTask>(receiptEdoTaskId);
				if(task == null)
				{
					return Result.Failure(new Error("ReceiptEdoTaskNotFound",
						$"ЭДО задача №{receiptEdoTaskId} на отправку чека не найдена, " +
						$"обратитесь в техподдержку"));
				}

				if(task.Status != EdoTaskStatus.Problem)
				{
					return Result.Failure(new Error("ReceiptEdoTaskDontHaveProblem",
						$"ЭДО задача №{receiptEdoTaskId} на отправку чека не имеет нерешенных проблем для переобработки."
					));
				}

				if(task.ReceiptStatus != EdoReceiptStatus.New)
				{
					return Result.Failure(new Error("ReceiptEdoTaskCantRehandleProblemInCurrentStage",
						$"Для ЭДО задачи №{receiptEdoTaskId} на отправку чека " +
						$"в стадии {task.ReceiptStatus.GetEnumTitle()} не доступна переобработка проблемы."
					));
				}

				var message = new ReceiptTaskCreatedEvent
				{
					ReceiptEdoTaskId = receiptEdoTaskId,
				};
				_bus.Publish(message);

				return Result.Success();
			}
		}

		private void CreateEventForEdoTaskCancellation(EdoTask edoTask)
		{
			var message = new RequestDocflowCancellationEvent
			{
				TaskId = edoTask.Id,
				Reason = $"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}"
			};

			_bus.Publish(message)
				.GetAwaiter()
				.GetResult();
		}

		private void CancelEdoTaskWithReason(IUnitOfWork uow, EdoTask edoTask, bool needCommit = false)
		{
			var cancellationReason = $"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}";
			_edoCancellationService.CancelTask(edoTask.Id, cancellationReason, true, uow: uow, needCommit: needCommit)
				.GetAwaiter()
				.GetResult();
		}

		private Result CanResendReceipt(ReceiptEdoTask receiptTask)
		{
			var errors = new List<Error>();

			if(receiptTask.Status is EdoTaskStatus.Completed || receiptTask.Status is EdoTaskStatus.InCancellation)
			{
				errors.Add(EdoErrors.CreateCannotResendCompletedTask(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.Completed)
			{
				errors.Add(EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.SavedToPool)
			{
				errors.Add(EdoErrors.CreateCannotResendReceiptFromSavedToPool(receiptTask.Id));
			}

			if(receiptTask.FiscalDocuments?.Any() == true)
			{
				var hasInvalidDocument = receiptTask.FiscalDocuments.Any(fd =>
					fd.Stage is FiscalDocumentStage.Completed ||
					!string.IsNullOrEmpty(fd.FiscalNumber) ||
					fd.Status is FiscalDocumentStatus.Printed || 
					fd.Status is FiscalDocumentStatus.Completed);

				if(hasInvalidDocument)
				{
					errors.Add(EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
				}
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public Result<string> TryResendUpdDocument(int orderEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var request = _edoRequestRepository
					.GetFirstOrDefault(uow, x => x.Task.Id == orderEdoTaskId);

				var orderValidationResult = ValidateOrderForResend(request.Order);
				if(orderValidationResult.IsFailure)
				{
					return Result.Failure<string>(orderValidationResult.Errors);
				}

				if(request.Task.TaskType == EdoTaskType.SaveCode)
				{
					var checkOtherRequestsResult = CheckOtherRequests(uow, request, orderEdoTaskId);
					if(checkOtherRequestsResult.IsFailure)
					{
						return Result.Failure<string>(checkOtherRequestsResult.Errors);
					}

					var edoAccount = _counterpartyEdoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(
						request.Order.Client,
						request.Order.Contract.Organization.Id
					);

					if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
					{
						return Result.Failure<string>(new Error("CounterpartyDontAgreeEdoConsent",
							"Переотправка документа невозможна, т.к.у контрагента нет согласия на ЭДО")
						);
					}

					var newRequest = _manualEdoRequestFactory.Create(uow, request.Order);

					uow.Save(newRequest);
					uow.Commit();

					_bus.Publish(new EdoRequestCreatedEvent { Id = newRequest.Id });

					return Result.Success($"Документ отправлен на переформирование.");
				}

				//Если сюда попадет документ, то значит не правильно выбраны условия доступности действия
				//или не реализована отправка выбранного документами по правильным условиям
				return Result.Failure<string>(new Error("DocumentSendNotSupported",
					$"Для выбранного документа не реализована отправка. \n" +
					$"Обратитесь за технической поддержкой.")
				);
			}
		}

		public Result<string> TryResendReceiptDocument(int orderEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var request = _edoRequestRepository
					.GetFirstOrDefault(uow, x => x.Task.Id == orderEdoTaskId);

				var orderValidationResult = ValidateOrderForResend(request.Order);
				if(orderValidationResult.IsFailure)
				{
					return Result.Failure<string>(orderValidationResult.Errors);
				}

				var receiptTask = request.Task.As<ReceiptEdoTask>();
				if(receiptTask == null)
				{
					return Result.Failure<string>(new Error("DocumentIsNotReceipt",
						"Переотправка документа невозможна, т.к. текущий документ не является чеком")
					);
				}

				if(receiptTask.ReceiptStatus == EdoReceiptStatus.SavedToPool)
				{
					var checkOtherRequestsResult = CheckOtherRequests(uow, request, orderEdoTaskId);
					if(checkOtherRequestsResult.IsFailure)
					{
						return Result.Failure<string>(checkOtherRequestsResult.Errors);
					}

					var newRequest = _manualEdoRequestFactory.Create(uow, request.Order);

					uow.Save(newRequest);
					uow.Commit();

					_bus.Publish(new EdoRequestCreatedEvent { Id = newRequest.Id });

					return Result.Success($"Документ отправлен на переформирование.");
				}

				//Если сюда попадет документ, то значит не правильно выбраны условия доступности действия
				//или не реализована отправка выбранного документами по правильным условиям
				return Result.Failure<string>(new Error("DocumentSendNotSupported",
					$"Для выбранного документа не реализована отправка. \n" +
					$"Обратитесь за технической поддержкой.")
				);
			}
		}

		public Result RehandleNewUpdDocumentWithProblem(int updEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<DocumentEdoTask>(updEdoTaskId);
				if(task == null)
				{
					return Result.Failure(new Error("UpdEdoTaskNotFound",
						$"ЭДО задача №{updEdoTaskId} на отправку УПД не найдена, " +
						$"обратитесь в техподдержку"));
				}

				if(task.Status != EdoTaskStatus.Problem)
				{
					return Result.Failure(new Error("UpdEdoTaskDontHaveProblem",
						$"ЭДО задача №{updEdoTaskId} на отправку УПД не имеет нерешенных проблем для переобработки."
					));
				}

				if(task.Stage != DocumentEdoTaskStage.New)
				{
					return Result.Failure(new Error("UpdEdoTaskCantRehandleProblemInCurrentStage",
						$"Для ЭДО задачи №{updEdoTaskId} на отправку УПД " +
						$"в стадии {task.Stage.GetEnumTitle()} не доступна переобработка проблемы."
					));
				}

				_messageService.PublishTaskCreatedEvent(task)
					.GetAwaiter()
					.GetResult();

				return Result.Success();
			}
		}

		public Result<string> UpdateDocflowStatus(int taskId, Guid? docflowId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Обновление статуса документооборота из Taxcom"))
			{
				var edoTask = uow.Session.Get<OrderEdoTask>(taskId);
				if(edoTask is null)
				{
					return Result.Failure<string>(EdoErrors.NoEdoTask);
				}

				if(docflowId.HasValue is false)
				{
					return Result.Failure<string>(EdoErrors.NoTaxcomDocflow);
				}

				var order = GetOrderByTaskId(uow, taskId);
				if(order is null)
				{
					return Result.Failure<string>(OrderErrors.NotFound);
				}

				var taxcomDocflow = _edoRepository.GetTaxcomDocflowByDocflowId(uow, docflowId.Value);
				if(taxcomDocflow is null)
				{
					return Result.Failure<string>(EdoErrors.NoTaxcomDocflow);
				}

				return UpdateDocflowStatusAsync(uow, taxcomDocflow.DocflowId, order.Contract.Organization.Id)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();
			}
		}

		private async Task<IEnumerable<string>> WithNotAcceptedBuyerTitle(
			ITaxcomApiClient taxcomApiClient,
			Guid docflowId,
			IEnumerable<string> transactionCodes,
			CancellationToken cancellationToken)
		{
			var codes = transactionCodes ?? Enumerable.Empty<string>();
			if(!TaxcomDocflowStateByTransactions.ContainsBuyerTitle(codes))
			{
				return codes;
			}

			try
			{
				var raw = await taxcomApiClient.GetDocFlowRawData(docflowId.ToString(), cancellationToken);
				if(raw.IsFailure || raw.Value == null)
				{
					return codes;
				}

				return TaxcomDocflowStateByTransactions.WithBuyerTitleResult(
					codes,
					BuyerTitleAcceptance.ReadResultCode(raw.Value));
			}
			catch(Exception exception)
			{
				_logger.LogWarning(
					exception,
					"Не удалось прочитать итог титула покупателя по документообороту {DocflowId}",
					docflowId);
				return codes;
			}
		}

		public async Task<Result<string>> UpdateDocflowStatusAsync(
			IUnitOfWork uow,
			Guid? docflowId,
			int organizationId,
			CancellationToken cancellationToken = default)
		{
			if(docflowId.HasValue is false)
			{
				return Result.Failure<string>(new Error(
					"DocflowIdRequired",
					"ID документооборота не может быть пустым"
				));
			}

			if(organizationId <= 0)
			{
				return Result.Failure<string>(new Error(
					"OrganizationIdRequired",
					"ID организации должен быть больше 0"
				));
			}

			try
			{
				var organization = _organizationRepository.GetOrganizationById(organizationId);
				if(organization is null)
				{
					return Result.Failure<string>(new Error(
						"OrganizationNotFound",
						$"Организация с ID {organizationId} не найдена"
					));
				}

				if(organization.TaxcomEdoSettings is null)
				{
					return Result.Failure<string>(new Error(
						"TaxcomSettingsNotFound",
						$"Taxcom ЭДО настройки не найдены для организации {organization.Name}"
					));
				}

				var edoAccount = organization.TaxcomEdoSettings.EdoAccount;
				var taxcomApiClient = _taxcomApiFactory.Create(organizationId, edoAccount);

				var description = await taxcomApiClient.GetDocflowStatus(docflowId.ToString(), edoAccount);

				var mainDocument = description.DocFlow.Documents.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Definition.Identifiers.ExternalIdentifier))
					?? throw new InvalidOperationException("Не найден главный документ");

				if(mainDocument is null)
				{
					return Result.Failure<string>(new Error(
						"MainDocumentNotFound",
						"Главный документ не найден в документообороте"
					));
				}

				EdoDocFlowStatus? reportedStatus = null;
				if(Enum.TryParse(description.DocFlow.Status, true, out EdoDocFlowStatus parsedStatus))
				{
					reportedStatus = parsedStatus;
				}

				var transactionCodes = await WithNotAcceptedBuyerTitle(
					taxcomApiClient,
					description.DocFlow.Id,
					description.DocFlow.Documents?.Select(document => document.TransactionCode),
					cancellationToken);

				var state = TaxcomDocflowStateByTransactions.Resolve(
					reportedStatus,
					transactionCodes);
				var docflowUpdatedEvent = new OutgoingTaxcomDocflowUpdatedEvent
				{
					DocFlowId = description.DocFlow.Id,
					EdoAccount = edoAccount,
					MainDocumentId = mainDocument.Definition.Identifiers.ExternalIdentifier,
					Status = state.Status?.ToString() ?? description.DocFlow.Status,
					StatusChangeDateTime = description.DocFlow.StatusChangeDateTime,
					IsReceived = state.IsReceived
				};

				await _bus.Publish(docflowUpdatedEvent, cancellationToken);

				var statusTitle = state.Status?.GetEnumTitle();
				if(string.IsNullOrWhiteSpace(statusTitle))
				{
					statusTitle = string.IsNullOrWhiteSpace(description.DocFlow.Status)
						? "не определён"
						: description.DocFlow.Status;
				}

				return Result.Success(
					$"Обновленный статус: {statusTitle}{Environment.NewLine}Документооборот {docflowId} обновится в карточке в течение нескольких минут.");
			}
			catch(Exception ex)
			{
				return Result.Failure<string>(new Error(
					"UpdateDocflowStatusException",
					$"Ошибка при обновлении статуса: {ex.Message}"
				));
			}
		}

		public bool CanResendViaEdoRequestCreatedEvent(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Проверка возможности переотправки документа через событие OrderDocumentSendEvent"))
			{
				if(!_edoRepository.HasActiveProblemWithSource(uow, taskId, _resendViaOrderDocumentSendEventProblemSources))
				{
					return false;
				}

				return true;
			}
		}

		public async Task<Result<string>> TryResendViaOrderDocumentSendEventAsync(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Переотправка документа через событие отправки заказа OrderDocumentSendEvent в Такском"))
			{				
				var orderEdoDocument = _edoRepository.GetOrderEdoDocumentByTaskId(uow, taskId);

				if(orderEdoDocument is null)
				{
					return Result.Failure<string>(new Error(
						"OrderEdoDocumentNotFound",
						$"Не найден исходящий документ ЭДО для задачи №{taskId}"));
				}

				var message = new OrderDocumentSendEvent
				{
					OrderDocumentId = orderEdoDocument.Id
				};

				await _bus.Publish(message);

				return Result.Success("Документ поставлен в очередь на переотправку");
			}
		}
	}
}
