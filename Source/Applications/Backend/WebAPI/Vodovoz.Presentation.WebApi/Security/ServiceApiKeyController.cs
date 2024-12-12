using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class ServiceApiKeyController : ControllerBase
	{
		private readonly IOptionsMonitor<SecurityOptions> _securityOptionsMonitor;

		public ServiceApiKeyController(IOptionsMonitor<SecurityOptions> securityOptionsMonitor)
		{
			_securityOptionsMonitor = securityOptionsMonitor
				?? throw new ArgumentNullException(nameof(securityOptionsMonitor));
		}

		[FeatureGate(FeatureFlags.ServiceApiKeyController)]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpGet("/GetServiceToken")]
		public IActionResult Token([FromServices] IFeatureManager featureManager)
		{
			var identity = GetIdentity();

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptionsMonitor.CurrentValue.Token.Key));

			var jwt = new JwtSecurityToken(
				claims: identity.Claims,
				signingCredentials: new SigningCredentials(
					securityKey,
					SecurityAlgorithms.HmacSha256));

			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			var response = new
			{
				access_token = encodedJwt,
				username = identity.Name
			};

			return new JsonResult(response);
		}

		private ClaimsIdentity GetIdentity()
		{
			ClaimsIdentity claimsIdentity =
				new ClaimsIdentity(
					Enumerable.Empty<Claim>(),
					"Token",
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);

			return claimsIdentity;
		}
	}
}
