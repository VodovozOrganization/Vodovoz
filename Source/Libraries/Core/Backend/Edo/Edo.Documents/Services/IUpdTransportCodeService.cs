using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Documents.Services
{
	/// <summary>
	/// Управляет связями упаковок при формировании УПД заказа.
	/// </summary>
	public interface IUpdTransportCodeService
	{
		/// <summary>
		/// Разбирает упаковку экземплярного кода, сохраняя соседние групповые упаковки.
		/// </summary>
		/// <param name="code">Код разбираемой упаковки или отдельной бутылки.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача сохранения изменённых связей упаковок.</returns>
		Task DisaggregateCodeAsync(TrueMarkAnyCode code, CancellationToken cancellationToken);

		/// <summary>
		/// Снимает связи с транспортниками, состав которых не соответствует позициям УПД.
		///	<param name="documentEdoTask">Задача создания УПД.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача проверки и сохранения изменённых кодов.</returns>
		/// </summary>
		Task DetachIncompleteTransportCodesAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
	}
}
