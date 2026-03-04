using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Organizations;

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
		
		/// <summary>
		/// Получить организацию по ID
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>Организация</returns>
		OrganizationEntity GetOrganizationById(int id);
		
		/// <summary>
		/// Получить организацию по ID асинхронно
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>Организация</returns>
		Task<OrganizationEntity> GetOrganizationByIdAsync(int id);
	}
}
