using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Entity.DocFlow;
using TaxcomEdo.Contracts;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.HealthChecks;
using Vodovoz.Settings;
using VodovozHealthCheck.Dto;

namespace TaxcomEdoApi.Services
{
	public class DocumentFlowService : BackgroundService
	{
		private readonly ILogger<DocumentFlowService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly ISettingsController _settingController;
		private readonly TaxcomEdoApiHealthCheck _taxcomEdoApiHealthCheck;
		private const int _delaySec = 90;

		private long? _lastEventOutgoingDocumentsTimeStamp;

		public DocumentFlowService(
			ILogger<DocumentFlowService> logger,
			TaxcomApi taxcomApi,
			ISettingsController settingController,
			TaxcomEdoApiHealthCheck taxcomEdoApiHealthCheck)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_settingController = settingController ?? throw new ArgumentNullException(nameof(settingController));
			_taxcomEdoApiHealthCheck = taxcomEdoApiHealthCheck ?? throw new ArgumentNullException(nameof(taxcomEdoApiHealthCheck));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс электронного документооборота запущен");
			_lastEventOutgoingDocumentsTimeStamp = _settingController.GetValue<long>("last_event_outgoing_documents_timestamp");
			await StartWorkingAsync(stoppingToken);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken)
		{
			_taxcomEdoApiHealthCheck.HealthResult = new VodovozHealthResultDto { IsHealthy = true };

			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);
				await ProcessOutgoingDocuments();
			}
		}

		private Task ProcessOutgoingDocuments()
		{
			try
			{
				IDocFlowUpdates docFlowUpdates;
				do
				{
					_logger.LogInformation("Получаем исходящие документы");

					docFlowUpdates =
						_taxcomApi.GetDocflowsUpdates(null, _lastEventOutgoingDocumentsTimeStamp, DocFlowDirection.Outgoing, null, true);

					if(docFlowUpdates.Updates is null)
					{
						return Task.CompletedTask;
					}

					_logger.LogInformation("Обрабатываем полученные контейнеры {DocFlowUpdatesCount}", docFlowUpdates.Updates.Count);

					foreach(var item in docFlowUpdates.Updates)
					{
						if(!item.Documents.Any())
						{
							continue;
						}
						
						var containerReceived =
							item.Documents.FirstOrDefault(x => x.TransactionCode == "PostDateConfirmation") != null;
						var firstDocument = item.Documents[0];

						var containerInfo = new EdoContainerInfo
						{
							MainDocumentId = firstDocument.ExternalIdentifier,
							DocFlowId = item.Id,
							Received = containerReceived,
							InternalId = firstDocument.InternalId,
							ErrorDescription = item.ErrorDescription,
							EdoDocFlowStatus = item.Status.ToString()
						};

						if(containerInfo.EdoDocFlowStatus == "Succeed")
						{
							containerInfo.Documents = _taxcomApi.GetDocflowRawData(item.Id.Value.ToString());
						}

						_logger.LogInformation(
							"Отправляем в очередь информацию о контейнере c документом {MainDocumentId}",
							containerInfo.MainDocumentId);
						
						//отправляем в очередь

						_lastEventOutgoingDocumentsTimeStamp = item.StatusChangeDateTime.ToBinary();
					}
				} while(!docFlowUpdates.IsLast);
			}
			catch(Exception e)
			{
				_taxcomEdoApiHealthCheck.HealthResult.IsHealthy = false;
				_taxcomEdoApiHealthCheck.HealthResult.AdditionalUnhealthyResults.Add($"Ошибка в процессе обработки исходящих документов: {e.Message}");

				_logger.LogError(e, "Ошибка в процессе обработки исходящих документов");
			}
			finally
			{
				_settingController.CreateOrUpdateSetting(
					"last_event_outgoing_documents_timestamp", _lastEventOutgoingDocumentsTimeStamp.ToString());
			}

			return Task.CompletedTask;
		}

		private async Task DelayAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Ждем {DelaySec}сек", _delaySec);
			await Task.Delay(_delaySec * 1000, stoppingToken);
		}
	}
}
