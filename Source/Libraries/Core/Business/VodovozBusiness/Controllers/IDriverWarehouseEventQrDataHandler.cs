using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Controllers
{
	public interface IDriverWarehouseEventQrDataHandler
	{
		(DriverWarehouseEventQrData QrData, IEnumerable<ValidationResult> ValidationResults) ConvertQrData(string qrData);
	}
}
