using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Client.Specifications
{
	/// <summary>
	/// Спецификации для фильтрации точек доставки
	/// </summary>
	public static class DeliveryPointSpecifications
	{
		/// <summary>
		/// Создает спецификацию для фильтрации точек доставки по идентификатору
		/// </summary>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <returns></returns>
		public static ExpressionSpecification<DeliveryPoint> CreateForId(int deliveryPointId)
			=> new ExpressionSpecification<DeliveryPoint>(x => x.Id == deliveryPointId);

		/// <summary>
		/// Создает спецификацию для фильтрации точек доставки по идентификатору контрагента
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <returns></returns>
		public static ExpressionSpecification<DeliveryPoint> CreateForCounterpartyId(int counterpartyId)
			=> new ExpressionSpecification<DeliveryPoint>(x => x.Counterparty.Id == counterpartyId);

		/// <summary>
		/// Создает спецификацию для фильтрации точек доставки, которые активны
		/// </summary>
		/// <returns></returns>
		public static ExpressionSpecification<DeliveryPoint> CreateForIsActive()
			=> new ExpressionSpecification<DeliveryPoint>(x => x.IsActive);

		/// <summary>
		/// Создает спецификацию для фильтрации точек доставки, доступных для доставки
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <returns></returns>
		public static ExpressionSpecification<DeliveryPoint> CreateForAvailableToDelivery(int counterpartyId, int deliveryPointId)
			=> CreateForId(deliveryPointId)
				& CreateForCounterpartyId(counterpartyId)
				& CreateForIsActive();
	}
}
