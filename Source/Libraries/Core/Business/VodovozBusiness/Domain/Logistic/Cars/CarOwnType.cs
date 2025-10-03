using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	/// <summary>
	/// Принадлежность авто
	/// </summary>

	[Appellative(
		Nominative = "Принадлежность авто",
		NominativePlural = "Принадлежности авто")]
	public enum CarOwnType
	{
		/// <summary>
		/// ТС компании
		/// </summary>
		[Display(Name = "ТС компании")]
		Company,

		/// <summary>
		/// ТС в раскате
		/// </summary>
		[Display(Name = "ТС в раскате")]
		Raskat,

		/// <summary>
		/// ТС водителя
		/// </summary>
		[Display(Name = "ТС водителя")]
		Driver
	}
}
