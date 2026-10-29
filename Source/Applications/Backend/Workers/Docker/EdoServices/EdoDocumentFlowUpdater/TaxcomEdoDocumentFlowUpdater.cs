using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using EdoDocumentFlowUpdater.Configs;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.DocflowDocuments;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Application.FileStorage;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Edo;
using Vodovoz.Settings;
using Vodovoz.Zabbix.Sender;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;
using IOrganizationRepository = Vodovoz.EntityRepositories.Organizations.IOrganizationRepository;

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
		private readonly IEdoRepository _edoRepository;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly string _serviceName;
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
			IPublishEndpoint publishEndpoint,
			IEdoRepository edoRepository,
			EdoProblemRegistrar edoProblemRegistrar)
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

			_serviceName = $"{nameof(TaxcomEdoDocumentFlowUpdater)}_{_documentFlowUpdaterOptions.EdoAccount}";
			_edoRepository = edoRepository;
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
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
					await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
						$"Не найдены временные метки по указанному кабинету в ЭДО {_documentFlowUpdaterOptions.EdoAccount}", cancellationToken);	
					
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

					await _zabbixSender.SendIsHealthyAsync(_serviceName, cancellationToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при запуске обновления статусов документооборотов");

					await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
						$"Ошибка при запуске обновления статусов документооборотов: {e}", cancellationToken);
				}
			}
		}

		private async Task ProcessOutgoingDocuments(CancellationToken cancellationToken)
		{
			try
			{
				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки исходящих документов"))
				{
					var lastProcessTime = _lastEventsProcessTime.LastProcessedEventOutgoingDocuments;
					DateTime resultLastProcessTime = lastProcessTime;

					var excludedDocflowIds = new List<string>();
					
					_logger.LogInformation("Получаем исходящие документы для статуса: Завершен с расхождениями");
					var completedWithDivergencesProcessed = await ProcessExceptedStatusOutgoingUpdates(
						uow, 
						"CompletedWithDivergences", 
						lastProcessTime, 
						cancellationToken
					);
					excludedDocflowIds.AddRange(completedWithDivergencesProcessed);

					_logger.LogInformation("Получаем исходящие документы для статуса: Не принят");
					var notAcceptedProcessed = await ProcessExceptedStatusOutgoingUpdates(
						uow,
						"NotAccepted",
						lastProcessTime,
						cancellationToken
					);
					excludedDocflowIds.AddRange(notAcceptedProcessed);

					excludedDocflowIds = excludedDocflowIds.Distinct().ToList();

					EdoDocFlowUpdates docFlowUpdates = null;
					do
					{
						using var scope = _serviceScopeFactory.CreateScope();
						var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
						docFlowUpdates = await taxcomApiClient.GetDocFlowsUpdates(
							new GetDocFlowsUpdatesParameters
							{
								DocFlowStatus = null,
								LastEventTimeStamp = lastProcessTime.ToBinary(),
								DocFlowDirection = "Outgoing",
								DepartmentId = null,
								IncludeTransportInfo = true
							},
							cancellationToken
						);

						if(docFlowUpdates.Updates is null)
						{
							break;
						}

						var remainingUpdates = docFlowUpdates.Updates
							.Where(x => x.Id.HasValue)
							.Where(x => !excludedDocflowIds.Contains(x.Id.Value.ToString()))
							.Select(x => new EdoDocFlow
							{
								Id = x.Id,
								Status = x.Status,
								InternalStatus = x.InternalStatus,
								StatusChangeDateTime = x.StatusChangeDateTime,
								Documents = x.Documents,
								ErrorDescription = x.ErrorDescription
							});

						docFlowUpdates = new EdoDocFlowUpdates
						{
							Updates = remainingUpdates,
							RequestDateTime = docFlowUpdates.RequestDateTime,
							LastEventTimeStamp = docFlowUpdates.LastEventTimeStamp,
							IsLast = docFlowUpdates.IsLast
						};

						var lastTime = await ProcessOutgoingUpdates(
							uow,
							docFlowUpdates, 
							taxcomApiClient, 
							lastProcessTime, 
							cancellationToken
						);

						var toExclude = docFlowUpdates.Updates
							.Where(x => x.Id.HasValue)
							.Select(x => x.Id.ToString());
						excludedDocflowIds.AddRange(toExclude);

						_lastEventsProcessTime.LastProcessedEventOutgoingDocuments = lastTime;
						lastProcessTime = lastTime;

					} while(!docFlowUpdates.IsLast);

					await uow.CommitAsync(cancellationToken);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки исходящих документов");

				await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
					$"Ошибка в процессе обработки исходящих документов: {e}", cancellationToken);
			}
			finally
			{
				await SaveLastEventProcessTime(cancellationToken);
			}
		}

		private async Task<IEnumerable<string>> ProcessExceptedStatusOutgoingUpdates(
			IUnitOfWork uow,
			string status,
			DateTime lastProcessTime,
			CancellationToken cancellationToken
			)
		{
			var result = new List<string>();
			EdoDocFlowUpdates docFlowUpdates = null;
			do
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
				docFlowUpdates = await taxcomApiClient.GetDocFlowsUpdates(
					new GetDocFlowsUpdatesParameters
					{
						DocFlowStatus = status,
						LastEventTimeStamp = lastProcessTime.ToBinary(),
						DocFlowDirection = "Outgoing",
						DepartmentId = null,
						IncludeTransportInfo = true
					},
					cancellationToken
				);

				if(docFlowUpdates.Updates is null)
				{
					return result;
				}

				// замена статуса у документооборотов, у которых приходит неправильный статус
				// приходит Success, вместо CompletedWithDivergences и NotAccepted
				foreach(var update in docFlowUpdates.Updates)
				{
					update.Status = status;
				}

				await ProcessOutgoingUpdates(
					uow,
					docFlowUpdates,
					taxcomApiClient,
					lastProcessTime,
					cancellationToken
				);

				var toExclude = docFlowUpdates.Updates
					.Where(x => x.Id.HasValue)
					.Select(x => x.Id.ToString());
				result.AddRange(toExclude);

			} while(!docFlowUpdates.IsLast);

			return result;
		}

		private async Task<DateTime> ProcessOutgoingUpdates(
			IUnitOfWork uow,
			EdoDocFlowUpdates docFlowUpdates,
			ITaxcomApiClient taxcomApiClient,
			DateTime lastProcessEventTime,
			CancellationToken cancellationToken
		)
		{
			_logger.LogInformation(
				"Обрабатываем полученные исходящие документообороты: {DocFlowUpdatesCount}",
				docFlowUpdates.Updates.Count()
			);

			DateTime lastProcessEventTimeResult = lastProcessEventTime;

			foreach(var docflowUpdate in docFlowUpdates.Updates)
			{
				EdoContainer container = null;
				EdoDocFlowDocument mainDocument = null;

				if(docflowUpdate.Documents.Any())
				{
					mainDocument = docflowUpdate.Documents.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExternalIdentifier));

					if(mainDocument is null)
					{
						_logger.LogWarning(
							"Исходящий ДО {DocflowId} со статусом {DocflowStatus} пришел без главного документа " +
							"или с неизвестным документом. Возможно ручная отправка...",
							docflowUpdate.Id,
							docflowUpdate.Status);
						continue;
					}

					container = _orderRepository.GetEdoContainerByMainDocumentId(uow, mainDocument.ExternalIdentifier);
				}
				else
				{
					_logger.LogWarning(
						"Исходящий ДО {DocflowId} со статусом {DocflowStatus} пришел без документов",
						docflowUpdate.Id,
						docflowUpdate.Status);
					continue;
				}

				_logger.LogInformation(
					"Обрабатываем полученные изменения исходящего ДО {DocflowId} со статусом {DocflowStatus} транзакция {Transaction}",
					docflowUpdate.Id,
					docflowUpdate.Status,
					mainDocument.TransactionCode);

				if(container != null)
				{
					await TryUpdateEdoContainer(cancellationToken, container, docflowUpdate, mainDocument, taxcomApiClient, uow);
				}
				else
				{
					await SendOutgoingTaxcomDocflowUpdatedEvent(docflowUpdate, mainDocument, cancellationToken);
				}

				await UpdateDocflowServiceDocumentMessage(uow, docflowUpdate, taxcomApiClient, cancellationToken);

				if(lastProcessEventTimeResult < docflowUpdate.StatusChangeDateTime)
				{
					lastProcessEventTimeResult = docflowUpdate.StatusChangeDateTime;
				}
			}

			return lastProcessEventTimeResult;
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

			var isReceived = docflow.Documents
				.FirstOrDefault(x => x.TransactionCode == _postDateConfirmation) != null;

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

		private static void UpdateTrueMarkTraceabilityInfo(EdoDocFlow docflow, OutgoingTaxcomDocflowUpdatedEvent @event)
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

						docFlowUpdates = await taxcomApiClient.GetDocFlowsUpdates(
							new GetDocFlowsUpdatesParameters
							{
								DocFlowStatus = "WaitingForSignature", //смотрим только доки, ожидающих подписи
								LastEventTimeStamp = _lastEventsProcessTime.LastProcessedEventIngoingDocuments.ToBinary(),
								DocFlowDirection = "Ingoing",
								DepartmentId = null,
								IncludeTransportInfo = true
							},
							cancellationToken
						);

						if(docFlowUpdates.Updates is null)
						{
							return;
						}

						_logger.LogInformation(
							"Обрабатываем полученные входящие документообороты {DocFlowUpdatesCount}",
							docFlowUpdates.Updates.Count()
						);
						
						var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(
							uow, 
							_documentFlowUpdaterOptions.EdoAccount
						);

						if(organization is null)
						{
							throw new InvalidOperationException(
								"Не найдена организация с таким кабинетом ЭДО " + _documentFlowUpdaterOptions.EdoAccount);
						}

						foreach(var docflowUpdate in docFlowUpdates.Updates)
						{
							await SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(docflowUpdate, organization.Name, cancellationToken);
							_lastEventsProcessTime.LastProcessedEventIngoingDocuments = docflowUpdate.StatusChangeDateTime;
						}

					} while(!docFlowUpdates.IsLast);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки входящих документов");

				await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
					$"Ошибка в процессе обработки входящих документов: {e}", cancellationToken);
			}
			finally
			{
				await SaveLastEventProcessTime(cancellationToken);
			}
		}

		private async Task UpdateDocflowServiceDocumentMessage(
			IUnitOfWork uow,
			EdoDocFlow docflowUpdate,
			ITaxcomApiClient taxcomApiClient,
			CancellationToken cancellationToken
		)
		{
			try
			{
				if(docflowUpdate.Id == null)
				{
					return;
				}

				var documentWithMessages = await taxcomApiClient.GetDocumentWithMessages(docflowUpdate.Id.Value.ToString(), cancellationToken);
				if(!documentWithMessages.Any())
				{
					return;
				}

				var docflow = _edoRepository.GetTaxcomDocflowByDocflowId(uow, docflowUpdate.Id.Value);

				foreach(var document in documentWithMessages)
				{
					switch(document.DocumentType)
					{
						case DocumentWithMessageType.CompletedWithDiscrepancy:
							docflow.DiscrepancyMessage = document.Message;
							break;
						case DocumentWithMessageType.CorrectionNotice:
							docflow.CorrectionNoticeMessage = document.Message;
							break;
						case DocumentWithMessageType.CancellationOffer:
							docflow.CancellationOfferMessage = document.Message;
							break;
						default:
							throw new NotSupportedException($"Не поддерживаемый тип документа {document.DocumentType}");
					}
				}

				await uow.SaveAsync(docflow, true, cancellationToken: cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обновлении сообщений из документов в документообороте {DocflowId}", docflowUpdate.Id);
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

				await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
					$"Ошибка в процессе обработки документов ожидающих аннулирования: {e}", cancellationToken);
			}
			finally
			{
				await SaveLastEventProcessTime(cancellationToken);
			}
		}

		private async Task SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(
			EdoDocFlow docflow, string organization, CancellationToken cancellationToken)
		{
			var mainDocumentId = docflow.Documents
				.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExternalIdentifier))?
				.ExternalIdentifier;
			
			var @event = new AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent
			{
				DocFlowId = docflow.Id,
				Organization = organization,
				MainDocumentId = mainDocumentId,
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
				docflow.Status.TryParseAsEnum<EdoDocFlowStatus>().Value is EdoDocFlowStatus.Sent;

			container.DocFlowId = docflow.Id;
			container.Received = containerReceived;
			container.InternalId = mainDocument.InternalId;
			container.ErrorDescription = docflow.ErrorDescription;
			container.EdoDocFlowStatus = docflow.Status.TryParseAsEnum<EdoDocFlowStatus>().Value;

			if(container.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
			{
				var docFlowRawDataResult = await taxcomApiClient.GetDocFlowRawData(docflow.Id.Value.ToString(), cancellationToken);
				var containerRawData = docFlowRawDataResult.Value;

				using var ms = new MemoryStream(containerRawData.ToArray());

				Result result = null;

				try
				{
					result = await _edoContainerFileStorageService.UpdateContainerAsync(container, ms, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Не удалось обновить контейнер в хранилище");

					await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
						$"Не удалось обновить контейнер в хранилище: {ex}", cancellationToken);
				}

				if(result != null && result.IsFailure)
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
					container.Order?.Id);
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
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Аннулирование счетов нового ЭДО");
			var requests = uow.GetAll<InformalEdoRequest>()
				.Where(x => (x.OrderDocumentType == OrderDocumentType.Bill || x.OrderDocumentType == OrderDocumentType.SpecialBill)
					&& x.Task != null && x.Task.CancellationReason != null
					&& x.Task.Status != EdoTaskStatus.Cancelled && x.Task.Status != EdoTaskStatus.InCancellation)
				.ToList();
			foreach(var request in requests)
			{
				try
				{
					using var scope = _serviceScopeFactory.CreateScope();
					var client = scope.ServiceProvider.GetRequiredService<ITaxcomApiClient>();
					
					if(!await SendBillCancellationOffers(uow, client, request, cancellationToken))
					{
						continue;
					}

					await uow.SaveAsync(request.Task, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
				catch(Exception exception)
				{
					_logger.LogError(exception, "Ошибка аннулирования счета по заявке №{RequestId}", request.Id);
				}
			}
		}

		private async Task<bool> SendBillCancellationOffers(
			IUnitOfWork uow, ITaxcomApiClient taxcomApiClient,
			InformalEdoRequest request, CancellationToken cancellationToken)
		{
			var document = uow.GetAll<OutgoingInformalEdoDocument>()
				.Where(x => x.InformalDocumentTaskId == request.Task.Id)
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();
			if(document == null)
			{
				if(request.Task.Status != EdoTaskStatus.New)
				{
					return false;
				}
				request.Task.ProcessTaskCancellation(EdoTaskStatus.Cancelled, request.Task.CancellationReason);
				request.Task.EndTime = DateTime.Now;
				return true;
			}

			switch (document.Status)
			{
				case EdoDocumentStatus.Error:
				case EdoDocumentStatus.Warning:
					request.Task.CancellationReason = null;
					return true;
				case EdoDocumentStatus.Cancelled:
				case EdoDocumentStatus.WaitingForCancellation:
					request.Task.ProcessTaskCancellation(document.Status == EdoDocumentStatus.Cancelled
						? EdoTaskStatus.Cancelled : EdoTaskStatus.InCancellation, request.Task.CancellationReason);
					return true;
			}

			var docflowIds = uow.GetAll<TaxcomDocflow>()
				.Where(x => x.EdoDocumentId == document.Id && x.DocflowId != null)
				.Select(x => x.DocflowId.Value)
				.Distinct()
				.ToList();
			if(!docflowIds.Any())
			{
				return false;
			}
			foreach(var docflowId in docflowIds)
			{
				await taxcomApiClient.SendOfferCancellation(docflowId.ToString(), request.Task.CancellationReason, cancellationToken);
			}
			request.Task.ProcessTaskCancellation(EdoTaskStatus.InCancellation, request.Task.CancellationReason);
			return true;
		}

		private async Task DelayAsync(CancellationToken cancellationToken)
		{
			var delay = _documentFlowUpdaterOptions.DelayBetweenDocumentFlowProcessingInSeconds;
			
			_logger.LogInformation("Ждем {Delay}сек", delay);
			await Task.Delay(delay * 1000, cancellationToken);
		}
		
		private async Task SaveLastEventProcessTime(CancellationToken cancellationToken)
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

				await _zabbixSender.SendProblemMessageAsync(_serviceName, ZabixSenderMessageType.Problem,
					$"Не удалось сохранить временную метку: {e}",
					cancellationToken);
			}
		}
	}
}
