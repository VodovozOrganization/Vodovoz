using System;
using System.Threading;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusUpdater : BackgroundService
	{
		private readonly ILogger<FastPaymentStatusUpdater> _logger;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentManager _fastPaymentManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private bool isFirstLaunch = true;

		public FastPaymentStatusUpdater(
			ILogger<FastPaymentStatusUpdater> logger,
			IFastPaymentRepository fastPaymentRepository,
			IFastPaymentManager fastPaymentManager,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentManager = fastPaymentManager ?? throw new ArgumentNullException(nameof(fastPaymentManager));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс обновления статуса обрабатывающихся быстрых платежей запущен");
			while(!stoppingToken.IsCancellationRequested)
			{
				if(isFirstLaunch)
				{
					_logger.LogInformation("Ждем 90сек. Первый запуск...");
					await Task.Delay(90000, stoppingToken);
				}
				else
				{
					_logger.LogInformation("Ждем 25сек");
					await Task.Delay(25000, stoppingToken);
				}

				try
				{
					_logger.LogInformation($"Обновление статуса обрабатывающихся платежей...");
					var count = 0;

					using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var processingFastPayments = _fastPaymentRepository.GetAllProcessingFastPayments(uow);

						using(var scope = _serviceScopeFactory.CreateScope())
						{
							var orderRequestManager = scope.ServiceProvider.GetRequiredService<IOrderRequestManager>();
							foreach(var payment in processingFastPayments)
							{
								var response = await orderRequestManager.GetOrderInfo(payment.Ticket);

								if((int)response.Status == (int)payment.FastPaymentStatus)
								{
									if(!_fastPaymentManager.IsTimeToCancelPayment(
											payment.CreationDate, !string.IsNullOrWhiteSpace(payment.QRPngBase64)))
									{
										continue;
									}

									var cancelPaymentResponse = await orderRequestManager.CancelPayment(payment.Ticket);

									if(cancelPaymentResponse.ResponseCode != 0)
									{
										_logger.LogError(
											$"Не удалось отменить сессию оплаты {payment.Ticket}. Код ответа: {cancelPaymentResponse.ResponseCode}");
										continue;
									}

									_logger.LogInformation($"Отменяем платеж с сессией: {payment.Ticket}");
									_fastPaymentManager.UpdateFastPaymentStatus(uow, payment, FastPaymentDTOStatus.Rejected, DateTime.Now);
								}
								else
								{
									var newStatus = response.Status;
									_logger.LogInformation(
										$"Обновляем статус платежа с сессией: {payment.Ticket} новый статус: {newStatus}");
									_fastPaymentManager.UpdateFastPaymentStatus(uow, payment, newStatus, response.StatusDate);
								}

								uow.Save(payment);
								uow.Commit();
								count++;
							}
						}
					}

					_logger.LogInformation(count > 0
						? $"{count} платежей поменяли свой статус"
						: "Не обнаружено обрабатывающихся платежей");
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при обновлении статуса обрабатывающихся платежей");
				}
				finally
				{
					isFirstLaunch = false;
				}
			}
		}
	}
}
