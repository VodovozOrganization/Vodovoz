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
		private const string _messageForZeroMovementsCount =
			"Для Вашего отдела и выбранных складов нет складских перемещений ожидающих приемки";
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

		public event Action<(bool Alert, string Message)> UpdateNotificationAction;

		public SentMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow)
		{
			if(!NeedNotifyEmployee(uow))
			{
				return new SentMovementsNotificationDetails
				{
					Notification = GetNotificationMessage(0)
				};
			}

			var result = new SentMovementsNotificationDetails
			{
				NeedNotify = true
			};
			
			var sentMovementsCount = GetTotalSentMovementDocumentsToWarehouses(uow);
			result.Notification = GetNotificationMessage(sentMovementsCount);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<MovementDocument>(OnMovementDocumentChanged);

			return result;
		}

		public (bool Alert, string Message) GetNotificationMessage(IUnitOfWork uow)
		{
			var totalNotificationsCount = GetTotalSentMovementDocumentsToWarehouses(uow);
			return GetNotificationMessage(totalNotificationsCount);
		}

		private bool NeedNotifyEmployee(IUnitOfWork uow)
		{
			var warehouseByMovementDocumentsNotificationsSubdivisionExists = 
				_warehouseRepository.WarehouseByMovementDocumentsNotificationsSubdivisionExists(uow, _subdivisionIdForNotify);
			var selectedByUserWarehousesToNotificationExists = _userSelectedWarehouses.Any();

			var needToNotifyEmployee = 
				warehouseByMovementDocumentsNotificationsSubdivisionExists
				|| selectedByUserWarehousesToNotificationExists;

			return needToNotifyEmployee;
		}

		private int GetTotalSentMovementDocumentsToWarehouses(IUnitOfWork uow)
		{
			var sentDocumentsToWarehousesByEmployeeSubdivision = 
				GetSentMovementDocumentsToWarehouseBySubdivision(uow);

			var sentDocumentsToUserSelectedWarehouses = 
				GetSentMovementDocumentsToWarehouseByUserSelectedWarehouses(uow, _userSelectedWarehouses, _subdivisionIdForNotify);

			return sentDocumentsToWarehousesByEmployeeSubdivision + sentDocumentsToUserSelectedWarehouses;
		}

		private int GetSentMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseBySubdivision(uow, _subdivisionIdForNotify);
		}

		private int GetSentMovementDocumentsToWarehouseByUserSelectedWarehouses(
			IUnitOfWork uow,
			IEnumerable<int> selectedWarehouses,
			int subdivisionIdForNotify)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(
				uow, selectedWarehouses, subdivisionIdForNotify);
		}

		private (bool Alert, string Message) GetNotificationMessage(int sentMovements)
		{
			return sentMovements > 0
				? (true, GetNotificationForPositiveMovementsCount(sentMovements))
				: (false, _messageForZeroMovementsCount);
		}

		private string GetNotificationForPositiveMovementsCount(int sentMovements)
		{
			return "Внимание! Для Вашего отдела " +
				$"{(_userSelectedWarehouses.Any() ? "и выбранных складов " : string.Empty)} " +
				$"{sentMovements} складских перемещений ожидают приемки";
		}

		private void OnMovementDocumentChanged(EntityChangeEvent[] changeEvents)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				UpdateNotificationAction?.Invoke(GetNotificationMessage(uow));
			}
		}
	}
}
