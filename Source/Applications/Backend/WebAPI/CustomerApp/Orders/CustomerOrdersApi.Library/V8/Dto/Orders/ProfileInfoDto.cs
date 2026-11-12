namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Ответ метода GetProfileInfo
	/// </summary>
	public sealed class ProfileInfoDto
	{
		/// <summary>
		/// Бонусный счёт
		/// </summary>
		public ProfileBonusesDto Bonuses { get; set; }
	}
}
