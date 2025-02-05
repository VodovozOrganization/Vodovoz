using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic
{

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "виды топлива",
		Nominative = "вид топлива",
		GenitivePlural = "Видов топлива")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private IList<FuelPriceVersion> _fuelPriceVersions = new List<FuelPriceVersion>();

		public FuelType()
		{
			Name = string.Empty;
		}

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(20)]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Цена")]
		[Required(ErrorMessage = "Цена должна быть заполнена.")]
		public virtual decimal Cost
		{
			get
			{
				return GetFuelPriceVersions();
			}
		}

		public virtual IList<FuelPriceVersion> FuelPriceVersions
		{
			get => _fuelPriceVersions;
			set => SetField(ref _fuelPriceVersions, value);
		}

		private GenericObservableList<FuelPriceVersion> _observableFuelPriceVersions;
		public virtual GenericObservableList<FuelPriceVersion> ObservableFuelPriceVersions => _observableFuelPriceVersions
			?? (_observableFuelPriceVersions = new GenericObservableList<FuelPriceVersion>(FuelPriceVersions));

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Cost < 0)
			{
				yield return new ValidationResult("Стоимость не может быть отрицательной",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Cost) });
			}
		}

		public override bool Equals(object obj)
		{
			var type = obj as FuelType;
			return type != null &&
				   Id == type.Id;
		}

		public override int GetHashCode()
		{
			return 2108858624 + Id.GetHashCode();
		}

		private decimal GetFuelPriceVersions()
		{
			var result = FuelPriceVersions.OrderByDescending(x => x.StartDate)?.FirstOrDefault()?.FuelPrice;

			return result.Value;
		}
	}
}

