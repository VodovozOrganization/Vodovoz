using Core.Infrastructure;
using Edo.Admin;
using Edo.Contracts.Messages.Events;
using Edo.Transport;
using EdoService.Library.Factories;
using Gamma.Utilities;
using MassTransit;
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
using Taxcom.Docflow.Utility;
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
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly MessageService _messageService;
		private readonly IUserService _userService;
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

		public EdoService(
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IEdoRepository edoRepository,
			MessageService messageService,
			IUserService userService,
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
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
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

		public Result<string> ScheduleResendEdoDocumentAfterTrueMarkCancellation(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку после отмены вывода из оборота в ЧЗ"))
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

				var withdrawalTaskIds = GetWithdrawalTaskIdsForBaseTask(uow, taskId);
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

				var productCodes = TrueMarkProductCodeFactory.CreateAutoCodesFromCancelledTask(edoTask);
				var resendEdoRequest = _manualEdoRequestFactory.Create(uow, order, productCodes);
				var withdrawalDocumentResult = GetSuccessfulWithdrawalDocumentForTask(
					uow,
					taskId,
					order.Id,
					withdrawalTaskIds);
				if(withdrawalDocumentResult.IsFailure)
				{
					return Result.Failure<string>(withdrawalDocumentResult.Errors);
				}

				var withdrawalDocument = withdrawalDocumentResult.Value;

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

		public Result<string> ResendEdoDocumentForOrderWithCodesFromPool(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Переотправка документа ЭДО с кодами ЧЗ из пула"))
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

				var cancellationResendAlreadyExists = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.Any(x => x.OriginalEdoTask.Id == taskId);

				if(cancellationResendAlreadyExists)
				{
					return Result.Failure<string>(EdoErrors.TrueMarkCancellationResendAlreadyExists);
				}

				var withdrawalTaskIds = GetWithdrawalTaskIdsForBaseTask(uow, taskId);
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

				var request = _manualEdoRequestFactory.Create(uow, order);

				CancelEdoTaskWithReason(uow, edoTask);

				uow.Save(request);
				uow.Commit();

				_edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка документов ЭДО с кодами ЧЗ из пула")
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				return Result.Success("Документ отправлен на переотправку с подбором кодов ЧЗ из пула");
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

				_orderEdoTaskCreatedEventPublisher.Publish(edoTask)
					.GetAwaiter()
					.GetResult();

				return Result.Success("Задача успешно отправлена на повторную обработку");
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

		private void ResendDocumentForCancelledEdoTask(IUnitOfWork uow, OrderEntity order, OrderEdoTask edoTask)
		{
			var productCodes = TrueMarkProductCodeFactory.CreateAutoCodesFromCancelledTask(edoTask);
			var request = _manualEdoRequestFactory.Create(uow, order, productCodes);

			uow.Save(request);
			uow.Commit();

			_edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка документов ЭДО")
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
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

		private static int[] GetWithdrawalTaskIdsForBaseTask(IUnitOfWork uow, int taskId)
		{
			return uow.GetAll<WithdrawalEdoRequest>()
				.Where(x => x.BaseDocumentEdoTask.Id == taskId && x.Task != null)
				.Select(x => x.Task.Id)
				.ToArray();
		}

		private static Result<TrueMarkDocument> GetSuccessfulWithdrawalDocumentForTask(
			IUnitOfWork uow,
			int taskId,
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

			if(withdrawalDocuments.Length == 0)
			{
				// У документов, созданных до добавления связи с задачей вывода, доступна только привязка к заказу.
				withdrawalDocuments = uow.GetAll<TrueMarkDocument>()
					.Where(x => x.Order.Id == orderId
						&& x.WithdrawalEdoTask == null
						&& x.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal
						&& x.IsSuccess
						&& x.Guid != null)
					.ToArray();
			}

			if(withdrawalDocuments.Length == 0)
			{
				if(!withdrawalTaskIdsArray.Any())
				{
					return Result.Failure<TrueMarkDocument>(new Error(
						"WithdrawalRequestForTaskNotFound",
						$"Не найдена заявка на вывод кодов из оборота для задачи ЭДО {taskId}"));
				}

				return Result.Failure<TrueMarkDocument>(new Error(
					"WithdrawalDocumentForTaskNotFound",
					$"Не найден успешный документ вывода кодов из оборота для задачи ЭДО {taskId}"));
			}

			if(withdrawalDocuments.Length > 1)
			{
				return Result.Failure<TrueMarkDocument>(new Error(
					"WithdrawalDocumentForTaskNotUnique",
					$"Найдено несколько документов вывода кодов из оборота для задачи ЭДО {taskId}"));
			}

			return Result.Success(withdrawalDocuments.Single());
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
				var edoDocumentsActions = UpdateEdoDocumentAction(uow, entity, type);

				if(type is DocumentContainerType.Upd)
				{
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
				}

				uow.Save(edoDocumentsActions);
				uow.Commit();
			}
		}

		private OrderEdoTrueMarkDocumentsActions UpdateEdoDocumentAction(IUnitOfWork uow, IDomainObject entity, DocumentContainerType type)
		{
			var restriction = GetRestrictionByType(entity, type);

			var edoDocumentsAction = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(restriction)
					.FirstOrDefault() ?? new OrderEdoTrueMarkDocumentsActions();

			FillEdoDocumentsActionByType(edoDocumentsAction, entity, type);

			if(type is DocumentContainerType.Upd)
			{
				edoDocumentsAction.IsNeedToResendEdoUpd = true;
			}
			else
			{
				edoDocumentsAction.IsNeedToResendEdoBill = true;
			}

			edoDocumentsAction.Created = DateTime.Now;

			return edoDocumentsAction;
		}

		private void FillEdoDocumentsActionByType(OrderEdoTrueMarkDocumentsActions edoDocumentsAction, IDomainObject entity, DocumentContainerType type)
		{
			switch(type)
			{
				case DocumentContainerType.Bill:
				case DocumentContainerType.Upd:
					edoDocumentsAction.Order = (Order)entity;
					break;
				case DocumentContainerType.BillWSForDebt:
					edoDocumentsAction.OrderWithoutShipmentForDebt = (OrderWithoutShipmentForDebt)entity;
					break;
				case DocumentContainerType.BillWSForPayment:
					edoDocumentsAction.OrderWithoutShipmentForPayment = (OrderWithoutShipmentForPayment)entity;
					break;
				case DocumentContainerType.BillWSForAdvancePayment:
					edoDocumentsAction.OrderWithoutShipmentForAdvancePayment = (OrderWithoutShipmentForAdvancePayment)entity;
					break;
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
		}

		private Expression<Func<OrderEdoTrueMarkDocumentsActions, bool>> GetRestrictionByType(IDomainObject entity, DocumentContainerType type)
		{
			switch(type)
			{
				case DocumentContainerType.Bill:
				case DocumentContainerType.Upd:
					return x => x.Order.Id == entity.GetId();
				case DocumentContainerType.BillWSForDebt:
					return x => x.OrderWithoutShipmentForDebt.Id == entity.GetId();
				case DocumentContainerType.BillWSForPayment:
					return x => x.OrderWithoutShipmentForPayment.Id == entity.GetId();
				case DocumentContainerType.BillWSForAdvancePayment:
					return x => x.OrderWithoutShipmentForAdvancePayment.Id == entity.GetId();
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
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
			var containersToRevokeStatuses = new EdoDocFlowStatus[]
			{
				EdoDocFlowStatus.Succeed
			};

			var orderEdoContainers = _orderRepository
				.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Where(ooec => containersToRevokeStatuses.Contains(ooec.EdoDocFlowStatus));

			var restriction = GetRestrictionByType(order, DocumentContainerType.Bill);

			var edoDocumentsAction = unitOfWork.GetAll<OrderEdoTrueMarkDocumentsActions>()
				.Where(restriction)
				.FirstOrDefault() ?? new OrderEdoTrueMarkDocumentsActions();

			FillEdoDocumentsActionByType(edoDocumentsAction, order, DocumentContainerType.Bill);

			edoDocumentsAction.IsNeedOfferCancellation = true;

			unitOfWork.Save(edoDocumentsAction);

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

		public void ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type)
		{
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

		private void CancelEdoTaskWithReason(IUnitOfWork uow, EdoTask edoTask)
		{
			var cancellationReason = $"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}";
			_edoCancellationService.CancelTask(edoTask.Id, cancellationReason, true, uow: uow).GetAwaiter().GetResult();
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

				var docflowUpdatedEvent = new OutgoingTaxcomDocflowUpdatedEvent
				{
					DocFlowId = description.DocFlow.Id,
					EdoAccount = edoAccount,
					MainDocumentId = mainDocument.Definition.Identifiers.ExternalIdentifier,
					Status = description.DocFlow.Status,
					StatusChangeDateTime = description.DocFlow.StatusChangeDateTime,
				};

				var recievedStatuses = _edoRepository.GetRecievedEdoDocFlowStatuses();
				docflowUpdatedEvent.IsReceived = recievedStatuses.Contains(docflowUpdatedEvent.Status.TryParseAsEnum<EdoDocFlowStatus>().Value);

				await _bus.Publish(docflowUpdatedEvent, cancellationToken);

				return Result.Success($"Статус документооборота {docflowId} обновится в течение нескольких минут. Обновленный статус: {docflowUpdatedEvent.Status}");
			}
			catch(Exception ex)
			{
				return Result.Failure<string>(new Error(
					"UpdateDocflowStatusException",
					$"Ошибка при обновлении статуса: {ex.Message}"
				));
			}
		}
	}
}
