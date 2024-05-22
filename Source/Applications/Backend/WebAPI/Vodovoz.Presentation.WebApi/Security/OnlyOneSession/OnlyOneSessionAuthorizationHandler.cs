using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Presentation.WebApi.Security.OnlyOneSession
{
	public class OnlyOneSessionAuthorizationHandler : AuthorizationHandler<OnlyOneSessionRequirement>
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IOptionsMonitor<SecurityOptions> _optionsMonitor;

		public OnlyOneSessionAuthorizationHandler(
			IServiceScopeFactory scopeFactory,
			IOptionsMonitor<SecurityOptions> optionsMonitor)
		{
			_scopeFactory = scopeFactory;
			_optionsMonitor = optionsMonitor;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyOneSessionRequirement requirement)
		{
			string username = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

			if(string.IsNullOrWhiteSpace(username))
			{
				return;
			}

			using var scope = _scopeFactory.CreateScope();
			using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var onlyOneSessionAllowed = _optionsMonitor.CurrentValue.Authorization?.OnlyOneSessionAllowed ?? false;

			var allowedApplicationTypes = _optionsMonitor.CurrentValue.Authorization?.ApplicationUserTypes ?? Enumerable.Empty<ExternalApplicationType>();

			var applicationUser = unitOfWork.Session.Query<ExternalApplicationUserForApi>()
				.Where(eau => eau.Login == username
					&& allowedApplicationTypes.Contains(eau.ExternalApplicationType))
				.FirstOrDefault();

			if(!onlyOneSessionAllowed)
			{
				context.Succeed(requirement);
				return;
			}

			var tokenActiveSessionKey = context.User?.Claims
				.FirstOrDefault(x => x.Type == VodovozClaimTypes.ActiveSessionKey)?.Value;

			if(applicationUser != null &&
				applicationUser.SessionKey == tokenActiveSessionKey)
			{
				context.Succeed(requirement);
				return;
			}

			context.Fail();
		}
	}
}
