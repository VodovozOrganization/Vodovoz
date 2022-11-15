using Taxcom.TTC.Reglament.Crypto;

namespace TrueApi.Models
{
	public class SignModel
	{
		private readonly string _thumbprint;
		private readonly string _base64StringForSign;
		private readonly bool _isSignAsAttached;

		public SignModel(string thumbprint, string base64StringForSign, bool isSignAsAttached)
		{
			_thumbprint = thumbprint;
			_base64StringForSign = base64StringForSign;
			_isSignAsAttached = isSignAsAttached;
		}

		public string Sign()
		{
			Encrypter encrypter = new Encrypter();

			var sign = _isSignAsAttached
				? encrypter.SignDocumentAsAttached(_base64StringForSign, _thumbprint)
				: encrypter.SignDocumentAsDetached(_base64StringForSign, _thumbprint);

			return sign;
		}
	}
}
