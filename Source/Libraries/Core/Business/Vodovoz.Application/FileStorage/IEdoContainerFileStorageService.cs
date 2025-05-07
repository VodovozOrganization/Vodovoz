using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.FileStorage
{
	public interface IEdoContainerFileStorageService
	{
		/// <summary>
		/// Создание архива
		/// </summary>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> CreateContainerAsync(EdoContainer entity, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Получение архива
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<Stream>> GetContainerAsync(EdoContainer entity, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление архива
		/// </summary>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> UpdateContainerAsync(EdoContainer entity, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление архива
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> DeleteContainerAsync(EdoContainer entity, CancellationToken cancellationToken);
	}
}
