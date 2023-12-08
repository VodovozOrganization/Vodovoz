using System.Collections.Generic;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public class RdlTextBoxFactory : IRdlTextBoxFactory
	{
		public Textbox CreateTextBox(
			string value,
			string left,
			string top,
			string width = "100pt",
			string height = "15pt")
		{
			return new Textbox
			{
				Height = height,
				Width = width,
				Top = top,
				Left = left,
				ItemsElementNameList = new List<ItemsChoiceType14>
				{
					ItemsChoiceType14.Value
				},

				ItemsList = new List<object>
				{
					value
				}
			};
		}
	}
}
