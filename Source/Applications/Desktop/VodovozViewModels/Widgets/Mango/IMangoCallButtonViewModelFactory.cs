using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Widgets.Mango
{
	/// <summary>
	/// Фабрика вью-моделей кнопки исходящего звонка через Манго
	/// </summary>
	public interface IMangoCallButtonViewModelFactory
	{
		/// <summary>
		/// Создаёт вью-модель кнопки звонка на добавочный номер водителя маршрутного листа
		/// </summary>
		/// <param name="uow">Единица работы</param>
		/// <param name="routeList">Маршрутный лист</param>
		MangoCallButtonViewModel CreateForRouteListDriver(IUnitOfWork uow, RouteList routeList);

		/// <summary>
		/// Создаёт вью-модель кнопки звонка на добавочный номер водителя, за которым закреплён заказ
		/// </summary>
		/// <param name="uow">Единица работы</param>
		/// <param name="order">Заказ</param>
		MangoCallButtonViewModel CreateForOrderDriver(IUnitOfWork uow, Order order);

		/// <summary>
		/// Пересчитывает доступность звонка на добавочный номер водителя маршрутного листа.
		/// Нужен, если водитель маршрутного листа мог измениться после создания вью-модели
		/// </summary>
		/// <param name="viewModel">Вью-модель кнопки звонка</param>
		/// <param name="uow">Единица работы</param>
		/// <param name="routeList">Маршрутный лист</param>
		void UpdateForRouteListDriver(MangoCallButtonViewModel viewModel, IUnitOfWork uow, RouteList routeList);
	}
}
