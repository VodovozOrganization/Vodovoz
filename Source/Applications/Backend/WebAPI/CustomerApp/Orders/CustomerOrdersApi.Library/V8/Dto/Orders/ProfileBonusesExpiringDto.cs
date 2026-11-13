using System;

namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Ближайшее сгорание бонусов
	/// </summary>
	public sealed class ProfileBonusesExpiringDto
	{
		/// <summary>
		/// Сколько бонусов сгорит
		/// </summary>
		public int Amount { get; set; }

		/// <summary>
		/// Дата сгорания
		/// </summary>
		public DateTime Date { get; set; }
	}
}
