namespace Vodovoz.Tools
{
	public interface IPasswordGenerator
	{
		string GeneratePassword(uint length);
		string GeneratePasswordWithOtherCharacter(uint length);
	}
}
