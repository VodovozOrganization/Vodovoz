using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents.Services
{
	public interface IUpdTransportCodeService
	{
		/// <summary>
		/// Снимает связи с транспортниками, состав которых не соответствует позициям УПД.
		/// </summary>
		/// <param name="documentEdoTask">Задача создания УПД.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача проверки и сохранения изменённых кодов.</returns>
		Task DetachIncompleteTransportCodesAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
	}
}
