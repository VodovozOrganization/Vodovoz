namespace Vodovoz.Presentation.WebApi.Security
{
	public class UserOptions
	{
		public string AllowedUserNameCharacters { get; set; }
		public bool RequireUniqueEmail { get; set; }
		public bool LoginCaseSensitive { get; set; }
	}
}
