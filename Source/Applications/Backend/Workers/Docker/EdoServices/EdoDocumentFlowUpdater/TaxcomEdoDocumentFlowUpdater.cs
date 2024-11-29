using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdoDocumentFlowUpdater.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Services;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings;
using Vodovoz.Zabbix.Sender;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace EdoDocumentFlowUpdater
{
	public class TaxcomEdoDocumentFlowUpdater : BackgroundService
	{
		private readonly ILogger<TaxcomEdoDocumentFlowUpdater> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TaxcomEdoDocumentFlowUpdaterOptions _documentFlowUpdaterOptions;
		private readonly ISettingsController _settingController;
		private readonly IZabbixSender _zabbixSender;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoContainerFileStorageService _edoContainerFileStorageService;

		private long? _lastEventOutgoingDocumentsTimeStamp;

		public TaxcomEdoDocumentFlowUpdater(
			ILogger<TaxcomEdoDocumentFlowUpdater> logger,
			IUserService userService,
			IOptions<TaxcomEdoDocumentFlowUpdaterOptions> documentFlowUpdaterOptions,
			IServiceScopeFactory serviceScopeFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ISettingsController settingController,
			IZabbixSender zabbixSender,
			IEdoContainerFileStorageService edoContainerFileStorageService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_documentFlowUpdaterOptions =
				(documentFlowUpdaterOptions ?? throw new ArgumentNullException(nameof(documentFlowUpdaterOptions))).Value;
			_settingController = settingController ?? throw new ArgumentNullException(nameof(settingController));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_edoContainerFileStorageService =
				edoContainerFileStorageService ?? throw new ArgumentNullException(nameof(edoContainerFileStorageService));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Процесс электронного документооборота запущен");
			_lastEventOutgoingDocumentsTimeStamp = _settingController.GetValue<long>("last_event_outgoing_documents_timestamp");
			await StartWorkingAsync(cancellationToken);
		}

		private async Task StartWorkingAsync(CancellationToken cancellationToken)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				try
				{
					await DelayAsync(cancellationToken);
					await ProcessOutgoingDocuments(cancellationToken);
					await CancellationDocFlows(cancellationToken);

					await _zabbixSender.SendIsHealthyAsync(cancellationToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при запуске обновления статусов документооборотов");
				}
			}
		}

		private async Task ProcessOutgoingDocuments(CancellationToken cancellationToken)
		{
			try
			{
				EdoDocFlowUpdates docFlowUpdates;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки исходящих документов"))
				{
					do
					{
						_logger.LogInformation("Получаем исходящие документы");

						using var scope = _serviceScopeFactory.CreateScope();
						var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();

						docFlowUpdates =
							await taxcomApiClient.GetDocFlowsUpdates(
								new GetDocFlowsUpdatesParameters
								{
									DocFlowStatus = null,
									LastEventTimeStamp = _lastEventOutgoingDocumentsTimeStamp,
									DocFlowDirection = "Outgoing",
									DepartmentId = null,
									IncludeTransportInfo = true
								},
								cancellationToken);

						if(docFlowUpdates.Updates is null)
						{
							return;
						}

						_logger.LogInformation("Обрабатываем полученные контейнеры {DocFlowUpdatesCount}", docFlowUpdates.Updates.Count());

						foreach(var item in docFlowUpdates.Updates)
						{
							EdoContainer container = null;
							EdoDocFlowDocument mainDocument = null;

							if(item.Documents.Any())
							{
								mainDocument = item.Documents.First();
								container = _orderRepository.GetEdoContainerByMainDocumentId(uow, mainDocument.ExternalIdentifier);
							}

							if(container != null)
							{
								var containerReceived =
									item.Documents.FirstOrDefault(x => x.TransactionCode == "PostDateConfirmation") != null;

								container.DocFlowId = item.Id;
								container.Received = containerReceived;
								container.InternalId = mainDocument.InternalId;
								container.ErrorDescription = item.ErrorDescription;
								container.EdoDocFlowStatus = Enum.Parse<EdoDocFlowStatus>(item.Status.ToString());

								if(container.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
								{
									var containerRawData =
										await taxcomApiClient.GetDocFlowRawData(item.Id.Value.ToString(), cancellationToken);

									using var ms = new MemoryStream(containerRawData.ToArray());

									var result =
										await _edoContainerFileStorageService.UpdateContainerAsync(container, ms, cancellationToken);

									if(result.IsFailure)
									{
										var errors = string.Join(", ", result.Errors.Select(e => e.Message));

										_logger.LogError("Не удалось обновить контейнер, ошибка: {Errors}", errors);
									}
								}

								_logger.LogInformation("Сохраняем изменения контейнера по заказу №{OrderId}", container.Order?.Id);
								await uow.SaveAsync(container);
								await uow.CommitAsync();
							}

							_lastEventOutgoingDocumentsTimeStamp = item.StatusChangeDateTime.ToBinary();
						}
					} while(!docFlowUpdates.IsLast);
				}
			}
			catch(Exception e)
			{
				const string errorMessage = "Ошибка в процессе обработки исходящих документов";
				_logger.LogError(e, errorMessage);
			}
			finally
			{
				_settingController.CreateOrUpdateSetting(
					"last_event_outgoing_documents_timestamp", _lastEventOutgoingDocumentsTimeStamp.ToString());
			}
		}

		private async Task CancellationDocFlows(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы по которым нужно отменить счёт");

			try
			{
				using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис аннулирования документов");
				var offerCancellationFromActions = uow
					.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(x => x.IsNeedOfferCancellation)
					.ToList();
			
				_logger.LogInformation("Всего нужно аннулировать {OfferCancellationCount}", offerCancellationFromActions.Count);

				foreach(var offerCancellation in offerCancellationFromActions)
				{
					using var scope = _serviceScopeFactory.CreateScope();
					var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
				
					var containersToOfferCancellation = uow.Session.Query<EdoContainer>()
						.Where(ec => ec.Order.Id == offerCancellation.Order.Id
							&& ec.Type == Type.Bill
							&& ec.EdoDocFlowStatus != EdoDocFlowStatus.Warning
							&& ec.EdoDocFlowStatus != EdoDocFlowStatus.Error
							&& ec.EdoDocFlowStatus != EdoDocFlowStatus.WaitingForCancellation
							&& ec.EdoDocFlowStatus != EdoDocFlowStatus.Cancelled)
						.ToList();

					foreach(var container in containersToOfferCancellation)
					{
						//Не отменяем контейнеры, которые были созданы после запроса на аннулирование
						if(offerCancellation.Created.HasValue && offerCancellation.Created < container.Created)
						{
							continue;
						}
					
						_logger.LogInformation("Отменяется оффер из контейнера №{EdoContainerId}, Заказа №{OrderId}, документооборота {DocFlowId}",
							container.Id,
							container.Order?.Id,
							container.DocFlowId);
					
						await SendOfferCancellationContainer(taxcomApiClient, container, cancellationToken);
					}

					offerCancellation.IsNeedOfferCancellation = false;
					await uow.SaveAsync(offerCancellation);
					await uow.CommitAsync();
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе аннулирования документов");
			}
		}
		
		private async Task SendOfferCancellationContainer(
			ITaxcomApiClient taxcomApiClient, EdoContainer edoContainer, CancellationToken cancellationToken)
		{
			try
			{
				await taxcomApiClient.SendOfferCancellation(
					edoContainer.DocFlowId.ToString(), "Состав заказа был изменен", cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе аннулирования документооборота {DocFlowId} из контейнера №{EdoContainerId}, Заказа №{OrderId}",
					edoContainer.DocFlowId,
					edoContainer.Id,
					edoContainer.Order.Id);
			}
		}

		private async Task DelayAsync(CancellationToken cancellationToken)
		{
			var delay = _documentFlowUpdaterOptions.DelayBetweenDocumentFlowProcessingInSeconds;
			
			_logger.LogInformation("Ждем {Delay}сек", delay);
			await Task.Delay(delay * 1000, cancellationToken);
		}
	}
}
