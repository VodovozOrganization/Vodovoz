using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Контракт сервиса по обновлению контракта/договора заказа
	/// </summary>
	public interface IOrderContractUpdater
	{
		/// <summary>
		/// Обновление договора в заказе
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <param name="onPaymentTypeChanged"><c>true</c> менялся тип оплаты, <c>false</c> не менялся тип оплаты</param>
		void UpdateContract(IUnitOfWork uow, Order order, bool onPaymentTypeChanged = false);
		/// <summary>
		/// Принудительное обновление договора
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <param name="organization">Организация, под которую нужен договор</param>
		void ForceUpdateContract(IUnitOfWork uow, Order order, Organization organization = null);
		/// <summary>
		/// Обновление или создание нового договора
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <param name="organization">Организация, под которую нужен договор</param>
		void UpdateOrCreateContract(IUnitOfWork uow, Order order, Organization organization = null);
	}
}
