using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IOrganizationRepository
	{
		/// <summary>
		/// Получить список email адресов организаций для рассылки
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		IEnumerable<string> GetEmailsForMailing();
	}
}
