using System;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <summary>
	/// Кэш сервис проверок в корзине
	/// </summary>
	public interface ICheckUserBasketCacheService
	{
		/// <summary>
		/// Кэширование данных проверок в корзине
		/// </summary>
		/// <param name="data">Кэшируемые данные</param>
		/// <returns><c>true</c> - удачно, <c>false</c> - неудачно</returns>
		Task<bool> TryCacheVerificationAsync(CheckUsersBasketCachedValue data);
		/// <summary>
		/// Получение закэшированных данных
		/// </summary>
		/// <param name="checkId">Идентификатор провекри</param>
		/// <returns></returns>
		Task<CheckUsersBasketCachedValue> GetCachedVerificationAsync(Guid? checkId);
	}
}
