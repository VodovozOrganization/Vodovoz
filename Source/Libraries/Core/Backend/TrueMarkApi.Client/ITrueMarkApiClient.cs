using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public interface ITrueMarkApiClient
	{
		Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken);
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
