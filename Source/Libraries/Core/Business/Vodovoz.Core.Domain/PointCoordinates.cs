namespace Vodovoz.Core.Domain
{
	public struct PointCoordinates
	{
		public PointCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}
		
		public decimal? Latitude { get; }
		public decimal? Longitude { get; }
	}
}
