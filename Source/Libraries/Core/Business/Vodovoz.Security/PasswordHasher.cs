using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Vodovoz.Security
{
	public class PasswordHasher : IPasswordHasher
	{
		private const int _bitsOnByte = 8;
		private const int _saltBits = 128;
		private const int _hashBits = 256;
		private const int _iterationsCount = 100000;
		
		public (string Salt, string PasswordHash) HashPassword(string password)
		{
			var salt = new byte[_saltBits / _bitsOnByte];
			using (var rngCsp = new RNGCryptoServiceProvider())
			{
				rngCsp.GetNonZeroBytes(salt);
			}

			var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: _iterationsCount,
				numBytesRequested: _hashBits / _bitsOnByte));
			
			return (Convert.ToBase64String(salt), hash);
		}

		public bool VerifyHashedPassword(byte[] salt, string hash, string providedPassword)
		{
			var generatedHash = HashPassword(providedPassword, salt);
			return generatedHash == hash;
		}
		
		public string HashPassword(string password, byte[] salt)
		{
			var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: _iterationsCount,
				numBytesRequested: _hashBits / _bitsOnByte));
			
			return hashed;
		}
	}
}
