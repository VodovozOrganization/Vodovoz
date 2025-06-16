using QS.DomainModel.Entity;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.FileStorage
{
	public interface IEntityFileStorageService<TEntity>
		where TEntity : IDomainObject
	{
		/// <summary>
		/// Создание файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> CreateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Получение файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<Stream>> GetFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> UpdateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> DeleteFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken);
	}
}
