﻿using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public interface ICustomReportItemFactory
	{
		CustomReportItem CreateDefaultCustomReportItem(
			string left,
			string top,
			CustomProperties customProperties,
			string type = "QR Code",
			string height = "90pt",
			string width = "90pt");
	}
}
