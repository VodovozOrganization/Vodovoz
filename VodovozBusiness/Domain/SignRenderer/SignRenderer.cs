using System;
using Newtonsoft.Json;
//using Mango;
using Vodovoz.Services;
namespace Vodovoz.Domain.SignRenderer
{
	public class SignRenderer
	{
		static IVpbxSettings Settings;
		static string JsonRequest;
		public SignRenderer(IVpbxSettings settings , string jsonRequest)
		{
			Settings = settings;
			JsonRequest = jsonRequest;
		}

		public static void SignForming()
		{
			//string signKey = sha(Settings.VpbxApiKey + JsonRequest + Settings.VpbxApiSalt);
		}
	}
}
