using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public interface ICustomReportFactory
	{
		CustomReportItem CreateDefaultQrReportItem(
			string left, string top, string qrString, string height = "80pt", string width = "80pt");

		CustomProperties CreateDefaultQrCustomProperties(string qrString);

		CustomReportItem CreateDefaultQrCustomReportItem(
			string left,
			string top,
			CustomProperties customProperties,
			string type = "QR Code",
			string height = "80pt",
			string width = "80pt");

		Textbox CreateTextBox(string value, string left, string top, string width = "90pt", string height = "15pt");
	}
}
