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
		/// Создает контрагента в Mindbox
		/// </summary>
		Task<MindboxOperationResult> CreateCounterpartyAsync(
			MindboxCounterpartyRequest request,
			Guid transactionId,
			CancellationToken cancellationToken);

		/// <summary>
		/// Загружает клиентов в Mindbox через операцию массового импорта
		/// </summary>
		Task<MindboxOperationResult> ImportCustomersAsync(
			byte[] csvContent,
			Guid transactionId,
			CancellationToken cancellationToken);
	}
}
