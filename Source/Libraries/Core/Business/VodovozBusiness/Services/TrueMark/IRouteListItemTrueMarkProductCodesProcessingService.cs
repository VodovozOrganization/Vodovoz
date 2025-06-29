using QS.DomainModel.UoW;
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
		void AddTrueMarkCodeToRouteListItem(IUnitOfWork uow, RouteListItemEntity routeListAddress, int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, SourceProductCodeStatus status, ProductCodeProblem problem);

		Result ValidateTrueMarkCodeIsInAggregationCode(TrueMarkAnyCode trueMarkCodeResult);

		/// <summary>
		/// Добавить список кодов к строке маршрутного листа. Статусы кодов не проверяются
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="routeListAddress">Строка маршрутного листа</param>
		/// <param name="orderSaleItemId">Строка заказа</param>
		/// <param name="trueMarkAnyCode">Код ЧЗ</param>
		/// <param name="status">Статус кода продукта</param>
		/// <param name="problem">Тип проблемы кода ЧЗ</param>
		/// <returns>Результат операции</returns>
		Task AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(IUnitOfWork uow, RouteListItemEntity routeListAddress, int orderSaleItemId,
			TrueMarkAnyCode trueMarkAnyCode, SourceProductCodeStatus status, ProductCodeProblem problem, CancellationToken cancellationToken = default);

		/// <summary>
		/// Проверяет может ли код ЧЗ для промежуточного хранения быть добавлен к строке МЛ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="stagingTrueMarkCode">Код ЧЗ для промежуточного хранения</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат проверки</returns>
		Task<Result> IsTrueMarkCodeCanBeAddedToRouteListItem(IUnitOfWork uow, StagingTrueMarkCode stagingTrueMarkCode, OrderItem orderItem, CancellationToken cancellationToken);
	}
}
