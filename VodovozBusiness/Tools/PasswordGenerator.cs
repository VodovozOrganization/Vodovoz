using System;
using System.Linq;

namespace Vodovoz.Tools
{
    public class PasswordGenerator : IPasswordGenerator
    {
        /// <summary>
        /// Генерирует пароль с минимум одной цифрой, одной строчной буквой и одной прописной буквой
        /// </summary>
        /// <param name="length">Минимальной значение - 3</param>
        /// <returns></returns>
        public string GeneratePassword(uint length)
        {
            if(length < 3) {
                throw new ArgumentException("Минимальная длина пароля - 3 символа");
            }
            var random = new Random();
            var charStr = "abcdefghijklmnpqrstuvwxyz";
            var password = "";

            for(int i = 0; i < length / 2; i++) {
                password += charStr[random.Next(0, charStr.Length)];
                password += Char.ToUpper(charStr[random.Next(0, charStr.Length)]);
            }
            password = password.Substring(1) + random.Next(1, 10);

            return String.Join("", password.OrderBy(x => random.Next()));
        }
        
        /// <summary>
        /// Генерирует пароль с минимум одной цифрой, одним спец. символом, одной строчной буквой и одной прописной буквой
        /// </summary>
        /// <param name="length">Минимальной значение - 4</param>
        /// <returns></returns>
        public string GeneratePasswordWithOtherCharacter(uint length)
        {
            if(length < 4) {
                throw new ArgumentException("Минимальная длина пароля - 4 символа");
            }
            var random = new Random();
            var charStr = "abcdefghijklmnpqrstuvwxyz";
            var password = "";
            
            var otherCharArray = new[] { '.', '?', '!', '*', '-', '+' };
            password += otherCharArray[random.Next(0, otherCharArray.Length)];
            password += random.Next(1, 10);

            for(int i = 0; i < (length - 2) / 2; i++) {
                password += charStr[random.Next(0, charStr.Length)];
                password += Char.ToUpper(charStr[random.Next(0, charStr.Length)]);
            }

            return String.Join("", password.OrderBy(x => random.Next()));
        }
    }
}
