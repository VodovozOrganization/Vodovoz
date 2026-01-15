namespace EmailDebtNotificationWorker.Services
{
	public interface IEmailDebtNotificationService
	{
		/// <summary>
		/// Запланировать рассылку писем о задолженности
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task ScheduleDebtNotificationsAsync(CancellationToken cancellationToken);
	}
}
