using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(
		Nominative = "Принадлежность авто",
		NominativePlural = "Принадлежности авто")]
	public enum CarOwnType
	{
		[Display(Name = "ТС компании")]
		Company,
		[Display(Name = "ТС в раскате")]
		Raskat,
		[Display(Name = "ТС водителя")]
		Driver
	}
}
