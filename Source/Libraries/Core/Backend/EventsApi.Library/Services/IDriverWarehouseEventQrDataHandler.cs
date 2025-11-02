using LogisticsEventsApi.Contracts;

namespace EventsApi.Library.Services
{
	public interface IDriverWarehouseEventQrDataHandler
	{
		DriverWarehouseEventQrData ConvertQrData(string qrData);
	}
}
