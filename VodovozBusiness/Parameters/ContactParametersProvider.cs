using Vodovoz.Core.DataService;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class ContactParametersProvider : IContactsParameters
	{
		public static ContactParametersProvider Instance { get; private set; }
		private static IContactsParameters parameters;

		static ContactParametersProvider()
		{	
			parameters = new BaseParametersProvider(new ParametersProvider());
			Instance = new ContactParametersProvider();
		}

		private ContactParametersProvider()
		{
		}

		public int MinSavePhoneLength => parameters.MinSavePhoneLength;

		public string DefaultCityCode => parameters.DefaultCityCode;
	}
}
