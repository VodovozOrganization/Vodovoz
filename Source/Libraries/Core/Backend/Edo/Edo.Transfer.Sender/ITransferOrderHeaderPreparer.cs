using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.Transfer.Sender
{
	/// <summary>
	/// Подготавливает шапку заказа трансфера и его документ-счетчик.
	/// </summary>
	public interface ITransferOrderHeaderPreparer
	{
		/// <summary>
		/// Создает заказ трансфера с датой самого раннего связанного заказа.
		/// </summary>
		/// <param name="transferEdoTask">Подготавливаемая задача трансфера.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Результат создания заказа трансфера.</returns>
		Task<Result<TransferOrder>> PrepareAsync(
			TransferEdoTask transferEdoTask,
			CancellationToken cancellationToken);
	}
}
