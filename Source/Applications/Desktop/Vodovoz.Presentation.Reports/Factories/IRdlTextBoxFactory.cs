using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
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
