using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusUpdater : BackgroundService
	{
		private readonly ILogger<FastPaymentStatusUpdater> _logger;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentManager _fastPaymentManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IErrorHandler _errorHandler;
		private bool _isFirstLaunch = true;
		private int _updatedCount;

		public FastPaymentStatusUpdater(
			ILogger<FastPaymentStatusUpdater> logger,
			IFastPaymentRepository fastPaymentRepository,
			IFastPaymentManager fastPaymentManager,
			IServiceScopeFactory serviceScopeFactory,
			IErrorHandler errorHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentManager = fastPaymentManager ?? throw new ArgumentNullException(nameof(fastPaymentManager));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс обновления статуса обрабатывающихся быстрых платежей запущен");
			await StartWorkingAsync(stoppingToken);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);

				try
				{
					_logger.LogInformation($"Обновление статуса обрабатывающихся платежей...");

					using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var processingFastPayments = _fastPaymentRepository.GetAllProcessingFastPayments(uow);

						using(var scope = _serviceScopeFactory.CreateScope())
						{
							await UpdateFastPaymentStatusAsync(processingFastPayments, scope, uow);
						}
					}

					_logger.LogInformation(_updatedCount > 0
						? $"{_updatedCount} платежей поменяли свой статус"
						: "Не обнаружено обрабатывающихся платежей");
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при обновлении статуса обрабатывающихся платежей");
				}
				finally
				{
					_isFirstLaunch = false;
					_updatedCount = 0;
				}
			}
		}

		private async Task UpdateFastPaymentStatusAsync(
			IEnumerable<FastPayment> processingFastPayments,
			IServiceScope scope,
			IUnitOfWork uow)
		{
			var orderRequestManager = scope.ServiceProvider.GetRequiredService<IOrderRequestManager>();
			var fastPaymentStatusChangeNotifier = scope.ServiceProvider.GetRequiredService<IFastPaymentStatusChangeNotifier>();

			foreach(var payment in processingFastPayments)
			{
				var ticket = payment.Ticket;
				var response = await orderRequestManager.GetOrderInfo(ticket, payment.Organization);
				if(response.ResponseCode != 0)
				{
					_errorHandler.LogErrorMessageFromUpdateOrderInfo(response, ticket, _logger);
					continue;
				}
				if((int)response.Status == (int)payment.FastPaymentStatus)
				{
					var fastPaymentWithQRNotFromOnline = payment.FastPaymentPayType == FastPaymentPayType.ByQrCode && !payment.OnlineOrderId.HasValue;
					var fastPaymentFromOnline = payment.OnlineOrderId.HasValue;
					if(!_fastPaymentManager.IsTimeToCancelPayment(payment.CreationDate, fastPaymentWithQRNotFromOnline, fastPaymentFromOnline))
					{
						continue;
					}

					_logger.LogInformation($"Отменяем платеж с сессией: {ticket}");
					_fastPaymentManager.UpdateFastPaymentStatus(uow, payment, FastPaymentDTOStatus.Rejected, DateTime.Now);
				}
				else
				{
					var newStatus = response.Status;
					_logger.LogInformation(
						$"Обновляем статус платежа с сессией: {ticket} новый статус: {newStatus}");
					_fastPaymentManager.UpdateFastPaymentStatus(uow, payment, newStatus, response.StatusDate);
				}

				uow.Save(payment);
				uow.Commit();
				_updatedCount++;
				NotifyFastPaymentStatusChange(fastPaymentStatusChangeNotifier, payment);
			}
		}

		private async Task DelayAsync(CancellationToken stoppingToken)
		{
			if(_isFirstLaunch)
			{
				_logger.LogInformation("Ждем 90сек. Первый запуск...");
				await Task.Delay(90000, stoppingToken);
			}
			else
			{
				_logger.LogInformation("Ждем 25сек");
				await Task.Delay(25000, stoppingToken);
			}
		}

		private void NotifyFastPaymentStatusChange(IFastPaymentStatusChangeNotifier notifier, FastPayment payment)
		{
			switch(payment.FastPaymentStatus)
			{
				case FastPaymentStatus.Rejected:
				case FastPaymentStatus.Performed:
					notifier.NotifyVodovozSite(payment.OnlineOrderId, payment.PaymentByCardFrom.Id, payment.Amount, false);
					notifier.NotifyMobileApp(payment.OnlineOrderId, payment.PaymentByCardFrom.Id, payment.Amount, false);
					break;
			}
		}
	}
}
