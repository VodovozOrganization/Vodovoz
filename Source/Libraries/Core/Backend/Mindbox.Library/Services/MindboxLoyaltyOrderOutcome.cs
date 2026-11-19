namespace Mindbox.Library.Services
{
	/// <summary>
	/// Результат создания заказа с бонусами в Mindbox
	/// </summary>
	public enum MindboxLoyaltyOrderOutcome
	{
		Success = 0,
		BonusesChanged = 1,
		ServiceUnavailable = 2
	}
}
