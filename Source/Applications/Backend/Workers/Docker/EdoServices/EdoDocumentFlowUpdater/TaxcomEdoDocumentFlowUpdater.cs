using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using EdoDocumentFlowUpdater.Configs;
using MassTransit;
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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Edo;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Zabbix.Sender;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace EdoDocumentFlowUpdater
{
	public class TaxcomEdoDocumentFlowUpdater : BackgroundService
	{
		private const string _postDateConfirmation = "PostDateConfirmation";
		private const string _trueMarkAccepted = "TracingAccepted";
		private const string _trueMarkRejected = "TracingRejected";
		private const string _trueMarkCancellationAccepted = "TracingCancellationAccepted";
		private const string _trueMarkCancellationRejected = "TracingCancellationRejected";

		private readonly ILogger<TaxcomEdoDocumentFlowUpdater> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly TaxcomEdoDocumentFlowUpdaterOptions _documentFlowUpdaterOptions;
		private readonly ISettingsController _settingController;
		private readonly IZabbixSender _zabbixSender;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly ITaxcomEdoDocflowLastProcessTimeRepository _edoDocflowLastProcessTimeRepository;
		private readonly IEdoContainerFileStorageService _edoContainerFileStorageService;
		private readonly IPublishEndpoint _publishEndpoint;

		private TaxcomEdoDocflowLastProcessTime _lastEventsProcessTime;

		public TaxcomEdoDocumentFlowUpdater(
			ILogger<TaxcomEdoDocumentFlowUpdater> logger,
			IUserService userService,
			IOptions<TaxcomEdoDocumentFlowUpdaterOptions> documentFlowUpdaterOptions,
			IServiceScopeFactory serviceScopeFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			ITaxcomEdoDocflowLastProcessTimeRepository edoDocflowLastProcessTimeRepository,
			ISettingsController settingController,
			IZabbixSender zabbixSender,
			IEdoContainerFileStorageService edoContainerFileStorageService,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_documentFlowUpdaterOptions =
				(documentFlowUpdaterOptions ?? throw new ArgumentNullException(nameof(documentFlowUpdaterOptions))).Value;
			_settingController = settingController ?? throw new ArgumentNullException(nameof(settingController));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_edoContainerFileStorageService =
				edoContainerFileStorageService ?? throw new ArgumentNullException(nameof(edoContainerFileStorageService));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_edoDocflowLastProcessTimeRepository =
				edoDocflowLastProcessTimeRepository ?? throw new ArgumentNullException(nameof(edoDocflowLastProcessTimeRepository));
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки ЭДО(получение временных меток)"))
			{
				_lastEventsProcessTime =
					_edoDocflowLastProcessTimeRepository.GetTaxcomEdoDocflowLastProcessTime(
						uow,
						_documentFlowUpdaterOptions.EdoAccount);

				if(_lastEventsProcessTime is null)
				{
					throw new InvalidOperationException("Не найдены временные метки по указанному кабинету в ЭДО");
				}
			}

			_logger.LogInformation("Процесс электронного документооборота запущен");
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
					await ProcessIngoingDocuments(cancellationToken);
					await ProcessWaitingForCancellationDocuments(cancellationToken);

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
									LastEventTimeStamp = _lastEventsProcessTime.LastProcessedEventOutgoingDocuments.ToBinary(),
									DocFlowDirection = "Outgoing",
									DepartmentId = null,
									IncludeTransportInfo = true
								},
								cancellationToken);

						if(docFlowUpdates.Updates is null)
						{
							return;
						}

						_logger.LogInformation(
							"Обрабатываем полученные исходящие документообороты {DocFlowUpdatesCount}",
							docFlowUpdates.Updates.Count());

						foreach(var item in docFlowUpdates.Updates)
						{
							EdoContainer container = null;
							EdoDocFlowDocument mainDocument = null;

							if(item.Documents.Any())
							{
								mainDocument = item.Documents.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExternalIdentifier));

								if(mainDocument is null)
								{
									_logger.LogWarning(
										"Исходящий ДО {DocflowId} со статусом {DocflowStatus} пришел без главного документа или с неизвестным документом. Возможно ручная отправка...",
										item.Id,
										item.Status);
									continue;
								}

								container = _orderRepository.GetEdoContainerByMainDocumentId(uow, mainDocument.ExternalIdentifier);
							}
							else
							{
								_logger.LogWarning(
									"Исходящий ДО {DocflowId} со статусом {DocflowStatus} пришел без документов",
									item.Id,
									item.Status);
								continue;
							}

							_logger.LogInformation(
								"Обрабатываем полученные изменения исходящего ДО {DocflowId} со статусом {DocflowStatus} транзакция {Transaction}",
								item.Id,
								item.Status,
								mainDocument.TransactionCode);

							if(container != null)
							{
								await TryUpdateEdoContainer(cancellationToken, container, item, mainDocument, taxcomApiClient, uow);
							}
							else
							{
								await SendOutgoingTaxcomDocflowUpdatedEvent(item, mainDocument, cancellationToken);
							}

							_lastEventsProcessTime.LastProcessedEventOutgoingDocuments = item.StatusChangeDateTime;
						}
					} while(!docFlowUpdates.IsLast);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки исходящих документов");
			}
			finally
			{
				await SaveLastEventProcessTime();
			}
		}

		private async Task SendOutgoingTaxcomDocflowUpdatedEvent(
			EdoDocFlow docflow,
			EdoDocFlowDocument mainDocument,
			CancellationToken cancellationToken)
		{
			if(mainDocument is null)
			{
				_logger.LogWarning("Документооборот {DocflowId} без главного документа", docflow.Id);
				return;
			}

			var isReceived =
				docflow.Documents.FirstOrDefault(x => x.TransactionCode == _postDateConfirmation) != null;

			var @event = new OutgoingTaxcomDocflowUpdatedEvent
			{
				DocFlowId = docflow.Id,
				MainDocumentId = mainDocument.ExternalIdentifier,
				EdoAccount = _documentFlowUpdaterOptions.EdoAccount,
				Status = docflow.Status,
				StatusChangeDateTime = docflow.StatusChangeDateTime,
				ErrorDescription = docflow.ErrorDescription,
				IsReceived = isReceived
			};

			UpdateTrueMarkTraceabilityInfo(docflow, @event);

			await _publishEndpoint.Publish(@event, cancellationToken);
		}

		private void UpdateTrueMarkTraceabilityInfo(EdoDocFlow docflow, OutgoingTaxcomDocflowUpdatedEvent @event)
		{
			TrueMarkTraceabilityStatus? trueMarkStatus = null;

			foreach(var docflowDocument in docflow.Documents)
			{
				trueMarkStatus = docflowDocument.TransactionCode switch
				{
					_trueMarkAccepted => TrueMarkTraceabilityStatus.Accepted,
					_trueMarkRejected => TrueMarkTraceabilityStatus.Rejected,
					_trueMarkCancellationAccepted => TrueMarkTraceabilityStatus.CancellationAccepted,
					_trueMarkCancellationRejected => TrueMarkTraceabilityStatus.CancellationRejected,
					_ => trueMarkStatus
				};
			}

			@event.TrueMarkTraceabilityStatus = trueMarkStatus?.ToString();
		}

		private async Task ProcessIngoingDocuments(CancellationToken cancellationToken)
		{
			try
			{
				EdoDocFlowUpdates docFlowUpdates;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки входящих документов"))
				{
					do
					{
						_logger.LogInformation("Получаем входящие документы");

						using var scope = _serviceScopeFactory.CreateScope();
						var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();

						docFlowUpdates =
							await taxcomApiClient.GetDocFlowsUpdates(
								new GetDocFlowsUpdatesParameters
								{
									DocFlowStatus = "WaitingForSignature", //смотрим только доки, ожидающих подписи
									LastEventTimeStamp = _lastEventsProcessTime.LastProcessedEventIngoingDocuments.ToBinary(),
									DocFlowDirection = "Ingoing",
									DepartmentId = null,
									IncludeTransportInfo = true
								},
								cancellationToken);

						if(docFlowUpdates.Updates is null)
						{
							return;
						}

						_logger.LogInformation(
							"Обрабатываем полученные входящие документообороты {DocFlowUpdatesCount}",
							docFlowUpdates.Updates.Count());
						
						var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(
							uow, _documentFlowUpdaterOptions.EdoAccount);

						if(organization is null)
						{
							throw new InvalidOperationException(
								"Не найдена организация с таким кабинетом ЭДО " + _documentFlowUpdaterOptions.EdoAccount);
						}

						foreach(var item in docFlowUpdates.Updates)
						{
							await SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(item, organization.Name, cancellationToken);
							_lastEventsProcessTime.LastProcessedEventIngoingDocuments = item.StatusChangeDateTime;
						}
					} while(!docFlowUpdates.IsLast);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки входящих документов");
			}
			finally
			{
				await SaveLastEventProcessTime();
			}
		}

		private async Task ProcessWaitingForCancellationDocuments(CancellationToken cancellationToken)
		{
			try
			{
				EdoDocFlowUpdates docFlowUpdates;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки документов ожидающих аннулирования"))
				{
					do
					{
						_logger.LogInformation("Получаем документы ожидающие аннулирования");

						using var scope = _serviceScopeFactory.CreateScope();
						var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
						var lastProcessTime = _lastEventsProcessTime
							.LastProcessedEventWaitingForCancellationDocuments
							.ToBinary();

						docFlowUpdates =
							await taxcomApiClient.GetDocFlowsUpdates(
								new GetDocFlowsUpdatesParameters
								{
									DocFlowStatus = "WaitingForCancellation",
									LastEventTimeStamp = lastProcessTime,
									DocFlowDirection = "Ingoing",
									DepartmentId = null,
									IncludeTransportInfo = true
								},
								cancellationToken);

						if(docFlowUpdates.Updates is null)
						{
							return;
						}

						_logger.LogInformation(
							"Обрабатываем документообороты ожидающие аннулирования: {DocFlowUpdatesCount}",
							docFlowUpdates.Updates.Count()
						);
						
						foreach(var item in docFlowUpdates.Updates)
						{
							await SendAcceptingWaitingForCancellationDocflowEvent(item, cancellationToken);
							_lastEventsProcessTime.LastProcessedEventWaitingForCancellationDocuments = item.StatusChangeDateTime;
						}
					} while(!docFlowUpdates.IsLast);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки документов ожидающих аннулирования");
			}
			finally
			{
				await SaveLastEventProcessTime();
			}
		}

		private async Task SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(
			EdoDocFlow docflow, string organization, CancellationToken cancellationToken)
		{
			var @event = new AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent
			{
				DocFlowId = docflow.Id,
				Organization = organization,
				MainDocumentId = docflow.Documents.First().ExternalIdentifier,
				EdoAccount = _documentFlowUpdaterOptions.EdoAccount,
			};
			
			await _publishEndpoint.Publish(@event, cancellationToken);
		}

		private async Task SendAcceptingWaitingForCancellationDocflowEvent(
			EdoDocFlow docflow, CancellationToken cancellationToken)
		{
			var @event = new AcceptingWaitingForCancellationDocflowEvent
			{
				DocFlowId = docflow.Id.Value.ToString(),
				EdoAccount = _documentFlowUpdaterOptions.EdoAccount,
			};

			await _publishEndpoint.Publish(@event, cancellationToken);
		}

		private async Task TryUpdateEdoContainer(
			CancellationToken cancellationToken,
			EdoContainer container,
			EdoDocFlow docflow,
			EdoDocFlowDocument mainDocument,
			ITaxcomApiClient taxcomApiClient,
			IUnitOfWork uow)
		{
			if(container is null)
			{
				return;
			}
			
			var containerReceived =
				docflow.Documents.FirstOrDefault(x => x.TransactionCode == _postDateConfirmation) != null;

			container.DocFlowId = docflow.Id;
			container.Received = containerReceived;
			container.InternalId = mainDocument.InternalId;
			container.ErrorDescription = docflow.ErrorDescription;
			container.EdoDocFlowStatus = docflow.Status.TryParseAsEnum<EdoDocFlowStatus>().Value;

			if(container.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
			{
				var containerRawData =
					await taxcomApiClient.GetDocFlowRawData(docflow.Id.Value.ToString(), cancellationToken);

				using var ms = new MemoryStream(containerRawData.ToArray());

				var result =
					await _edoContainerFileStorageService.UpdateContainerAsync(container, ms, cancellationToken);

				if(result.IsFailure)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Message));

					_logger.LogError("Не удалось обновить контейнер, ошибка: {Errors}", errors);
				}
			}
			
			TryUpdateEdoTask(uow, container);

			_logger.LogInformation("Сохраняем изменения контейнера по заказу №{OrderId}", container.Order?.Id);
			await uow.SaveAsync(container);
			await uow.CommitAsync();
		}

		private void TryUpdateEdoTask(IUnitOfWork uow, EdoContainer container)
		{
			var task = uow
				.GetAll<BulkAccountingEdoTask>()
				.SingleOrDefault(x => x.Id == container.EdoTaskId);

			if(task is null)
			{
				_logger.LogWarning(
					"Не найдена таска для контейнера с документом {EdoDocumentId} по заказу {OrderId}, возможно это старый контейнер...",
					container.MainDocumentId,
					container.Order.Id);
				return;
			}

			switch(container.EdoDocFlowStatus)
			{
				case EdoDocFlowStatus.Succeed:
					task.Status = EdoTaskStatus.Completed;
					break;
				//что делаем при аннулировании???
				//все зависит от наших действий, если мы работаем с одной таской, то скорее всего ничего
				//т.к. будет создана задача на переотправку, а сама таска уже в нужном статусе, т.е. в работе
				//иначе надо переводить ее в завершенный статус, если у нас будет создаваться новая таска на переотправку
				case EdoDocFlowStatus.Cancelled:
				case EdoDocFlowStatus.NotAccepted:
				case EdoDocFlowStatus.Unknown:
					break;
				
				case EdoDocFlowStatus.Error:
				case EdoDocFlowStatus.Warning:
				case EdoDocFlowStatus.CompletedWithDivergences:
					task.Status = EdoTaskStatus.Problem;
					//создать описание проблемы
					break;
				default:
					task.Status = EdoTaskStatus.InProgress;
					break;
			}
			
			uow.Save(task);
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
							&& ec.Type == DocumentContainerType.Bill
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
		
		private async Task SaveLastEventProcessTime()
		{
			try
			{
				using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сохранение временной метки");
				await uow.SaveAsync(_lastEventsProcessTime);
				await uow.CommitAsync();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Не удалось сохранить временную метку");
			}
		}
	}
}
