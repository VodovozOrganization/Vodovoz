using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Complaints;

namespace Vodovoz.Controllers
{
	public class ComplaintNotificationController : IComplaintNotificationController
	{
		private readonly string _messageForZeroComplaintsCount = "Для Вашего отдела нет рекламаций, ожидающих комментария";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly int _subdivisionIdForNotify;

		public ComplaintNotificationController(
			IUnitOfWorkFactory unitOfWorkFactory,
			IComplaintsRepository complaintsRepository,
			int subdivisionIdForNotify)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_subdivisionIdForNotify = subdivisionIdForNotify;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ComplaintDiscussionComment>(OnComplaintChanged);
		}

		public event Action<SendedComplaintNotificationDetails> UpdateNotificationAction;

		public SendedComplaintNotificationDetails GetNotificationDetails(IUnitOfWork uow)
		{
			var sendedComplaints = GetSendedComplaintIdsBySubdivision(uow);

			var notificationDetails = new SendedComplaintNotificationDetails()
			{
				SendedComplaintsCount = sendedComplaints.Count,
				NeedNotify = sendedComplaints.Count > 0,
				NotificationMessage = GetNotificationMessage(sendedComplaints.Count),
				SendedComplaintsIds = sendedComplaints
			};

			return notificationDetails;
		}

		private List<int> GetSendedComplaintIdsBySubdivision(IUnitOfWork uow)
		{
			var compaints = _complaintsRepository.GetUnclosedWithNoCommentsComplaintIdsBySubdivision(uow, _subdivisionIdForNotify);
			return compaints.ToList();
		}

		private string GetNotificationMessage(int sendedComplaints)
		{
			var message = sendedComplaints > 0
				? GetNotificationForPositiveComplaintsCount(sendedComplaints)
				: _messageForZeroComplaintsCount;

			return message;
		}

		private string GetNotificationForPositiveComplaintsCount(int sendedComplaints) =>
			$"Внимание! У Вашего отдела {sendedComplaints} {(sendedComplaints == 1 ? "открытая рекламация" : "открытых рекламации")}.";

		private void OnComplaintChanged(EntityChangeEvent[] changeEvents)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				UpdateNotificationAction?.Invoke(GetNotificationDetails(uow));
			}
		}
	}

	public class SendedComplaintNotificationDetails
	{
		public bool NeedNotify { get; set; }
		public int SendedComplaintsCount { get; set; }
		public string NotificationMessage { get; set; }
		public List<int> SendedComplaintsIds { get; set; }
	}
}
