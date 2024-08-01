using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.Application.FileStorage
{
	public interface IS3FileStorageService
	{
		/// <summary>
		/// Создание файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="name">Имя файла</param>
		/// <param name="content">Содержимое файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> CreateFileAsync(string bucketName, string name, string content, CancellationToken cancellationToken);
		//Task<Result> CreateFile(string name, Stream inputStream);

		/// <summary>
		/// Получение файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="name">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> GetFileAsync(string bucketName, string name, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="name">Имя файла</param>
		/// <param name="content">Содержимое файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> UpdateFileAsync(string bucketName, string name, string content, CancellationToken cancellationToken);
		//Task<Result> UpdateFile(string name, Stream inputStream);

		/// <summary>
		/// Удаление файла
		/// </summary>
		/// <param name="bucketName">Имя бакета</param>
		/// <param name="name">Имя файла</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> DeleteFileAsync(string bucketName, string name, CancellationToken cancellationToken);
	}
}
