using System.Threading;
using System.Threading.Tasks;

namespace Edo.Transport
{
	/// <summary>
	/// Публикует события, необходимые после переноса кодов маркировки из отмененного заказа.
	/// </summary>
	public interface ICancelledOrderTrueMarkCodesTransferEventPublisher
	{
		/// <summary>
		/// Опубликовать событие создания клиентской ЭДО-заявки по переносу кодов маркировки.
		/// </summary>
		/// <param name="requestId">Номер клиентской ЭДО-заявки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task PublishEdoRequestCreated(int requestId, CancellationToken cancellationToken = default);
	}
}
