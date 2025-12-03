namespace EmailSchedulerWorker.Services
{
	public partial class EmailSchedulingService
	{
		public interface IEmailSchedulingService
		{
			/// <summary>
			/// Обработать очередь отправки писем
			/// </summary>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			Task ProcessEmailQueueAsync(CancellationToken cancellationToken);

			/// <summary>
			/// Получить количество ожидающих писем в очереди
			/// </summary>
			/// <returns></returns>
			Task<int> GetPendingEmailsCountAsync();
		}
	}
}
