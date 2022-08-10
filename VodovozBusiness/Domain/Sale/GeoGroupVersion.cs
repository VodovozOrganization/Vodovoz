using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Versions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Версия части города",
		NominativePlural = "Версии частей города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeoGroupVersion : VersionEntityBase, IValidatableObject
	{
		private Employee _author;
		private GeoGroup _geoGroup;
		private decimal? _baseLatitude;
		private decimal? _baseLongitude;
		private Subdivision _cashSubdivision;
		private Warehouse _warehouse;


		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Часть города")]
		public virtual GeoGroup GeoGroup
		{
			get => _geoGroup;
			set => SetField(ref _geoGroup, value);
		}

		[Display(Name = "Широта координат базы")]
		[PropertyChangedAlso(nameof(CoordinatesText))]
		public virtual decimal? BaseLatitude
		{
			get => _baseLatitude;
			set => SetField(ref _baseLatitude, value);
		}

		[Display(Name = "Долгота координат базы")]
		[PropertyChangedAlso(nameof(CoordinatesText))]
		public virtual decimal? BaseLongitude
		{
			get => _baseLongitude;
			set => SetField(ref _baseLongitude, value);
		}

		[Display(Name = "Касса")]
		public virtual Subdivision CashSubdivision
		{
			get => _cashSubdivision;
			set => SetField(ref _cashSubdivision, value);
		}

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		public virtual bool BaseCoordinatesExist => BaseLatitude.HasValue && BaseLongitude.HasValue;
		public virtual string CoordinatesText => BaseCoordinatesExist ? string.Format("(ш. {0:F5}, д. {1:F5})", BaseLatitude, BaseLongitude) : string.Empty;

		public virtual void SetСoordinates(decimal? latitude, decimal? longitude)
		{
			if(!EqualCoords(BaseLatitude, latitude) || !EqualCoords(BaseLongitude, longitude))
			{
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
			if(coord1.HasValue && coord2.HasValue)
			{
				decimal CoordDiff = Math.Abs(coord1.Value - coord2.Value);
				return Math.Round(CoordDiff, 6) == decimal.Zero;
			}

			return false;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!BaseCoordinatesExist)
			{
				yield return new ValidationResult(
					"Укажите координаты базы, обслуживающей эту часть города",
					new[] { nameof(BaseLatitude), nameof(BaseLongitude) }
				);
			}

			if(CashSubdivision == null)
			{
				yield return new ValidationResult("Необходимо указать кассу", new[] { nameof(CashSubdivision) });
			}
			else if(!CashSubdivision.IsCashSubdivision)
			{
				yield return new ValidationResult("Выбранное подразделение не является кассой", new[] { nameof(CashSubdivision) });
			}

			if(Warehouse == null)
			{
				yield return new ValidationResult("Необходимо указать склад", new[] { nameof(Warehouse) });
			}
		}

		#endregion IValidatableObject implementation
	}
}
