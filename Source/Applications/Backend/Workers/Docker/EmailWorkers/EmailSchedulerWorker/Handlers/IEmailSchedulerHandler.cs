using Mailganer.Api.Client.Dto;

namespace EmailSchedulerWorker.Handlers
{
	public interface IEmailSchedulerHandler
	{
		/// <summary>
		/// Обработать новое email сообщение
		/// </summary>
		/// <param name="emailMessage"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task HandleNew(EmailMessage emailMessage, int clientId, IEnumerable<int> orderIds, CancellationToken cancellationToken);
	}
}
