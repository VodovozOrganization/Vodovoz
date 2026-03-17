using Mango.Business.Models;
using Mango.Contracts.V1.Response;

namespace Mango.Business.Interfaces
{
	public interface IMangoReferenceDataBuilder
	{
		/// <summary>
		/// Подготовить фильтрующие данные по существующем группам
		/// </summary>
		/// <param name="response">Ответ с группами</param>
		/// <returns></returns>
		MangoReferenceData Build(GroupsResponse response);
	}
}
