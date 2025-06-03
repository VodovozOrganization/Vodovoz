using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Domain.Logistic.Specifications
{
	/// <summary>
	/// Спецификации для фильтрации графиков доставки
	/// </summary>
	public static class DeliveryScheduleSpecifications
	{
		/// <summary>
		/// Спецификация для фильтрации графиков доставки по идентификатору
		/// </summary>
		/// <param name="deliveryScheduleId"></param>
		/// <returns></returns>
		public static ExpressionSpecification<DeliverySchedule> CreateForId(int deliveryScheduleId)
			=> new ExpressionSpecification<DeliverySchedule>(x => x.Id == deliveryScheduleId);
	}
}
