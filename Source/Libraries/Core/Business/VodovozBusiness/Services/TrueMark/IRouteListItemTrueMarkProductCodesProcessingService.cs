using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

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
		/// <param name="problem">Тип проблемы кода ЧЗ</param>
		void AddTrueMarkCodeToRouteListItem(IUnitOfWork uow, RouteListItem routeListAddress, int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, SourceProductCodeStatus status, ProductCodeProblem problem);

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
		Task<Result> AddTrueMarkCodeToRouteListItemWithCodeChecking(IUnitOfWork uow, RouteListItem routeListAddress, OrderItem vodovozOrderItem, string scannedCode, SourceProductCodeStatus status, CancellationToken cancellationToken, bool isCheckForCodeChange = false, bool skipCodeIntroducedAndHasCorrectInnCheck = false);
		Task<Result> IsTrueMarkCodeCanBeAddedToRouteListItem(IUnitOfWork uow, TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, RouteListItem routeListAddress, OrderItem orderItem, CancellationToken cancellationToken, bool isCheckForCodeChange = false, bool skipCodeIntroducedAndHasCorrectInnCheck = false);
		Result ValidateTrueMarkCodeIsInAggregationCode(TrueMarkAnyCode trueMarkCodeResult);

		/// <summary>
		/// Добавить список кодов к строке маршрутного листа. Статусы кодов не проверяются
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="orderSaleItemId">Строка заказа</param>
		/// <param name="scannedCodes">Списко отсканированных кодов</param>
		/// <param name="status">Статус кода продукта</param>
		/// <param name="problem">Тип проблемы кода ЧЗ</param>
		/// <returns>Результат операции</returns>
		Task<Result> AddProductCodesToRouteListItemNoCodeStatusCheck(IUnitOfWork uow, RouteListItemEntity routeListAddress, int orderSaleItemId,
			IEnumerable<string> scannedCodes, SourceProductCodeStatus status, ProductCodeProblem problem);
	}
}
