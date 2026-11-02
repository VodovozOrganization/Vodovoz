using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdoService.Library.Services
{
	/// <summary>
	/// Хранилище неизменяемых PDF счетов для отправки через ЭДО.
	/// </summary>
	public interface IBillEdoDocumentFileStorageService
	{
		/// <summary>
		/// Сохраняет PDF счета.
		/// </summary>
		/// <param name="documentId">Идентификатор документа в снимке счета.</param>
		/// <param name="pdf">Содержимое PDF.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача сохранения файла.</returns>
		Task SaveAsync(Guid documentId, byte[] pdf, CancellationToken cancellationToken);

		/// <summary>
		/// Загружает сохраненный PDF счета.
		/// </summary>
		/// <param name="documentId">Идентификатор документа в снимке счета.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Содержимое PDF.</returns>
		Task<byte[]> GetAsync(Guid documentId, CancellationToken cancellationToken);

		/// <summary>
		/// Удаляет PDF счета, для которого не удалось сохранить заявку.
		/// </summary>
		/// <param name="documentId">Идентификатор документа в снимке счета.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача удаления файла.</returns>
		Task DeleteAsync(Guid documentId, CancellationToken cancellationToken);
	}
}
