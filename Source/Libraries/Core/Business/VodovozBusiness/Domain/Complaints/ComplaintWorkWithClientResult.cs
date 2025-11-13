using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Complaints
{
	[Appellative(
		Nominative = "результат работы по клиенту",
		NominativePlural = "результаты работ по клиентам")]
	public enum ComplaintWorkWithClientResult
	{
		[Display(Name = "Проблема решена")]
		Solved,
		[Display(Name = "Проблема не решена")]
		NotSolved
	}
}
