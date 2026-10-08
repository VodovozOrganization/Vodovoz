using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents.Services
{
	/// <summary>
	/// Проверяет состав транспортных кодов перед отправкой УПД заказа.
	/// </summary>
	public interface IUpdTransportCodeService
	{
		/// <summary>
		/// Снимает связи с транспортниками, состав которых не соответствует позициям УПД.
		/// </summary>
		Task DetachIncompleteTransportCodesAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
	}
}
