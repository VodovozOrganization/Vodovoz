using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class AuthorizationOptions
	{
		public IEnumerable<ApplicationUserRole> GrantedRoles { get; set; }
		public IEnumerable<ExternalApplicationType> ApplicationUserTypes { get; set; }
		public bool OnlyOneSessionAllowed { get; set; } = false;
	}
}
