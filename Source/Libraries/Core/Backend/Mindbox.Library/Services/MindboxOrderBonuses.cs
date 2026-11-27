namespace Mindbox.Library.Services
{
	/// <summary>
	/// Бонусы по заказу для карточки МП
	/// </summary>
	public sealed class MindboxOrderBonuses
	{
		public bool IsAvailable { get; set; }
		public int? WrittenOff { get; set; }
		public int? Earned { get; set; }
		public MindboxOrderBonusesEarnStatus? EarnStatus { get; set; }

		public static MindboxOrderBonuses Unavailable()
		{
			return new MindboxOrderBonuses { IsAvailable = false };
		}
	}
}
