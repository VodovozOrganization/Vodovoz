using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Managers
{
	public class SignatureManager : ISignatureManager
	{
		private readonly IMD5HexHashFromString _mD5HexHashFromString;

		public SignatureManager(IMD5HexHashFromString mD5HexHashFromString)
		{
			_mD5HexHashFromString = mD5HexHashFromString ?? throw new System.ArgumentNullException(nameof(mD5HexHashFromString));
		}

		public string GenerateSignature(SignatureParams parameters)
		{
			var md5Hash1 = _mD5HexHashFromString.GetMD5HexHashFromString(parameters.Sign);
			var md5Hash2 = _mD5HexHashFromString.GetMD5HexHashFromString(
				parameters.ShopId.ToString() + parameters.OrderId + parameters.OrderSumInKopecks);
			var md5Hash3 = _mD5HexHashFromString.GetMD5HexHashFromString((md5Hash1 + md5Hash2).ToUpper());

			return md5Hash3.ToUpper();
		}

		public bool Validate(string bankSignature, SignatureParams parameters, out string paymentSignature)
		{
			paymentSignature = GenerateSignature(parameters);
			return paymentSignature == bankSignature;
		}
	}
}
