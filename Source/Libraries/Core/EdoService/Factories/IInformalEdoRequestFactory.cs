using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace EdoService.Library.Factories
{
	public interface IInformalEdoRequestFactory
	{
		/// <summary>
		/// Может ли фабрика создать заявку по переданному типу документа
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		bool CanCreateFor(OrderDocumentType type);

		/// <summary>
		/// Создать заявку
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		InformalEdoRequest Create(Order order);
	}
}
