using QS.DomainModel.Entity;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.FileStorage
{
	public interface IEntityPhotoStorageService<TEntity>
		where TEntity : IDomainObject, IHasPhoto
	{
		/// <summary>
		/// Создание файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> CreatePhotoAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Получение файла
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<Stream>> GetPhotoAsync(TEntity entity, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление файла
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> UpdatePhotoAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление файла
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> DeletePhotoAsync(TEntity entity, CancellationToken cancellationToken);
	}
}
