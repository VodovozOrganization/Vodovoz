using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Service
{
	public enum ServiceDistrictsSetStatus
	{
		[Display(Name = "Черновик")]
		Draft,
		[Display(Name = "Активна")]
		Active,
		[Display(Name = "Закрыта")]
		Closed
	}
}
