using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Edo;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	/// <summary>
	/// Интерфейс получения данных по операторам ЭДО
	/// </summary>
	public interface ICustomerAppEdoOperatorRepository
	{
		/// <summary>
		/// Получение списка всех операторов ЭДО
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <returns>Список операторов ЭДО</returns>
		IEnumerable<EdoOperatorDto> GetAllEdoOperators(IUnitOfWork uow);
	}
}
