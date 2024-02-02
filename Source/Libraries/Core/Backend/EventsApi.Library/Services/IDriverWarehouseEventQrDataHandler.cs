using EventsApi.Library.Dtos;

namespace EventsApi.Library.Services
{
	public interface IDriverWarehouseEventQrDataHandler
	{
		DriverWarehouseEventQrData ConvertQrData(string qrData);
	}
}
