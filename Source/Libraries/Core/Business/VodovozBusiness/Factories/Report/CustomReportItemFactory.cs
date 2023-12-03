using System.Collections.Generic;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Factories.Report
{
	public class CustomReportItemFactory : ICustomReportItemFactory
	{
		public CustomReportItem CreateDefaultCustomReportItem(
			string left,
			string top,
			CustomProperties customProperties,
			string type = "QR Code",
			string height = "90pt",
			string width = "90pt")
		{
			return new CustomReportItem
			{
				Height = height,
				Width = width,
				Left = left,
				Top = top,
				ItemsElementNameList = new List<ItemsChoiceType29>
				{
					ItemsChoiceType29.Type,
					ItemsChoiceType29.CustomProperties
				},

				ItemsList = new List<object>
				{
					"QR Code",
					customProperties,
				}
			};
		}
	}
}
