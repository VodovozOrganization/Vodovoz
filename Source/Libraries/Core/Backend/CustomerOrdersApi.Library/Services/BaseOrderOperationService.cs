using CustomerOrdersApi.Library.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;

namespace CustomerOrdersApi.Library.Services
{
	public abstract class BaseOrderOperationService<TDto, TResult>
		where TDto : OrderOperationDto
		where TResult : OrderOperationResult, new()
	{
		protected readonly IUnitOfWorkFactory _unitOfWorkFactory;
		protected readonly ILogger _logger;
		protected readonly IOnlineOrderRepository _onlineOrderRepository;
		protected readonly IOrderRepository _orderRepository;
		protected readonly IRouteListItemRepository _routeListItemRepository;
		protected readonly IEmployeeRepository _employeeRepository;
		protected readonly ISubdivisionRepository _subdivisionRepository;
		protected readonly IRouteListService _routeListService;
		protected readonly INomenclatureSettings _nomenclatureSettings;
		protected readonly ICallTaskWorker _callTaskWorker;

		protected static readonly OrderStatus[] SimpleStatuses = new[]
		{
			OrderStatus.NewOrder,
			OrderStatus.WaitForPayment,
			OrderStatus.Accepted
		};

		protected static readonly OrderStatus[] ComplexStatuses = new[]
		{
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay
		};

		protected static readonly OrderStatus[] AllowedStatuses =
			SimpleStatuses.Concat(ComplexStatuses).ToArray();

		protected BaseOrderOperationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger logger,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
		}

		/// <summary>
		/// Название операции
		/// </summary>
		protected abstract string OperationName { get; }

		/// <summary>
		/// Простой перенос заказа (статусы: Новый, Ожидание оплаты, Принят)
		/// </summary>
		protected abstract Task<TResult> ProcessSimpleOperationAsync(IUnitOfWork uow, Order order, OnlineOrder onlineOrder, TDto dto);

		/// <summary>
		/// Сложный перенос заказа (статусы: В маршрутном листе, На погрузке, В пути)
		/// </summary>
		protected abstract Task<TResult> ProcessComplexOperationAsync(IUnitOfWork uow, Order order, OnlineOrder onlineOrder, TDto dto);

		/// <summary>
		/// Проверка специфичных для сервиса параметров и условий, которые не были покрыты базовой валидацией
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="order"></param>
		/// <param name="onlineOrder"></param>
		/// <param name="dto"></param>
		/// <returns></returns>
		protected abstract Task<OperationValidationResult> ValidateSpecificAsync(IUnitOfWork uow, Order order, OnlineOrder onlineOrder, TDto dto);

		protected abstract string GetSuccessMessage();

		protected virtual bool IsStatusAllowed(OrderStatus status)
		{
			return AllowedStatuses.Contains(status);
		}

		/// <summary>
		/// Оплачен ли заказ онлайн
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		protected static bool IsPaidOnline(Order order) => order.PaymentType is PaymentType.PaidOnline;

		public async Task<TResult> ExecuteAsync(TDto dto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();

			try
			{
				_logger.LogInformation(
					"Начало {OperationName} заказа: ExternalOrderId: {ExternalOrderId}",
					OperationName,
					dto.ExternalOrderId);

				var onlineOrder = await FindOnlineOrderAsync(uow, dto.ExternalOrderId);
				if(onlineOrder == null)
				{
					return CreateNotFoundResult(dto.ExternalOrderId);
				}

				var order = GetActiveOrder(onlineOrder);
				if(order == null)
				{
					return CreateNoActiveOrderResult();
				}

				var statusValidation = ValidateOperationByStatus(order);
				if(!statusValidation.canOperate)
				{
					return CreateStatusErrorResult(statusValidation);
				}

				var specificValidation = await ValidateSpecificAsync(uow, order, onlineOrder, dto);
				if(!specificValidation.IsValid)
				{
					return CreateValidationErrorResult(specificValidation);
				}

				if(SimpleStatuses.Contains(order.OrderStatus))
				{
					return await ProcessSimpleOperationAsync(uow, order, onlineOrder, dto);
				}
				else if(ComplexStatuses.Contains(order.OrderStatus))
				{
					return await ProcessComplexOperationAsync(uow, order, onlineOrder, dto);
				}
				else
				{
					return CreateInvalidStatusResult(order);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при {OperationName} заказа {ExternalOrderId}",
					OperationName.ToLower(),
					dto.ExternalOrderId);

				return CreateErrorResult(ex);
			}
		}

		private async Task<OnlineOrder> FindOnlineOrderAsync(IUnitOfWork uow, Guid externalOrderId)
		{
			return _onlineOrderRepository.GetOnlineOrderByExternalId(uow, externalOrderId);
		}

		private (bool canOperate, bool requiresComplexHandling, string errorMessage) ValidateOperationByStatus(Order order)
		{
			if(!IsStatusAllowed(order.OrderStatus))
			{
				return (
					false,
					false,
					$"Невозможно выполнить операцию для заказа в статусе '{order.OrderStatus.GetEnumTitle()}'"
				);
			}

			return (
				true,
				ComplexStatuses.Contains(order.OrderStatus),
				null
			);
		}

		private Order GetActiveOrder(OnlineOrder onlineOrder)
		{
			var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();
			return onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));
		}

		private TResult CreateNotFoundResult(Guid externalOrderId)
		{
			_logger.LogWarning(
				"Попытка {OperationName} несуществующего заказа: ExternalOrderId: {ExternalOrderId}",
				OperationName,
				externalOrderId);

			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 404,
				Title = "One or more validation errors occurred",
				DetailMessage = "Заказ не найден"
			};
			return result;
		}

		private static TResult CreateValidationErrorResult(OperationValidationResult validationResult)
		{
			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 400,
				Title = "One or more validation errors occurred",
				DetailMessage = validationResult.ErrorMessage
			};
			return result;
		}

		private static TResult CreateNoActiveOrderResult()
		{
			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 400,
				Title = "One or more validation errors occurred",
				DetailMessage = "Онлайн заказ не имеет активного привязанного заказа"
			};
			return result;
		}

		private TResult CreateStatusErrorResult((bool canOperate, bool requiresComplexHandling, string errorMessage) statusValidation)
		{
			_logger.LogWarning(
				"{OperationName} заказа невозможна, причина: {Reason}",
				OperationName,
				statusValidation.errorMessage);

			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 408,
				Title = "One or more validation errors occurred",
				DetailMessage = statusValidation.errorMessage
			};
			return result;
		}

		private static TResult CreateInvalidStatusResult(Order order)
		{
			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 408,
				Title = "One or more validation errors occurred",
				DetailMessage = $"Невозможно выполнить операцию для заказа в статусе '{order.OrderStatus.GetEnumTitle()}'"
			};
			return result;
		}

		private static TResult CreateErrorResult(Exception ex)
		{
			var result = new TResult
			{
				IsSuccess = false,
				StatusCode = 500,
				Title = "One or more validation errors occurred",
				DetailMessage = "Произошла ошибка, пожалуйста, попробуйте позже"
			};
			return result;
		}

		protected TResult CreateSuccessResult(string message)
		{
			var result = new TResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = message
			};
			return result;
		}
	}
}
