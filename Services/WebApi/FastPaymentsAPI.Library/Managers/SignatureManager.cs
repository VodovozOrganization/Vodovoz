using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Managers
{
	public class SignatureManager : ISignatureManager
	{
		public string GenerateSignature(SignatureParams parameters)
		{
			var md5Hash1 = MD5HexHashFromString.GetMD5HexHashFromString(parameters.Sign);
			var md5Hash2 = MD5HexHashFromString.GetMD5HexHashFromString(
				parameters.ShopId.ToString() + parameters.OrderId + parameters.OrderSumInKopecks);
			var md5Hash3 = MD5HexHashFromString.GetMD5HexHashFromString((md5Hash1 + md5Hash2).ToUpper());

			return md5Hash3.ToUpper();
		}

		public bool Validate(string signature, SignatureParams parameters)
		{
			var paymentSignature = GenerateSignature(parameters);
			return paymentSignature == signature;
		}
	}
}
