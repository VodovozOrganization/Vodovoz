using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdo.Contracts.Xml.Container;

namespace TaxcomEdoApi.Library.Services.Interfaces;

public interface IEdoDocflowService
{
	/// <summary>
	/// Отправка контейнера Такском с электронным документом или служебным сообщением.
	/// Каждый отправляемый контейнер Такском должен содержать файлы meta.xml и card.xml, а также электронный документ или служебное сообщение
	/// </summary>
	/// <param name="container">Контйенер с документами</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены операции</param>
	/// <returns></returns>
	Task<TaxcomResponse> SendMessageAsync(byte[] container, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение с сервера системы Такском-Доклайнз списка входящих или исходящих транзакций для всех получаемых и отправляемых электронных документов.
	/// </summary>
	/// <param name="parameters">Параметры фильтрации нужных данных</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены операции</param>
	/// <returns></returns>
	Task<TaxcomResponse<ContainerDescription>> GetMessageListAsync(
		GetMessageListParameters parameters, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение с сервера системы Такском-Доклайнз списка документооборотов со статусами.
	/// </summary>
	/// <param name="parameters">Параметры выборки <see cref="GetDocFlowsUpdatesParameters"/></param>
	/// <param name="certificateData">Данные сертификата</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse<ContainerDescription>> GetListAsync(
		GetDocFlowsUpdatesParameters parameters, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение контейнера Такском с документом или служебным сообщением с сервера системы Такском-Доклайнз
	/// </summary>
	/// <param name="docFlowId">Id документооборота</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены операции</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoDocFlowUpdates>> GetMessageAsync(string docFlowId, byte[] certificateData, CancellationToken cancellationToken = default);
}
