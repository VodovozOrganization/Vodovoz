using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Validators;
using Gamma.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Osrm;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Settings.Common;
using VodovozInfrastructure.Cryptography;

namespace CustomerAppsApi.Library.Models
{
	public class DeliveryPointService : IDeliveryPointService
	{
		private readonly ILogger<DeliveryPointService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryPointModelValidator _deliveryPointModelValidator;
		private readonly IDeliveryPointFactory _deliveryPointFactory;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IOsrmSettings _globalSettings;
		private readonly IOsrmClient _osrmClient;
		private readonly IMD5HexHashFromString _md5HexHashFromString;
		private readonly object _locker = new object();

		public DeliveryPointService(
			ILogger<DeliveryPointService> logger,
			IUnitOfWork uow,
			IDeliveryPointModelValidator deliveryPointModelValidator,
			IDeliveryPointFactory deliveryPointFactory,
			IDeliveryPointRepository deliveryPointRepository,
			IDeliveryRepository deliveryRepository,
			IOsrmSettings globalSettings,
			IOsrmClient osrmClient,
			IMD5HexHashFromString md5HexHashFromString)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_deliveryPointModelValidator =
				deliveryPointModelValidator ?? throw new ArgumentNullException(nameof(deliveryPointModelValidator));
			_deliveryPointFactory = deliveryPointFactory ?? throw new ArgumentNullException(nameof(deliveryPointFactory));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_md5HexHashFromString = md5HexHashFromString ?? throw new ArgumentNullException(nameof(md5HexHashFromString));
		}
		
