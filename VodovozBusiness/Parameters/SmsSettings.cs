using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class SmsSettings : ISmsSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public SmsSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		public string MegafonSenderName => _parametersProvider.GetStringValue("megafon_sms_sender");

		public SmsProvider SmsProvider
		{
			get
			{
				var smsProvider = _parametersProvider.GetStringValue("sms_provider_(Megafon,SmsRu)");
				switch(smsProvider)
				{
					case "SmsRu":
						return SmsProvider.SmsRu;
					case "Megafon":
					default:
						return SmsProvider.Megafon;
				}
			}
		}
	}
}
