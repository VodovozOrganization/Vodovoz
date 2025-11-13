using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Интерфейс получения организации для заказа из настроек
	/// </summary>
	public interface IOrganizationForOrderFromSet
	{
		/// <summary>
		/// Получение организации для заказа из множества организаций
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationsSet">Множество организаций</param>
		/// <param name="canReturnNull">Может возвращать null</param>
		/// <returns>Организация</returns>
		Organization GetOrganizationForOrderFromSet(TimeSpan requestTime, IOrganizations organizationsSet, bool canReturnNull = false);

		/// <summary>
		/// Получение словаря организаций с интервалом времени, в который они подбираются для заказа
		/// </summary>
		/// <param name="organizations">Список организаций</param>
		/// <returns>Словарь организаций</returns>
		IReadOnlyDictionary<int, (TimeSpan From, TimeSpan To)> GetChoiceTimeOrganizationFromSet(
			IEnumerable<INamedDomainObject> organizations);
	}
}
