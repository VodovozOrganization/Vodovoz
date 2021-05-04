namespace Vodovoz.Services
{
    public interface IRouteListParametersProvider
    {
        int CashSubdivisionSofiiskayaId { get; }
        int CashSubdivisionParnasId { get; }
        int WarehouseSofiiskayaId { get; }
        int WarehouseParnasId { get; }
        int SouthGeographicGroupId { get; }
    }
}