using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Controllers
{
	public class MovementDocumentsNotificationsController : IMovementDocumentsNotificationsController
	{
		private const string _messageForZeroMovementsCount = "Для Вашего отдела и выбранных складов нет складских перемещений ожидающих приемки";
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
				result.SendedMovementsCount = GetTotalSendedMovementDocumentsToWarehouses(uow);
				result.NeedNotify = true;
				result.NotificationMessage = GetNotificationMessage(result.SendedMovementsCount.Value);
				NotifyConfiguration.Instance.BatchSubscribeOnEntity<MovementDocument>(OnMovementDocumentChanged);
			}

			return result;
		}

		public string GetNotificationMessage(IUnitOfWork uow)
		{
			var totalNotificationsCount = GetTotalSendedMovementDocumentsToWarehouses(uow);

			return GetNotificationMessage(totalNotificationsCount);
		}

		private bool NeedNotifyEmployee(IUnitOfWork uow)
		{
			var warehouseByMovementDocumentsNotificationsSubdivisionExists = _warehouseRepository.WarehouseByMovementDocumentsNotificationsSubdivisionExists(uow, _subdivisionIdForNotify);
			var selectedByUserWarehousesToNotificationExists = _userSelectedWarehouses.Count() > 0;

			var needToNotifyEmployee = 
				warehouseByMovementDocumentsNotificationsSubdivisionExists
				|| selectedByUserWarehousesToNotificationExists;

			return needToNotifyEmployee;
		}

		private int GetTotalSendedMovementDocumentsToWarehouses(IUnitOfWork uow)
		{
			var sendedDocumentsToWarehousesByEmployeeSubdivision = 
				GetSendedMovementDocumentsToWarehouseBySubdivision(uow);

			var sendedDocumentsToUserSelectedWarehouses = 
				GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(uow, _userSelectedWarehouses, _subdivisionIdForNotify);

			return sendedDocumentsToWarehousesByEmployeeSubdivision + sendedDocumentsToUserSelectedWarehouses;
		}

		private int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseBySubdivision(uow, _subdivisionIdForNotify);
		}

		private int GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(IUnitOfWork uow, IEnumerable<int> selectedWarehouses, int subdivisionIdForNotify)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(uow, selectedWarehouses, subdivisionIdForNotify);
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
			return $"<span foreground=\"red\">Внимание! Для Вашего отдела " +
				$"{(_userSelectedWarehouses.Count() > 0 ? "и выбранных складов " : string.Empty)} " +
				$"{sendedMovements} складских перемещений ожидают приемки</span>";
		}

		private void OnMovementDocumentChanged(EntityChangeEvent[] changeEvents)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				UpdateNotificationAction?.Invoke(GetNotificationMessage(uow));
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
