namespace VodovozInfrastructure.Services
{
	public class CoordinatesParser : ICoordinatesParser
	{
		public ParsedCoordinatesResult GetCoordinatesFromBuffer(string buffer)
		{
			var coordinates = buffer?.Split(',');
			
			if(coordinates?.Length == 2)
			{
				coordinates[0] = coordinates[0].Replace('.', ',');
				coordinates[1] = coordinates[1].Replace('.', ',');

				var goodLat = decimal.TryParse(coordinates[0].Trim(), out var latitude);
				var goodLon = decimal.TryParse(coordinates[1].Trim(), out var longitude);

				if(goodLat && goodLon)
				{
					return new ParsedCoordinatesResult(null, new ParsedCoordinates(latitude, longitude));
				}
			}
			
			return new ParsedCoordinatesResult("Буфер обмена не содержит координат или содержит неправильные координаты", null);
		}
	}
}
