using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public interface ICustomPropertiesFactory
	{
		CustomProperties CreateDefaultQrCustomProperties(string qrString);
	}
}
