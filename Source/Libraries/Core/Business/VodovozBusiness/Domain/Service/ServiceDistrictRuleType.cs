using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Service
{
	public abstract partial class ServiceDistrictRule
	{
		public enum ServiceDistrictRuleType
		{
			[Display(Name = "Основной")]
			Common,
			[Display(Name = "По дням")]
			WeekDay
		}
	}
}
