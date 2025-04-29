namespace VodovozInfrastructure.Services
{
	public struct ParsedCoordinates
	{
		public ParsedCoordinates(decimal latitude, decimal longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}
		
		public decimal Latitude { get; }
		public decimal Longitude { get; }
	}
}
