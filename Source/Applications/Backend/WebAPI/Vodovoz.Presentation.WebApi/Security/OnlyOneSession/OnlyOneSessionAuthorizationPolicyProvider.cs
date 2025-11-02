using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.WebApi.Security.OnlyOneSession
{
	public class OnlyOneSessionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
	{
		public OnlyOneSessionAuthorizationPolicyProvider(IOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions> options)
			: base(options)
		{
		}

		public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
		{
			AuthorizationPolicy policy = await base.GetPolicyAsync(policyName);

			if(policy != null)
			{
				return policy;
			}

			return new AuthorizationPolicyBuilder()
				.AddRequirements(new OnlyOneSessionRequirement())
				.Build();
		}
	}
}
