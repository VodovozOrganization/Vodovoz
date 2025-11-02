namespace Vodovoz.Settings.Logistics
{
	public interface IRouteListSettings
	{
		int CashSubdivisionSofiiskayaId { get; }
		int CashSubdivisionParnasId { get; }
		int WarehouseSofiiskayaId { get; }
		int WarehouseParnasId { get; }

		//Склад Бугры
		int WarehouseBugriId { get; }
		int SouthGeographicGroupId { get; }
	}
}
