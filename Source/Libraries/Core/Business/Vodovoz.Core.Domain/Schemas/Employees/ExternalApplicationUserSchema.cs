namespace Vodovoz.Core.Domain.Schemas.Employees
{
	public class ExternalApplicationUserSchema
	{
		public static string TableName => "external_applications_users";
		public static string IdColumn => "id";
		public static string LoginColumn => "login";
		public static string PasswordColumn => "password";
		public static string SessionKeyColumn => "session_key";
		public static string TokenColumn => "token";
		public static string ExternalApplicationTypeColumn => "external_application_type";
		public static string EmployeeColumn => "employee_id";
	}
}
