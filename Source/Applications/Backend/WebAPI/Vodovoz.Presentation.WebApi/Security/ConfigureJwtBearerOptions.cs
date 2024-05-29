using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
	{
		private readonly IOptions<SecurityOptions> _options;

		public ConfigureJwtBearerOptions(IOptions<SecurityOptions> options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		public void Configure(JwtBearerOptions options)
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidIssuer = _options.Value.Token.Issuer,
				ValidateAudience = false,
				ValidAudience = _options.Value.Token.Audience,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey =
					new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Token.Key))
			};

			options.IncludeErrorDetails = true;
		}

		public void PostConfigure(string name, JwtBearerOptions options)
		{
			Configure(options);
		}
	}
}
