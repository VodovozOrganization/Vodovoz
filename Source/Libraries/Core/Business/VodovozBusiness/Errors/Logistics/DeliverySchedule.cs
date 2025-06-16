using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Logistics
{
	public static class DeliverySchedule
	{
		/// <summary>
		/// Ошибка, возникающая при отсутствии графика доставки
		/// </summary>
		public static Error NotFound => new Error(
			typeof(DeliverySchedule),
			nameof(NotFound),
			"График доставки не найден");
	}
}
