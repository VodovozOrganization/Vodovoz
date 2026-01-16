namespace Vodovoz.Security
{
	public interface IPasswordHasher
	{
		(string Salt, string PasswordHash) HashPassword(string password);
		string HashPassword(string password, byte[] salt);
		bool VerifyHashedPassword(string salt, string hash, string providedPassword);
	}
}
