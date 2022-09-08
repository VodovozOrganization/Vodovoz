using System.ComponentModel.DataAnnotations;

namespace VodovozInfrastructure.Versions
{
	public enum VersionStatus
	{
		[Display(Name = "Черновик")]
		Draft,
		[Display(Name = "Активна")]
		Active,
		[Display(Name = "Закрыта")]
		Closed
	}
}
