namespace Vodovoz.Services
{
	public interface IRouteListParametersProvider
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
