using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.Application.FileStorage
{
	public interface IFileStorageService
	{
		/// <summary>
		/// Создание файла
		/// </summary>
		/// <param name="name">Имя файла</param>
		/// <param name="content">Содержимое файла</param>
		/// <returns></returns>
		Task<Result> CreateFileAsync(string name, string content, CancellationToken cancellationToken);
		//Result CreateFile(string name, Stream inputStream);

		/// <summary>
		/// Получение файла
		/// </summary>
		/// <param name="name">Имя файла</param>
		/// <returns></returns>
		Task<Result> GetFileAsync(string name, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление файла
		/// </summary>
		/// <param name="name">Имя файла</param>
		/// <param name="content">Содержимое файла</param>
		/// <returns></returns>
		Task<Result> UpdateFileAsync(string name, string content, CancellationToken cancellationToken);
		//Result UpdateFile(string name, Stream inputStream);

		/// <summary>
		/// Удаление файла
		/// </summary>
		/// <param name="name">Имя файла</param>
		/// <returns></returns>
		Task<Result> DeleteFileAsync(string name, CancellationToken cancellationToken);
	}
}
