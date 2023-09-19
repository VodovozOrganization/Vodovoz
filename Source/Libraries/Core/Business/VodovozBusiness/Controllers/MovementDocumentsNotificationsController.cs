using Vodovoz.EntityRepositories.Store;
using System;
using QS.DomainModel.UoW;
using QS.DomainModel.NotifyChange;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;

namespace Vodovoz.Controllers
{
	public class MovementDocumentsNotificationsController : IMovementDocumentsNotificationsController
	{
		private const string _messageForZeroMovementsCount = "Для Вашего отдела нет складских перемещений ожидающих приемки";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IMovementDocumentRepository _movementDocumentRepository;
		private readonly int _subdivisionIdForNotify;

		public MovementDocumentsNotificationsController(
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehouseRepository warehouseRepository,
			IMovementDocumentRepository movementDocumentRepository,
			int subdivisionIdForNotify)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_movementDocumentRepository =
			 movementDocumentRepository ?? throw new ArgumentNullException(nameof(movementDocumentRepository));
			_subdivisionIdForNotify = subdivisionIdForNotify;
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
			return GetNotificationMessage(GetSendedMovementDocumentsToWarehouseBySubdivision(uow));
		}

		private bool NeedNotifyEmployee(IUnitOfWork uow)
		{
			return _warehouseRepository.WarehouseByMovementDocumentsNotificationsSubdivisionExists(uow, _subdivisionIdForNotify);
		}

		private int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow)
		{
			return _movementDocumentRepository.GetSendedMovementDocumentsToWarehouseBySubdivision(uow, _subdivisionIdForNotify);
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
