using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Mindbox.Contracts;

namespace Vodovoz.Mindbox.Services
{
	/// <summary>
	/// Клиент API Mindbox
	/// </summary>
	public interface IMindboxClient
	{
		/// <summary>
		/// Создает или обновляет клиента в Mindbox
		/// </summary>
		Task<MindboxOperationResult> CreateCustomerAsync(
			MindboxCustomerRequest request,
			Guid transactionId,
			CancellationToken cancellationToken);
	}
}
