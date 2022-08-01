namespace FastPaymentsAPI.Library.Managers
{
	public interface ISignatureManager
	{
		string GenerateSignature(SignatureParams parameters);
		bool Validate(string bankSignature, SignatureParams parameters, out string paymentSignature);
	}
}
