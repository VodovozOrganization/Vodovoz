using RobotMiaApi.Contracts.Responses.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RobotMiaApi.Services
{
	/// <summary>
	/// Сервис для работы с номенклатурами
	/// </summary>
	public interface INomenclatureService
	{
		/// <summary>
		/// Получение номенклатур
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<NomenclatureDto>> GetNomenclatures();

		/// <summary>
		/// Получение номенклатуры неустойки
		/// </summary>
		/// <returns></returns>
		Task<NomenclatureDto> GetForfeitNomenclature();
	}
}
