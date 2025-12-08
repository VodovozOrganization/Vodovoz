namespace TaxcomEdo.Library.Options
{
	public sealed class TaxcomEdoOptions
	{
		public const string Path = nameof(TaxcomEdoOptions);

		/// <summary>
		/// Основной ЭДО аккаунт
		/// </summary>
		public string OurMainEdoAccountId { get; set; }

		/// <summary>
		/// Остальные ЭДО аккаунты
		/// </summary>
		public string[] OurEdoAccountsIds { get; set; }
	}
}
