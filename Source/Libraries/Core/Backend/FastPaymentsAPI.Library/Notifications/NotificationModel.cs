using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsAPI.Library.Notifications
{
	public class NotificationModel
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IFastPaymentRepository _repository;

		public NotificationModel(IUnitOfWorkFactory uowFactory, IFastPaymentRepository repository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		public void SaveNotification(FastPayment payment, FastPaymentNotificationType type, bool notified)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var notification = _repository.GetNotificationsForPayment(uow, type, payment.Id);
				var now = DateTime.Now;
				
				notification ??= new FastPaymentNotification
				{
					Time = now,
					Payment = payment,
					Type = type
				};

				notification.SuccessfullyNotified = notified;
				notification.LastTryTime = now;
				uow.Save(notification);
				uow.Commit();
			}
		}
	}
}
