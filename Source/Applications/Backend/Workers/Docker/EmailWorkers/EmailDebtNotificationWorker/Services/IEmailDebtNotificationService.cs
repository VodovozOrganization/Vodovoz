namespace EmailDebtNotificationWorker.Services
{
	public interface IEmailDebtNotificationService
	{
		/// <summary>
		/// Обработать очередь отправки писем
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task ProcessEmailQueueAsync(CancellationToken cancellationToken);
	}
}
