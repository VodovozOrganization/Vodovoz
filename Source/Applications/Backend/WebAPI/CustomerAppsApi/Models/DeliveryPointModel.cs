using System;
using System.Linq;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Validators;
using Gamma.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozInfrastructure.Cryptography;

namespace CustomerAppsApi.Models
{
	public class DeliveryPointModel : IDeliveryPointModel
	{
		private readonly ILogger<DeliveryPointModel> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryPointModelValidator _deliveryPointModelValidator;
		private readonly IDeliveryPointFactory _deliveryPointFactory;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IMD5HexHashFromString _md5HexHashFromString;
		private object _locker = new object();

		public DeliveryPointModel(
			ILogger<DeliveryPointModel> logger,
			IUnitOfWork uow,
			IDeliveryPointModelValidator deliveryPointModelValidator,
			IDeliveryPointFactory deliveryPointFactory,
			IDeliveryPointRepository deliveryPointRepository,
			IMD5HexHashFromString md5HexHashFromString)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_deliveryPointModelValidator =
				deliveryPointModelValidator ?? throw new ArgumentNullException(nameof(deliveryPointModelValidator));
			_deliveryPointFactory = deliveryPointFactory ?? throw new ArgumentNullException(nameof(deliveryPointFactory));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_md5HexHashFromString = md5HexHashFromString ?? throw new ArgumentNullException(nameof(md5HexHashFromString));
		}
		
		public DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId)
		{
			_logger.LogInformation("Поступил запрос выборки всех ТД клиента {CounterpartyId} от {Source}",
				counterpartyErpId,
				SourceTitle(source));
			
			try
			{
				var deliveryPoints =
					_deliveryPointRepository.GetDeliveryPointsForSendByCounterpartyId(_uow, counterpartyErpId);
				return _deliveryPointFactory.CreateDeliveryPointsInfo(deliveryPoints);
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

		public int AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			_logger.LogInformation("Поступил запрос добавления ТД клиенту {CounterpartyId} от {Source}",
				newDeliveryPointInfoDto.CounterpartyErpId,
				SourceTitle(newDeliveryPointInfoDto.Source));

			var validationResult = _deliveryPointModelValidator.NewDeliveryPointInfoDtoValidate(newDeliveryPointInfoDto);

			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation(
					"Не прошли валидацию при создании ТД от {Source} для клиента {CounterpartyId}: {ValidationResult}",
					SourceTitle(newDeliveryPointInfoDto.Source),
					newDeliveryPointInfoDto.CounterpartyErpId,
					validationResult);

				return StatusCodes.Status500InternalServerError;
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

						return StatusCodes.Status202Accepted;
					}

					try
					{
						_logger.LogInformation(
							"Пробуем сохранить запись о создаваемой ТД от {Source} для клиента {CounterpartyId}",
							SourceTitle(newDeliveryPointInfoDto.Source),
							newDeliveryPointInfoDto.CounterpartyErpId);

						var creatingDeliveryPointDto =
							_deliveryPointFactory.CreateNewExternalCreatingDeliveryPoint(newDeliveryPointInfoDto.Source, uniqueKey);
						_uow.Save(creatingDeliveryPointDto);
						_uow.Commit();
					}
					catch(Exception e)
					{
						_logger.LogError(
							e,
							"Ошибка при записи создаваемой ТД от {Source} для клиента {CounterpartyId}",
							SourceTitle(newDeliveryPointInfoDto.Source),
							newDeliveryPointInfoDto.CounterpartyErpId);

						return StatusCodes.Status500InternalServerError;
					}
				}

				var deliveryPoint = _deliveryPointFactory.CreateNewDeliveryPoint(newDeliveryPointInfoDto);
				deliveryPoint.SetСoordinates(newDeliveryPointInfoDto.Latitude, newDeliveryPointInfoDto.Longitude, _uow);
				
				_uow.Save(deliveryPoint);
				_uow.Commit();

				return StatusCodes.Status200OK;
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"При создании/сохранении новой точки доставки для клиента {CounterpartyId} от {Source} произошла ошибка",
					newDeliveryPointInfoDto.CounterpartyErpId,
					SourceTitle(newDeliveryPointInfoDto.Source));

				return StatusCodes.Status500InternalServerError;
			}
		}

		public UpdatedDeliveryPointCommentDto UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComment)
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
					return _deliveryPointFactory.CreateNotFoundUpdatedDeliveryPointCommentsDto();
				}
				
				deliveryPoint.OnlineComment = updatingComment.Comment;
				_uow.Save(deliveryPoint);
				_uow.Commit();
				
				return _deliveryPointFactory.CreateSuccessUpdatedDeliveryPointCommentsDto();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"При обновлении комментария от {Source} для ТД {DeliveryPointId} произошла ошибка",
					SourceTitle(updatingComment.Source),
					updatingComment.DeliveryPointErpId);
				return _deliveryPointFactory.CreateErrorUpdatedDeliveryPointCommentsDto("При обновлении комментария произошла ошибка");
			}
		}

		private string SourceTitle(Source source) => source.GetEnumTitle();
	}
}
