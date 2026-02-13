using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Employees
{
	/// <summary>
	/// Cервис для работы с пользователями внешних приложений
	/// </summary>
	public interface IExternalApplicationUserService
	{
		/// <summary>
		/// Получение информации о пользователе внешнего приложения по логину и типу приложения
		/// </summary>
		/// <param name="username"></param>
		/// <param name="externalApplicationType"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<Result<Employee>> GetExternalUserEmployee(
			string username,
			ExternalApplicationType externalApplicationType,
			CancellationToken cancellationToken);
	}
}
