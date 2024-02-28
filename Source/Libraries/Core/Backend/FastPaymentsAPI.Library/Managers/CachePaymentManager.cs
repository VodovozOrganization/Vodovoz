using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsAPI.Library.Factories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;

namespace FastPaymentsAPI.Library.Managers
{
	public class CachePaymentManager : BackgroundService
	{
		private readonly ILogger<CachePaymentManager> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FastPaymentFileCache _fastPaymentFileCache;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IFastPaymentFactory _fastPaymentApiFactory;

		public CachePaymentManager(
			ILogger<CachePaymentManager> logger,
			IUnitOfWorkFactory uowFactory,
			FastPaymentFileCache fastPaymentFileCache,
			IFastPaymentRepository fastPaymentRepository,
			IOrderRepository orderRepository,
			IFastPaymentFactory fastPaymentApiFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_fastPaymentFileCache = fastPaymentFileCache ?? throw new ArgumentNullException(nameof(fastPaymentFileCache));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс синхронизации платежей из кэша запущен");
			while(!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(30000, stoppingToken);

				try
				{
					_logger.LogInformation("Поиск данных платежей в кэше...");

					var paymentsInCache = _fastPaymentFileCache.GetAllPaymentCaches();

					var message = $"Найдено {paymentsInCache.Count} данных платежей.";
					if(paymentsInCache.Count > 0)
					{
						_logger.LogInformation(message + " Синхронизация...");
					}
					else
					{
						_logger.LogInformation(message);
						continue;
					}

					IList<FastPaymentDTO> cachesToRemove = new List<FastPaymentDTO>();
					int savedPayments = 0;

					using(var uow = _uowFactory.CreateWithoutRoot())
					{
						foreach(var paymentDto in paymentsInCache)
						{
							var fastPayment = _fastPaymentRepository.GetFastPaymentByTicket(uow, paymentDto.Ticket);

							if(fastPayment == null)
							{
								var order = _orderRepository.GetOrder(uow, paymentDto.OrderId);
								var newPayment = _fastPaymentApiFactory.GetFastPayment(order, paymentDto);
								newPayment.SetProcessingStatus();
								uow.Save(newPayment);
								uow.Commit();
							}

							cachesToRemove.Add(paymentDto);
							savedPayments++;
						}

						if(savedPayments > 0)
						{
							_fastPaymentFileCache.RemovePaymentCaches(cachesToRemove);
						}
					}

					_logger.LogInformation($"Обработано {savedPayments} платежей");
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при синхронизации платежей из кэша");
				}
			}
		}
	}
}
