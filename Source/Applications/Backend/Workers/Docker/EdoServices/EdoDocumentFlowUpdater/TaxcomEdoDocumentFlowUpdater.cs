using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdoDocumentFlowUpdater.Configs;
using EdoDocumentFlowUpdater.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings;
using VodovozHealthCheck.Dto;

namespace EdoDocumentFlowUpdater
{
	public class TaxcomEdoDocumentFlowUpdater : BackgroundService
	{
		private readonly ILogger<TaxcomEdoDocumentFlowUpdater> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TaxcomEdoDocumentFlowUpdaterOptions _documentFlowUpdaterOptions;
		private readonly ISettingsController _settingController;
		private readonly TaxcomEdoDocFlowUpdaterHealthCheck _taxcomEdoDocFlowUpdaterHealthCheck;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoContainerFileStorageService _edoContainerFileStorageService;

		private long? _lastEventOutgoingDocumentsTimeStamp;

		public TaxcomEdoDocumentFlowUpdater(
			ILogger<TaxcomEdoDocumentFlowUpdater> logger,
			IOptions<TaxcomEdoDocumentFlowUpdaterOptions> documentFlowUpdaterOptions,
			IServiceScopeFactory serviceScopeFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			ISettingsController settingController,
			TaxcomEdoDocFlowUpdaterHealthCheck taxcomEdoDocFlowUpdaterHealthCheck,
			IEdoContainerFileStorageService edoContainerFileStorageService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_documentFlowUpdaterOptions =
				(documentFlowUpdaterOptions ?? throw new ArgumentNullException(nameof(documentFlowUpdaterOptions))).Value;
			_settingController = settingController ?? throw new ArgumentNullException(nameof(settingController));
			_taxcomEdoDocFlowUpdaterHealthCheck =
				taxcomEdoDocFlowUpdaterHealthCheck ?? throw new ArgumentNullException(nameof(taxcomEdoDocFlowUpdaterHealthCheck));
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
			_taxcomEdoDocFlowUpdaterHealthCheck.HealthResult = new VodovozHealthResultDto { IsHealthy = true };

			while(!cancellationToken.IsCancellationRequested)
			{
				await DelayAsync(cancellationToken);
				await ProcessOutgoingDocuments(cancellationToken);
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
				_taxcomEdoDocFlowUpdaterHealthCheck.HealthResult.IsHealthy = false;
				_taxcomEdoDocFlowUpdaterHealthCheck.HealthResult.AdditionalUnhealthyResults.Add($"Ошибка в процессе обработки исходящих документов: {e.Message}");

				_logger.LogError(e, "Ошибка в процессе обработки исходящих документов");
			}
			finally
			{
				_settingController.CreateOrUpdateSetting(
					"last_event_outgoing_documents_timestamp", _lastEventOutgoingDocumentsTimeStamp.ToString());
			}
		}

		private async Task DelayAsync(CancellationToken cancellationToken)
		{
			var delay = _documentFlowUpdaterOptions.DelayBetweenDocumentFlowProcessingInSeconds;
			
			_logger.LogInformation("Ждем {DelaySec}сек", delay);
			await Task.Delay(delay * 1000, cancellationToken);
		}
	}
}
