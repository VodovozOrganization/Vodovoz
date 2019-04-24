using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Часть города",
		NominativePlural = "Части города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeographicGroup : BusinessObjectBase<ScheduleRestrictedDistrict>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название части города должно быть заполнено")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		decimal? baseLatitude;
		[Display(Name = "Широта координат базы")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? BaseLatitude {
			get => baseLatitude;
			protected set => SetField(ref baseLatitude, value, () => BaseLatitude);
		}

		decimal? baseLongitude;
		[Display(Name = "Долгота координат базы")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? BaseLongitude {
			get => baseLongitude;
			protected set => SetField(ref baseLongitude, value, () => BaseLongitude);
		}

		#region calculated properties
		public virtual bool BaseCoordinatesExist => BaseLatitude.HasValue && BaseLongitude.HasValue;

		public virtual string CoordinatesText => BaseCoordinatesExist ? string.Format("(ш. {0:F5}, д. {1:F5})", BaseLatitude, BaseLongitude) : string.Empty;
		#endregion

		public virtual void SetСoordinates(decimal? latitude, decimal? longitude)
		{
			if(!EqualCoords(BaseLatitude, latitude) || !EqualCoords(BaseLongitude, longitude)) {
				BaseLatitude = latitude;
				BaseLongitude = longitude;
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
			if(!BaseCoordinatesExist)
				yield return new ValidationResult(
					"Укажите координаты базы, обслуживающей эту часть города",
					new[] {
						this.GetPropertyName(o => o.BaseLatitude),
						this.GetPropertyName(o => o.BaseLongitude)
					}
				);
		}
	}
}