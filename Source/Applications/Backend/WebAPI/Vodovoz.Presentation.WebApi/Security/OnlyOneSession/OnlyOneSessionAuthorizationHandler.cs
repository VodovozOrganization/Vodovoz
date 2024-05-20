using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

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
			string username = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

			if(!string.IsNullOrWhiteSpace(username))
			{
				return;
			}

			using(var scope = _scopeFactory.CreateScope())
			{
				IEmployeeRepository employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

				IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var employee = employeeRepository.GetEmployeeByAndroidLogin(unitOfWork, username);

				var onlyOneSessionAllowed = _optionsMonitor.CurrentValue.Authorization?.OnlyOneSessionAllowed ?? false;

				var allowedApplicationTypes = _optionsMonitor.CurrentValue.Authorization?.ApplicationUserTypes ?? Enumerable.Empty<ExternalApplicationType>();

				var applicationUsers = employee.ExternalApplicationsUsers.Where(x => allowedApplicationTypes.Contains(x.ExternalApplicationType));

				if(!onlyOneSessionAllowed
					|| applicationUsers
						.Any(x => x.SessionKey == context.User?.Claims
							.FirstOrDefault(x => x.ValueType == VodovozClaimTypes.ActiveSessionKey)?.Value))
				{
					context.Succeed(requirement);
				}
			}
		}
	}
}