		public DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId)
		{
			_logger.LogInformation("Поступил запрос выборки всех активных ТД клиента {CounterpartyId} от {Source}",
				counterpartyErpId,
				SourceTitle(source));
			
			try
			{
				var deliveryPoints =
					_deliveryPointRepository.GetActiveDeliveryPointsForSendByCounterpartyId(_uow, counterpartyErpId);
				return _deliveryPointFactory.CreateDeliveryPointsDto(deliveryPoints);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении точек доставки клиента {CounterpartyId} для {Source}",
					counterpartyErpId,
					SourceTitle(source));
				
				return _deliveryPointFactory.CreateErrorDeliveryPointsInfo(
					$"Ошибка при получении точек доставки клиента {counterpartyErpId}");
			}
		}

		public CreatedDeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto, out int statusCode, bool isDryRun = false)
		{
			_logger.LogInformation("Поступил запрос добавления ТД клиенту {CounterpartyId} от {Source}",
				newDeliveryPointInfoDto.CounterpartyErpId,
				SourceTitle(newDeliveryPointInfoDto.Source));

			FillEmptyEntranceAndRoomAndFloorDefaultValue(newDeliveryPointInfoDto);
			var validationResult = _deliveryPointModelValidator.NewDeliveryPointInfoDtoValidate(newDeliveryPointInfoDto);

			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation(
					"Не прошли валидацию при создании ТД от {Source} для клиента {CounterpartyId}: {ValidationResult}",
					SourceTitle(newDeliveryPointInfoDto.Source),
					newDeliveryPointInfoDto.CounterpartyErpId,
					validationResult);

				statusCode = StatusCodes.Status500InternalServerError;
				return null;
			}

			try
			{
				var uniqueKey = _md5HexHashFromString.GetMD5HexHashFromString(
					newDeliveryPointInfoDto.CounterpartyErpId + newDeliveryPointInfoDto.Street + newDeliveryPointInfoDto.Building +
					newDeliveryPointInfoDto.Room);

				lock(_locker)
				{
					var creatingDeliveryPoint = _uow
						.GetAll<ExternalCreatingDeliveryPoint>()
						.FirstOrDefault(
							x => x.UniqueKey == uniqueKey
								&& x.Source == (int)newDeliveryPointInfoDto.Source
								&& x.CreatingDate == DateTime.Today);

					if(creatingDeliveryPoint != null)
					{
						_logger.LogInformation(
							"Запрос по созданию ТД от {Source} для клиента {CounterpartyId}, ключ: {uniqueKey} обрабатывается",
							SourceTitle(newDeliveryPointInfoDto.Source),
							newDeliveryPointInfoDto.CounterpartyErpId,
							uniqueKey);

						statusCode = StatusCodes.Status202Accepted;
						return null;
					}

					try
					{
						_logger.LogInformation(
							"Пробуем сохранить запись о создаваемой ТД от {Source} для клиента {CounterpartyId}",
							SourceTitle(newDeliveryPointInfoDto.Source),
							newDeliveryPointInfoDto.CounterpartyErpId);

						var creatingDeliveryPointDto =
							_deliveryPointFactory.CreateNewExternalCreatingDeliveryPoint(newDeliveryPointInfoDto.Source, uniqueKey);

						if(!isDryRun)
						{
							_uow.Save(creatingDeliveryPointDto);
							_uow.Commit();
						}
					}
					catch(Exception e)
					{
						_logger.LogError(
							e,
							"Ошибка при записи создаваемой ТД от {Source} для клиента {CounterpartyId}",
							SourceTitle(newDeliveryPointInfoDto.Source),
							newDeliveryPointInfoDto.CounterpartyErpId);

						statusCode = StatusCodes.Status500InternalServerError;
						return null;
					}
				}

				var deliveryPoint = _deliveryPointFactory.CreateNewDeliveryPoint(newDeliveryPointInfoDto);
				deliveryPoint.SetСoordinates(
					newDeliveryPointInfoDto.Latitude,
					newDeliveryPointInfoDto.Longitude,
					_deliveryRepository,
					_globalSettings,
					_osrmClient,
					_uow);

				if(!isDryRun)
				{
					_uow.Save(deliveryPoint);
					_uow.Commit();
				}

				statusCode = StatusCodes.Status201Created;
				return _deliveryPointFactory.CreateDeliveryPointDto(newDeliveryPointInfoDto, deliveryPoint.Id);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"При создании/сохранении новой точки доставки для клиента {CounterpartyId} от {Source} произошла ошибка",
					newDeliveryPointInfoDto.CounterpartyErpId,
					SourceTitle(newDeliveryPointInfoDto.Source));

				statusCode = StatusCodes.Status500InternalServerError;
				return null;
			}
		}

		public int UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComment, bool isDryRun = false)
		{
			_logger.LogInformation("Поступил запрос обновления комментрия ТД {DeliveryPointId} от {Source}",
				updatingComment.DeliveryPointErpId,
				SourceTitle(updatingComment.Source));
			
			try
			{
				var deliveryPoint = _uow.GetById<DeliveryPoint>(updatingComment.DeliveryPointErpId);

				if(deliveryPoint is null)
				{
					_logger.LogInformation(
						"Запрос по обновлению комментария от {Source} для несуществующей ТД {DeliveryPointId}",
						SourceTitle(updatingComment.Source),
						updatingComment.DeliveryPointErpId);
					
					return StatusCodes.Status404NotFound;
				}
				
				deliveryPoint.OnlineComment = updatingComment.Comment;

				if(!isDryRun)
				{
					_uow.Save(deliveryPoint);
					_uow.Commit();
				}

				return StatusCodes.Status200OK;
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"При обновлении комментария от {Source} для ТД {DeliveryPointId} произошла ошибка",
					SourceTitle(updatingComment.Source),
					updatingComment.DeliveryPointErpId);
				
				return StatusCodes.Status500InternalServerError;
			}
		}

		private string SourceTitle(Source source) => source.GetEnumTitle();
		
		private void FillEmptyEntranceAndRoomAndFloorDefaultValue(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			const string defaultValue = "-";
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Entrance))
			{
				newDeliveryPointInfoDto.Entrance = defaultValue;
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Room))
			{
				newDeliveryPointInfoDto.Room = defaultValue;
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Floor))
			{
				newDeliveryPointInfoDto.Floor = defaultValue;
			}
		}
	}
}
