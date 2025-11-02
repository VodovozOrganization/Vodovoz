using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.FileStorage
{
	public interface IS3FileStorageService
	{
		/// <summary>
		/// Создание файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> CreateFileAsync(string bucketName, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Получение файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<Stream>> GetFileAsync(string bucketName, string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="inputStream">Входящий поток данных</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> UpdateFileAsync(string bucketName, string fileName, Stream inputStream, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> DeleteFileAsync(string bucketName, string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Проверка существования файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<bool>> FileExistsAsync(string bucketName, string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Получение всех файлов в бакете
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<IEnumerable<string>>> GetAllObjectsFileNamesInBucketAsync(string bucketName, CancellationToken cancellationToken);
	}
}
