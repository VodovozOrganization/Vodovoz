using VodovozBusiness.Domain.Common;

namespace VodovozBusiness.Domain.Employees
{
	public class EmployeeFileInformation : FileInformation
	{
		private int _employeeId;

		public int EmployeeId
		{
			get => _employeeId;
			set => _employeeId = value;
		}
	}
}
