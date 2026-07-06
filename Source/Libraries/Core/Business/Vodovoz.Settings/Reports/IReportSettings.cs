namespace Vodovoz.Settings.Reports
{
	public interface IReportSettings
	{
		/// <summary>
		/// Получить идентификатор организации по умолчанию
		/// </summary>
		int GetDefaultOrderChangesOrganizationId { get; }

		/// <summary>
		/// Список идентификаторов номенклатур для исключения затрат ОХР
		/// </summary>
		int[] DealerNomenclatureIds { get; }
	}
}
