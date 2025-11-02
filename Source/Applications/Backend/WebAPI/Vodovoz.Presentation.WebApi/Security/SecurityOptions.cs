namespace Vodovoz.Presentation.WebApi.Security
{
	public class SecurityOptions
	{
		public TokenOptions Token { get; set; }
		public PasswordOptions Password { get; set; }
		public LockoutOptions Lockout { get; set; }
		public UserOptions User { get; set; }
		public AuthorizationOptions Authorization { get; set; }
	}
}
