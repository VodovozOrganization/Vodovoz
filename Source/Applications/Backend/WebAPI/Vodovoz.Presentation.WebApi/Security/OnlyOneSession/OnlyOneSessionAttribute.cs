using Microsoft.AspNetCore.Authorization;

namespace Vodovoz.Presentation.WebApi.Security.OnlyOneSession
{
	public class OnlyOneSessionAttribute : AuthorizeAttribute
	{
		public OnlyOneSessionAttribute() : base (policy: nameof(OnlyOneSession))
		{
		}
	}
}
