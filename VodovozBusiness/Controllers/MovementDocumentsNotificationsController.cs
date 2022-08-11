using Vodovoz.EntityRepositories.Store;
using System;
using QS.DomainModel.UoW;

namespace Vodovoz.Controllers
{
	public class MovementDocumentsNotificationsController : IMovementDocumentsNotificationsController
	{
		private const string _messageForZeroMovementsCount = "Для Вашего отдела нет складских перемещений ожидающих приемки";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IMovementDocumentRepository _movementDocumentRepository;

		public MovementDocumentsNotificationsController(
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehouseRepository warehouseRepository,
			IMovementDocumentRepository movementDocumentRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_movementDocumentRepository =
			 movementDocumentRepository ?? throw new ArgumentNullException(nameof(movementDocumentRepository));
		}

		public SendedMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow, Subdivision subdivision)
		{
			var result = new SendedMovementsNotificationDetails();

			if(subdivision is null)
			{
				return result;
			}

			if(NeedNotifyEmployee(uow, subdivision))
			{
				result.SendedMovementsCount = GetSendedMovementDocumentsToWarehouseBySubdivision(uow, subdivision.Id);
				result.NeedNotify = true;
				result.NotificationMessage = GetNotificationMessage(result.SendedMovementsCount.Value);
			}

			return result;
		}

		public string GetNotificationMessageBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return GetNotificationMessage(GetSendedMovementDocumentsToWarehouseBySubdivision(uow, subdivisionId));
		}

		private bool NeedNotifyEmployee(IUnitOfWork uow, Subdivision subdivision)
		{
			return _warehouseRepository.WarehouseByMovementDocumentsNotificationsSubdivisionExists(uow, subdivision.Id);
		}

		private int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseBySubdivision(uow, subdivisionId);
		}

		private string GetNotificationMessage(int sendedMovements)
		{
			var message = sendedMovements > 0
				? GetNotificationForPositiveMovementsCount(sendedMovements)
				: _messageForZeroMovementsCount;

			return message;
		}

		private string GetNotificationForPositiveMovementsCount(int sendedMovements)
		{
			return $"<span foreground=\"red\">Внимание! Для Вашего отдела {sendedMovements} складских перемещений ожидают приемки</span>";
		}
	}

	public class SendedMovementsNotificationDetails
	{
		public bool NeedNotify { get; set; }
		public int? SendedMovementsCount { get; set; }
		public string NotificationMessage { get; set; }
	}
}
