using System;
using Vodovoz.Infrastructure.Services;
using QSValidation;
using QS.Permissions;
using Vodovoz.Infrastructure.Journal;
using QS.Services;

namespace Vodovoz
{
	public static class ServicesConfig
	{
		private static ICommonServices commonServices;
		public static ICommonServices CommonServices {
			get {
				if(commonServices == null) {
					commonServices = new CommonServices(ValidationService, InteractiveService, PermissionService, UserService);
				}
				return commonServices;
			}
		}

		private static IValidationService validationService;
		public static IValidationService ValidationService {
			get {
				if(validationService == null) {
					IValidationViewFactory viewFactory = new GtkValidationViewFactory();
					validationService = new ValidationService(viewFactory);
				}
				return validationService;
			}
		}

		private static IInteractiveService interactiveService;
		public static IInteractiveService InteractiveService {
			get {
				if(interactiveService == null) {
					interactiveService = new GtkInteractiveService();
				}
				return interactiveService;
			}
		}

		private static IPermissionService permissionService;
		public static IPermissionService PermissionService {
			get {
				if(permissionService == null) {
					permissionService = new PermissionService(PermissionsSettings.EntityPermissionValidator);
				}
				return permissionService;
			}
		}

		private static IUserService userService;
		public static IUserService UserService {
			get {
				if(userService == null) {
					userService = new UserService();
				}
				return userService;
			}
		}

		private static IEmployeeService employeeService;
		public static IEmployeeService EmployeeService {
			get {
				if(employeeService == null) {
					employeeService = new EmployeeService();
				}
				return employeeService;
			}
		}

		private static IRepresentationEntityPicker representationEntityPicker;
		public static IRepresentationEntityPicker RepresentationEntityPicker {
			get {
				if(representationEntityPicker == null) {
					IRepresentationJournalFactory journalFactory = new PermissionControlledRepresentationJournalFactory();
					representationEntityPicker = new RepresentationEntityPickerGtk(journalFactory);
				}
				return representationEntityPicker;
			}
		}
	}
}
