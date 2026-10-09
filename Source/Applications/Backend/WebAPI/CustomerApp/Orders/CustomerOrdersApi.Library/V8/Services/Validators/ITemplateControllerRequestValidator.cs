using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V8.Services.Validators
{
	/// <summary>
	/// Валидатор запроса получения настроек доставки автозаказа
	/// </summary>
	public interface ITemplateControllerRequestValidator
	{
		/// <summary>
		/// Проверка корректности данных запроса получения настроек доставки автозаказа <see cref="GetOrderTemplateScheduleOptionsRequest"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="request">Данные запроса</param>
		/// <returns>Либо успешный результат, либо с ошибкой <see cref="Result"/></returns>
		Result Validate(
			IUnitOfWork uow,
			GetOrderTemplateScheduleOptionsRequest request
		);
		
		/// <summary>
		/// Проверка корректности данных запроса проверки параметров автозаказа <see cref="CheckOrderTemplateRequest"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="request">Данные запроса</param>
		/// <returns>Либо успешный результат, либо с ошибкой <see cref="Result"/></returns>
		Result Validate(
			IUnitOfWork uow,
			CheckOrderTemplateRequest request
		);
	}
}
