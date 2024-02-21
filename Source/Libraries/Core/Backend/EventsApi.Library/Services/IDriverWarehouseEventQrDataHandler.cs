using DriverApi.Contracts.V4;

namespace EventsApi.Library.Services
{
	public interface IDriverWarehouseEventQrDataHandler
	{
		DriverWarehouseEventQrData ConvertQrData(string qrData);
	}
}
