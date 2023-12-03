using Vodovoz.RDL.Elements;

namespace Vodovoz.Factories.Report
{
	public interface IRdlTextBoxFactory
	{
		Textbox CreateTextBox(
			string value,
			string left,
			string top,
			string width = "100pt",
			string height = "15pt");
	}
}
