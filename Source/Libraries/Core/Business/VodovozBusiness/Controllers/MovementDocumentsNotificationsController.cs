using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Controllers
{
	public class MovementDocumentsNotificationsController : IMovementDocumentsNotificationsController
	{
		private const string _messageForZeroMovementsCount = "Для Вашего отдела нет складских перемещений ожидающих приемки";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IMovementDocumentRepository _movementDocumentRepository;
		private readonly int _subdivisionIdForNotify;
		private readonly IEnumerable<int> _userSelectedWarehouses;

		public MovementDocumentsNotificationsController(
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehouseRepository warehouseRepository,
			IMovementDocumentRepository movementDocumentRepository,
			int subdivisionIdForNotify,
			IEnumerable<int> userSelectedWarehouses)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_movementDocumentRepository =
			 movementDocumentRepository ?? throw new ArgumentNullException(nameof(movementDocumentRepository));
			_subdivisionIdForNotify = subdivisionIdForNotify;
			_userSelectedWarehouses = userSelectedWarehouses ?? throw new ArgumentNullException(nameof(userSelectedWarehouses));
		}

		public event Action<string> UpdateNotificationAction;

		public SendedMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow)
		{
			var result = new SendedMovementsNotificationDetails();

			if(NeedNotifyEmployee(uow))
			{
				result.SendedMovementsCount = GetSendedMovementDocumentsToWarehouseBySubdivision(uow);
				result.NeedNotify = true;
				result.NotificationMessage = GetNotificationMessage(result.SendedMovementsCount.Value);
				NotifyConfiguration.Instance.BatchSubscribeOnEntity<MovementDocument>(OnMovementDocumentChanged);
			}

			return result;
		}

		public string GetNotificationMessageBySubdivision(IUnitOfWork uow)
		{
			var notificationsBySubdivisionCount = GetSendedMovementDocumentsToWarehouseBySubdivision(uow);
			var notificationsBySelectedWarehousesCount = GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(uow, _userSelectedWarehouses);

			var totalNotificationsCount = notificationsBySubdivisionCount + notificationsBySelectedWarehousesCount;

			return GetNotificationMessage(totalNotificationsCount);
		}

		private bool NeedNotifyEmployee(IUnitOfWork uow)
		{
			return _warehouseRepository.WarehouseByMovementDocumentsNotificationsSubdivisionExists(uow, _subdivisionIdForNotify);
		}

		private int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseBySubdivision(uow, _subdivisionIdForNotify);
		}

		private int GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(IUnitOfWork uow, IEnumerable<int> selectedWarehouses)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(uow, selectedWarehouses, _subdivisionIdForNotify);
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

		private void OnMovementDocumentChanged(EntityChangeEvent[] changeEvents)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				UpdateNotificationAction?.Invoke(GetNotificationMessageBySubdivision(uow));
			}
		}
	}

	public class SendedMovementsNotificationDetails
	{
		public bool NeedNotify { get; set; }
		public int? SendedMovementsCount { get; set; }
		public string NotificationMessage { get; set; }
	}
}
