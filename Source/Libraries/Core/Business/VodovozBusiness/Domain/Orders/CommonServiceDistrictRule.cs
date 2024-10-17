using VodovozBusiness.Domain.Service;

namespace VodovozBusiness.Domain.Orders
{
	public class CommonServiceDistrictRule : ServiceDistrictRule
	{
		public override object Clone()
		{
			var newCommonDistrictRuleItem = new CommonServiceDistrictRule
			{
				Price = Price,
				ServiceDistrict = ServiceDistrict,
				ServiceType = ServiceType
			};

			return newCommonDistrictRuleItem;
		}
	}
}
