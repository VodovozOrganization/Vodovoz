namespace Vodovoz.Services
{
	public interface IGeographicGroupParametersProvider
	{
		int SouthGeographicGroupId { get; }
		int NorthGeographicGroupId { get; }
		int EastGeographicGroupId { get; }
	}
}
