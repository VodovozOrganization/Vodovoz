using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

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
		Task BuildUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken);
	}
}
