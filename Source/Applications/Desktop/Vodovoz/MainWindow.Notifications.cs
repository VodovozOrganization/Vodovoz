using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using System;
using Vodovoz.Controllers;
using Vodovoz.ViewModels.Complaints;
using Vodovoz;
using System.Linq;
using Vodovoz.Infrastructure;
using Vodovoz.Extensions;

public partial class MainWindow
{
	#region Уведомления об отправленных перемещениях и о наличии рекламаций

	#region Методы для уведомления об отправленных перемещениях для подразделения
	private void OnBtnUpdateNotificationClicked(object sender, EventArgs e)
	{
		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			var movementsNotification = _movementsNotificationsController.GetNotificationMessage(uow);
			UpdateSentMovementsNotification(movementsNotification);
		}

		if(!_hideComplaintsNotifications)
		{
			var complaintsNotifications = GetComplaintNotificationDetails();
			UpdateSendedComplaintsNotification(complaintsNotifications);
		}
	}

	private void UpdateSentMovementsNotification((bool Alert, string Message) notificationDetails)
	{
		var message = notificationDetails.Alert
			? $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">{notificationDetails.Message}</span>"
			: notificationDetails.Message;

		lblMovementsNotification.Markup = message;
	}
	#endregion

	#region Методы для уведомления о наличии незакрытых рекламаций без комментариев для подразделения
	private void UpdateSendedComplaintsNotification(SendedComplaintNotificationDetails notificationDetails)
	{
		lblComplaintsNotification.Markup = notificationDetails.NotificationMessage;
		hboxComplaintsNotification.Visible = notificationDetails.NeedNotify;
	}

	private SendedComplaintNotificationDetails GetComplaintNotificationDetails()
	{
		SendedComplaintNotificationDetails notificationDetails;

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			notificationDetails = _complaintNotificationController.GetNotificationDetails(uow);
		}

		return notificationDetails;
	}

	private void OnBtnOpenComplaintClicked(object sender, EventArgs e)
	{
		var notificationDetails = GetComplaintNotificationDetails();

		UpdateSendedComplaintsNotification(notificationDetails);

		if(notificationDetails.SendedComplaintsCount > 0)
		{
			NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(
				null,
				EntityUoWBuilder.ForOpen(notificationDetails.SendedComplaintsIds.Min()),
				OpenPageOptions.None);
		}
	}

	#endregion

	#endregion
}
