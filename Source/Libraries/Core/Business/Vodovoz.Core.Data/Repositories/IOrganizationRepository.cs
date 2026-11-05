using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
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

		/// <summary>
		/// Получить организацию по ID в сессии переданного UoW.
		/// Ленивые поля организации доступны, пока этот UoW открыт
		/// </summary>
		/// <param name="uow">UoW, в сессии которого загружается организация</param>
		/// <param name="id">ID</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Организация</returns>
		Task<OrganizationEntity> GetOrganizationByIdAsync(IUnitOfWork uow, int id, CancellationToken cancellationToken);
	}
}
