namespace Vodovoz.Security
{
	public interface IPasswordHasher
	{
		(string Salt, string PasswordHash) HashPassword(string password);
		string HashPassword(string password, byte[] salt);
		bool VerifyHashedPassword(byte[] salt, string hash, string providedPassword);
	}
}
