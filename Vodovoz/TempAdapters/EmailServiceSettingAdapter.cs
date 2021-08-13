using EmailService;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class EmailServiceSettingAdapter : IEmailServiceSettingAdapter
	{
		public bool SendingAllowed => EmailServiceSetting.SendingAllowed;
	}
}