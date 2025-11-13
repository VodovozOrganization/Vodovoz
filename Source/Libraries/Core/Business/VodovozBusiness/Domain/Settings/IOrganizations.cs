using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Интерфейс для настроек выбора организации для заказа
	/// </summary>
	public interface IOrganizations
	{
		/// <summary>
		/// Список организаций
		/// </summary>
		IObservableList<Organization> Organizations { get; }
	}
}
