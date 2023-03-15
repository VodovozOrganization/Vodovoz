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
		}

		public event Action<string> UpdateNotificationAction;

		public SendedComplaintNotificationDetails GetNotificationDetails(IUnitOfWork uow)
		{
			var result = new SendedComplaintNotificationDetails();
			var sendedComplaints = GetSendedComplaintIdsBySubdivision(uow);

			if(sendedComplaints.Count > 0)
			{
				result.SendedComplaintsCount = sendedComplaints.Count;
				result.NeedNotify = true;
				result.NotificationMessage = GetNotificationMessage(result.SendedComplaintsCount.Value);

				NotifyConfiguration.Instance.BatchSubscribeOnEntity<Complaint>(OnComplaintChanged);
			}

			return result;
		}

		public List<int> GetSendedComplaintIdsBySubdivision(IUnitOfWork uow)
		{
			var compaints = _complaintsRepository.GetUnclosedWithNoCommentsComplaintIdsBySubdivision(uow, _subdivisionIdForNotify);
			return compaints.ToList();
		}

		public string GetNotificationMessageBySubdivision(IUnitOfWork uow)
		{
			return GetNotificationMessage(GetSendedComplaintIdsBySubdivision(uow).Count);
		}

		private string GetNotificationMessage(int sendedComplaints)
		{
			var message = sendedComplaints > 0
				? GetNotificationForPositiveComplaintsCount(sendedComplaints)
				: _messageForZeroComplaintsCount;

			return message;
		}

		private string GetNotificationForPositiveComplaintsCount(int sendedComplaints)
		{
			return string.Format("<span foreground=\"red\">Внимание! У Вашего отдела {0} {1}.</span>", 
				sendedComplaints,
				sendedComplaints == 1 ? "открытая рекламация" : "открытых рекламации");
		}

		private void OnComplaintChanged(EntityChangeEvent[] changeEvents)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				UpdateNotificationAction?.Invoke(GetNotificationMessageBySubdivision(uow));
			}
		}
	}

	public class SendedComplaintNotificationDetails
	{
		public bool NeedNotify { get; set; }
		public int? SendedComplaintsCount { get; set; }
		public string NotificationMessage { get; set; }
	}
}
