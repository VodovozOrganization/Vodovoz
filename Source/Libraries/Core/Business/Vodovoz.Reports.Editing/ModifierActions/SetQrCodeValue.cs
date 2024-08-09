using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetQrCodeValue : ModifierAction
	{
		private readonly string _qrCodeItemName;
		private readonly string _newValue;

		public SetQrCodeValue(string qrCodeItemName, string newValue)
		{
			if(string.IsNullOrWhiteSpace(qrCodeItemName))
			{
				throw new ArgumentException($"'{nameof(qrCodeItemName)}' cannot be null or whitespace.", nameof(qrCodeItemName));
			}

			if(string.IsNullOrWhiteSpace(newValue))
			{
				throw new ArgumentException($"'{nameof(newValue)}' cannot be null or whitespace.", nameof(newValue));
			}

			_qrCodeItemName = qrCodeItemName;
			_newValue = newValue;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;

			report.SetQrCodeValue(_qrCodeItemName, _newValue, @namespace);
		}
	}
}

