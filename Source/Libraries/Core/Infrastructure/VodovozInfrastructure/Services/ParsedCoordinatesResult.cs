namespace VodovozInfrastructure.Services
{
	public struct ParsedCoordinatesResult
	{
		public ParsedCoordinatesResult(string errorMessage, ParsedCoordinates? parsedCoordinates)
		{
			ErrorMessage = errorMessage;
			ParsedCoordinates = parsedCoordinates;
		}
		
		public string ErrorMessage { get; }
		public ParsedCoordinates? ParsedCoordinates { get; }
	}
}
