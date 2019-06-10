using System;
namespace Vodovoz.Infrastructure.Services
{
	public class CommonServices : ICommonServices
	{
		public CommonServices(
			IValidationService validationService, 
			IInteractiveService interactiveService,
			IPermissionService permissionService, 
			IUserService userService)
		{
			ValidationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
			InteractiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			PermissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			UserService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public IValidationService ValidationService { get; }

		public IInteractiveService InteractiveService { get; }

		public IPermissionService PermissionService { get; }

		public IUserService UserService { get; }
	}
}
