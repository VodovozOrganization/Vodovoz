namespace EmailSchedulerWorker.Services
{
	public interface IRateLimiterService
	{
		/// <summary>
		/// Можно ли отправить письмо сейчас
		/// </summary>
		/// <returns></returns>
		Task<bool> CanSendAsync();

		/// <summary>
		/// Зарегистрировать отправку письма
		/// </summary>
		/// <returns></returns>
		Task RegisterSendAsync();

		/// <summary>
		/// Задержать выполнение, если превышен лимит отправки писем
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task DelayIfNeededAsync(CancellationToken cancellationToken);
	}
}
