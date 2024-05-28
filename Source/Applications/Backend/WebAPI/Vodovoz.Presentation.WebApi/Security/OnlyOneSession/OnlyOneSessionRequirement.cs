using Microsoft.AspNetCore.Authorization;

namespace Vodovoz.Presentation.WebApi.Security.OnlyOneSession
{
	public class OnlyOneSessionRequirement : IAuthorizationRequirement
	{
	}
}
