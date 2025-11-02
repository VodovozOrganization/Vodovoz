namespace VodovozInfrastructure.Cryptography
{
	public interface ISignatureManager
	{
		string GenerateSignature(SignatureParams parameters);
		bool Validate(string sourceSignature, SignatureParams parameters, out string generatedSignature);
	}
}
