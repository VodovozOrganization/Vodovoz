using Vodovoz.Core.Domain.Employees;

namespace VodovozBusiness.Nodes
{
	public class EmployeeInnerPhoneNode : IEmployeeInnerPhone
	{
		public int EmployeeId { get; private set; }
		public uint? InnerPhone { get; private set; }

		public static EmployeeInnerPhoneNode Create(int id, uint? innerPhone) =>
			new EmployeeInnerPhoneNode
			{
				EmployeeId = id,
				InnerPhone = innerPhone
			};
	}
}
