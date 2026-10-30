using System;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Данные интервала доставки
	/// </summary>
	public class DeliveryScheduleData
	{
		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// От
		/// </summary>
		public TimeSpan From { get; set; }

		/// <summary>
		/// До
		/// </summary>
		public TimeSpan To { get; set; }

		public static DeliveryScheduleData Create(int id, TimeSpan from, TimeSpan to) =>
			new DeliveryScheduleData
			{
				Id = id,
				From = from,
				To = to
			};
	}
}
