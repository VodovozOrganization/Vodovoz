namespace FastPaymentsAPI.Library.Managers;

public interface ISignatureManager
{
	string GenerateSignature(SignatureParams parameters);
	bool Validate(string signature, SignatureParams parameters);
}
