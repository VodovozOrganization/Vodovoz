using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;

namespace Edo.Common.Services
{
	/// <summary>
	/// Сервис проверки регистрации клиентов в Честном Знаке
	/// </summary>
	public interface IClientsTrueMarkRegistrationCheckService
	{
		/// <summary>
		/// Возвращает статус регистрации клиента в Честном Знаке по списку ИНН
		/// </summary>
		/// <param name="inns">Список строк ИНН</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IDictionary<string, Result<RegistrationInChestnyZnakStatus>>> GetTrueMarkRegistrationsStatuses(IEnumerable<string> inns, CancellationToken cancellationToken = default);

		/// <summary>
		/// Возвращает статус регистрации клиента в Честном Знаке по ИНН
		/// </summary>
		/// <param name="inn">ИНН</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<RegistrationInChestnyZnakStatus>> GetTrueMarkRegistrationStatus(string inn, CancellationToken cancellationToken);
	}
}
