namespace Vodovoz.Core.Domain.Clients
{
	public static class CompanyConstants
	{
		/// <summary>
		/// Длина ОГРНИП для ИП
		/// </summary>
		public const int PrivateBusinessmanOgrnLength = 15;
		/// <summary>
		/// Длина ИНН для ИП
		/// </summary>
		public const int PrivateBusinessmanInnLength = 12;
		/// <summary>
		/// Длина ОГРН для организаций(не ИП)
		/// </summary>
		public const int NotPrivateBusinessmanOgrnLength = 13;
		/// <summary>
		/// Длина ИНН для организаций(не ИП)
		/// </summary>
		public const int NotPrivateBusinessmanInnLength = 10;
	}
}
