using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Часть города",
		NominativePlural = "Части города")]
	[EntityPermission]
	public class GeographicGroup : BusinessObjectBase<ScheduleRestrictedDistrict>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название района города должно быть заполнено")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		decimal? latitude;
		[Display(Name = "Широта")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Latitude {
			get { return latitude; }
			protected set { SetField(ref latitude, value, () => Latitude); }
		}

		decimal? longitude;
		[Display(Name = "Долгота")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Longitude {
			get { return longitude; }
			protected set { SetField(ref longitude, value, () => Longitude); }
		}


		#region calculated properties
		public virtual bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

		public virtual string CoordinatesText => HasCoordinates ? string.Format("(ш. {0:F5}, д. {1:F5})", Latitude, Longitude) : string.Empty;
		#endregion

		public virtual void SetСoordinates(decimal? latitude, decimal? longitude)
		{
			if(!EqualCoords(Latitude, latitude) || !EqualCoords(Longitude, longitude)) {
				Latitude = latitude;
				Longitude = longitude;
			}
		}

		/// <summary>
		/// Сравнивает координаты с точностью 6 знаков после запятой
		/// </summary>
		/// <returns><c>true</c>, Если координаты равны, <c>false</c> иначе.</returns>
		public virtual bool EqualCoords(decimal? coord1, decimal? coord2)
		{
			if(coord1.HasValue && coord2.HasValue) {
				decimal CoordDiff = Math.Abs(coord1.Value - coord2.Value);
				return Math.Round(CoordDiff, 6) == decimal.Zero;
			}

			return false;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!HasCoordinates)
				yield return new ValidationResult(
					"Укажите координаты базы, обслуживающей этот район города",
					new[] {
						this.GetPropertyName(o => o.Latitude),
						this.GetPropertyName(o => o.Longitude)
					}
				);
		}
	}
}