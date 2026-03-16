using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public interface ITrueMarkApiClient
	{
		/// <summary>
		/// Максимальное количество ИНН, которое можно передать в одном запросе для проверки регистрации участников в системе маркировки.
		/// Если количество ИНН превышает это значение, необходимо разбить запрос на несколько частей и выполнить несколько запросов к API.
		/// </summary>
		int ParticipantsCheckMaxCount { get; }

		/// <summary>
		/// Возвращает информацию о документе в ЧЗ по его идентификатору
		/// </summary>
		/// <param name="documentId">Идентификатор документа</param>
		/// <param name="inn">ИНН организации</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<TrueMarkDocumentInfo> GetDocumentInfo(Guid documentId, string inn, CancellationToken cancellationToken);

		Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken);

		/// <summary>
		/// Проверяет статус регистрации участников в системе маркировки для указанных ИНН. Возвращает список с результатами проверки для каждого ИНН
		/// </summary>
		/// <param name="inns">Список строк ИНН</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IEnumerable<ParticipantRegistrationDto>> GetParticipantsRegistrations(IEnumerable<string> inns, CancellationToken cancellationToken);
		Task<ProductInstancesInfoResponse> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes, CancellationToken cancellationToken);

		/// <summary>
		/// Отправка документа вывода из оборота (индивидуальный учет)
		/// </summary>
		/// <param name="document">Документ</param>
		/// <param name="inn">ИНН организации</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Номер созданного документа</returns>
		Task<string> SendIndividualAccountingWithdrawalDocument(string document, string inn, CancellationToken cancellationToken);
	}
}
