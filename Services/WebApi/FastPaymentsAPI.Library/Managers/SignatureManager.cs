using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Managers
{
	public class SignatureManager : ISignatureManager
	{
		public string GenerateSignature(SignatureParams parameters)
		{
			var MD5Hash1 = MD5HexHashFromString.GetMD5HexHashFromString(parameters.Sign);
			var MD5Hash2 = MD5HexHashFromString.GetMD5HexHashFromString(
				parameters.ShopId.ToString() + parameters.OrderId + parameters.OrderSumInKopecks);
			var MD5Hash3 = MD5HexHashFromString.GetMD5HexHashFromString((MD5Hash1 + MD5Hash2).ToUpper());

			return MD5Hash3.ToUpper();
		}

		public bool Validate(string signature, SignatureParams parameters)
		{
			var paymentSignature = GenerateSignature(parameters);
			return paymentSignature == signature;
		}
	}
}
