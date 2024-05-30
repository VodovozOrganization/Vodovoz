using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class ConfigureIdentityOptions : IConfigureOptions<IdentityOptions>
	{
		private readonly IOptions<SecurityOptions> _options;

		public ConfigureIdentityOptions(IOptions<SecurityOptions> options)
		{
			_options = options ?? throw new System.ArgumentNullException(nameof(options));
		}

		public void Configure(IdentityOptions options)
		{
			// Password settings
			options.Password.RequireDigit = _options.Value.Password.RequireDigit;
			options.Password.RequireLowercase = _options.Value.Password.RequireLowercase;
			options.Password.RequireNonAlphanumeric = _options.Value.Password.RequireNonAlphanumeric;
			options.Password.RequireUppercase = _options.Value.Password.RequireUppercase;
			options.Password.RequiredLength = _options.Value.Password.RequiredLength;
			options.Password.RequiredUniqueChars = _options.Value.Password.RequiredUniqueChars;

			// Lockout settings.
			options.Lockout.DefaultLockoutTimeSpan = _options.Value.Lockout.DefaultLockout;
			options.Lockout.MaxFailedAccessAttempts = _options.Value.Lockout.MaxFailedAccessAttempts;
			options.Lockout.AllowedForNewUsers = _options.Value.Lockout.AllowedForNewUsers;

			// User settings.
			options.User.AllowedUserNameCharacters = _options.Value.User.AllowedUserNameCharacters;
			options.User.RequireUniqueEmail = _options.Value.User.RequireUniqueEmail;
		}
	}
}
