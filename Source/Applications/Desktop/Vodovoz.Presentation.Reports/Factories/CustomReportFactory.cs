using System;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public class CustomReportFactory : ICustomReportFactory
	{
		private readonly ICustomPropertiesFactory _customPropertiesFactory;
		private readonly ICustomReportItemFactory _customReportItemFactory;
		private readonly IRdlTextBoxFactory _rdlTextBoxFactory;

		public CustomReportFactory(
			ICustomPropertiesFactory customPropertiesFactory,
			ICustomReportItemFactory customReportItemFactory,
			IRdlTextBoxFactory rdlTextBoxFactory
			)
		{
			_customPropertiesFactory = customPropertiesFactory ?? throw new ArgumentNullException(nameof(customPropertiesFactory));
			_customReportItemFactory = customReportItemFactory ?? throw new ArgumentNullException(nameof(customReportItemFactory));
			_rdlTextBoxFactory = rdlTextBoxFactory ?? throw new ArgumentNullException(nameof(rdlTextBoxFactory));
		}

		public CustomReportItem CreateDefaultQrReportItem(
			string left, string top, string qrString, string height = "80pt", string width = "80pt")
		{
			var customProperties = CreateDefaultQrCustomProperties(qrString);
			return CreateDefaultQrCustomReportItem(left, top, customProperties, height: height, width: width);
		}

		public CustomProperties CreateDefaultQrCustomProperties(string qrString)
		{
			return _customPropertiesFactory.CreateDefaultQrCustomProperties(qrString);
		}
		
		public CustomReportItem CreateDefaultQrCustomReportItem(
			string left,
			string top,
			CustomProperties customProperties,
			string type = "QR Code",
			string height = "80pt",
			string width = "80pt")
		{
			return _customReportItemFactory.CreateDefaultCustomReportItem(left, top, customProperties, type, height, width);
		}

		public Textbox CreateTextBox(string value, string left, string top, string width = "90pt", string height = "15pt")
		{
			return _rdlTextBoxFactory.CreateTextBox(value, left, top, width, height);
		}
	}
}
