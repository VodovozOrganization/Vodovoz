namespace VodovozInfrastructure.Services
{
	public interface ICoordinatesParser
	{
		ParsedCoordinatesResult GetCoordinatesFromBuffer(string buffer);
	}
}
