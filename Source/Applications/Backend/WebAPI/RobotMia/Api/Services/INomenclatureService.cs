using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Services
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
