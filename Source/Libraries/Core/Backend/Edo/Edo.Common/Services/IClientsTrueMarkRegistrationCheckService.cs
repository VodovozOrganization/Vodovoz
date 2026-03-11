using System.Threading;
using System.Threading.Tasks;
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
		/// Возвращает статус регистрации клиента в Честном Знаке по ИНН
		/// </summary>
		/// <param name="inn">ИНН</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<RegistrationInChestnyZnakStatus>> GetTrueMarkRegistrationStatus(string inn, CancellationToken cancellationToken);
	}
}
