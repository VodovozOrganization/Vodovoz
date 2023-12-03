using Vodovoz.RDL.Elements;

namespace Vodovoz.Factories.Report
{
	public interface ICustomPropertiesFactory
	{
		CustomProperties CreateDefaultQrCustomProperties(string qrString);
	}
}
