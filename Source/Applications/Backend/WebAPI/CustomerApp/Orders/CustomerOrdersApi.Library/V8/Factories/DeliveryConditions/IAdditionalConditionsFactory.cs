using System.Collections.Generic;
using CustomerOrdersApi.Library.V8.Dto.AdditionalConditions;

namespace CustomerOrdersApi.Library.V8.Factories.DeliveryConditions
{
	/// <summary>
	/// Фабрика доп условий
	/// </summary>
	public interface IAdditionalConditionsFactory
	{
		/// <summary>
		/// Создание условий для новых клиентов
		/// </summary>
		/// <returns>Список условий</returns>
		IEnumerable<AdditionalCondition> CreateForNewClient();
		/// <summary>
		/// Создание условий для постоянных клиентов
		/// </summary>
		/// <returns>Список условий</returns>
		IEnumerable<AdditionalCondition> CreateDefault();
	}
}
