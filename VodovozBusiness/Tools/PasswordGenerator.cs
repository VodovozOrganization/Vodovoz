using System;
using System.Linq;

namespace Vodovoz.Tools
{
    public class PasswordGenerator : IPasswordGenerator
    {
        public string GeneratePassword(int length)
        {
            var random = new Random();
            var otherCharArray = new[] { '.', '?', '!', '*', '-', '+' };
            var charStr = "abcdefghijklmnpqrstuvwxyz";
            var password = random.Next(1, 10).ToString();
            password += otherCharArray[random.Next(0, otherCharArray.Length)];

            for(int i = 0; i < length / 2; i++) {
                password += charStr[random.Next(0, charStr.Length)];
                password += Char.ToUpper(charStr[random.Next(0, charStr.Length)]);
            }

            return String.Join("", password.OrderBy(x => random.Next()));
        }
    }
}
