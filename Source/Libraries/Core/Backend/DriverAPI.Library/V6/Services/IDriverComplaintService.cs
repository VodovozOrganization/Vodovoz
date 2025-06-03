using DriverApi.Contracts.V6;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Сервис для работы с рекламациями водителей
	/// </summary>
	public interface IDriverComplaintService
	{
		/// <summary>
		/// Получить закрепленные причины рекламаций
		/// </summary>
		/// <returns>Результат выполнения с перечислением причин рекламаций</returns>
		Result<IEnumerable<DriverComplaintReasonDto>> GetPinnedComplaintReasons();
	}
}
