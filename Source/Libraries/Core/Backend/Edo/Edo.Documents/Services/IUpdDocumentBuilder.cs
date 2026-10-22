using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Documents.Services
{
	public interface IUpdDocumentBuilder
	{
		/// <summary>
		/// Создает и заполняет документ УПД на основе данных из задачи ЭДО
		/// </summary>
		/// <param name="documentEdoTask">Задача ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат создания документа УПД</returns>
		Task BuildForOwnNeedUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
		/// <summary>
		/// Удаляет существующий документ УПД задачи и создает его заново
		/// с учетом текущих кодов в строках задачи
		/// </summary>
		/// <param name="documentEdoTask">Задача ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task RebuildForOwnNeedUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
		UpdInventPosition CreateInventPosition(OrderEntity order, OrderItemEntity orderItem);
		EdoUpdDocument GetUpdDocumentWithoutCodes(DocumentEdoTask documentEdoTask);
	}
}
