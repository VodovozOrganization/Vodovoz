namespace Vodovoz.Settings.Reports
{
	public interface IReportSettings
	{
		/// <summary>
		/// Получить идентификатор организации по умолчанию
		/// </summary>
		int GetDefaultOrderChangesOrganizationId { get; }

		/// <summary>
		/// Получить идентификатор группы номенклатуры для группы "Диллеры"
		/// </summary>
		int GetDealerNomenclatureGroupId { get; }
	}
}
