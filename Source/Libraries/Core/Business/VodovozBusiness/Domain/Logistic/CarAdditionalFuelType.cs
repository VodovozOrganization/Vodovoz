using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Logistic
{
	/// <summary>
	/// Дополнительный вид топлива для автомобиля
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "дополнительные виды топлива",
		Nominative = "дополнительный вид топлива",
		GenitivePlural = "дополнительных видов топлива")]
	[HistoryTrace]
	public class CarAdditionalFuelType : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private Car _car;
		private FuelType _fuelType;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Автомобиль
		/// </summary>
		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		/// <summary>
		/// Вид топлива
		/// </summary>
		[Display(Name = "Вид топлива")]
		public virtual FuelType FuelType
		{
			get => _fuelType;
			set => SetField(ref _fuelType, value);
		}

		public virtual string Title => $"{Car?.Title} - {FuelType?.Name}";

	}
}
