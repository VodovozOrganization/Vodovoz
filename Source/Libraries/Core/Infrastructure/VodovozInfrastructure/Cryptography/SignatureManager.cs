using System.Linq;
using System.Reflection;

namespace VodovozInfrastructure.Cryptography
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

			var properties =
				parameters.GetType()
					.GetProperties()
					.Where(x => x.Name != nameof(parameters.Sign))
					.OrderBy(x => x.GetCustomAttribute<PositionForGenerateSignatureAttribute>().Position);
			
			var stringForHash2 = properties.Aggregate(string.Empty, (current, property) => current + property.GetValue(parameters));
			var md5Hash2 = _mD5HexHashFromString.GetMD5HexHashFromString(stringForHash2);
			var md5Hash3 = _mD5HexHashFromString.GetMD5HexHashFromString((md5Hash1 + md5Hash2).ToUpper());

			return md5Hash3.ToUpper();
		}

		public bool Validate(string sourceSignature, SignatureParams parameters, out string generatedSignature)
		{
			generatedSignature = GenerateSignature(parameters);
			return generatedSignature == sourceSignature;
		}
	}
}
