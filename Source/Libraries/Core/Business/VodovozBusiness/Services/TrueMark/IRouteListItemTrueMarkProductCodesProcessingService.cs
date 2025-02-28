using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Интерфейс для обработки кодов Честного Знака в строках маршрутного листа
	/// </summary>
	public interface IRouteListItemTrueMarkProductCodesProcessingService
	{
		/// <summary>
		/// Добавить код Честного Знака к строке маршрутного листа
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="vodovozOrderItemId">ID строки заказа</param>
		/// <param name="trueMarkWaterIdentificationCode">Код Честного Знака</param>
		/// <param name="status">Статус кода продукта</param>
		void AddSingleTrueMarkCodeToRouteListItem(IUnitOfWork uow, RouteListItem routeListAddress, int vodovozOrderItemId, TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, SourceProductCodeStatus status);

		/// <summary>
		/// Добавить код Честного Знака к строке маршрутного листа с предварительной проверкой кода
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="vodovozOrderItem">Строка заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <param name="status">Статус кода продукта</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <param name="isCheckForCodeChange">Выполняется операция замены коды. Если true, то не выполняется проверка кол-ва добавленных кодов</param>
		/// <returns>Результат операции</returns>
		Task<Result> AddTrueMarkCodeToRouteListItemWithCodeChecking(IUnitOfWork uow, RouteListItem routeListAddress, OrderItem vodovozOrderItem, string scannedCode, SourceProductCodeStatus status, CancellationToken cancellationToken, bool isCheckForCodeChange = false);

		/// <summary>
		/// Изменить код Честного Знака в строке маршрутного листа с проверкой кода
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="vodovozOrderItem">Строка заказа</param>
		/// <param name="oldScannedCode">Старый сканированный код</param>
		/// <param name="newScannedCode">Новый сканированный код</param>
		/// <param name="status">Статус кода продукта</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат операции</returns>
		Task<Result> ChangeTrueMarkCodeToRouteListItemWithCodeChecking(IUnitOfWork uow, RouteListItem routeListAddress, OrderItem vodovozOrderItem, string oldScannedCode, string newScannedCode, SourceProductCodeStatus status, CancellationToken cancellationToken);

		/// <summary>
		/// Удалить код Честного Знака из строки маршрутного листа
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="vodovozOrderItemId">Номер строки заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <returns>Результат операции</returns>
		Task<Result> RemoveTrueMarkCodeFromRouteListItem(IUnitOfWork uow, RouteListItem routeListAddress, int vodovozOrderItemId, string scannedCode);
	}
}
