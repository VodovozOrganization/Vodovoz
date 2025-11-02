using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Versions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Версия части города",
		NominativePlural = "Версии частей города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeoGroupVersion : VersionEntityBase, IValidatableObject, IDomainObject
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
		public virtual decimal? BaseLatitude
		{
			get => _baseLatitude;
			set => SetField(ref _baseLatitude, value);
		}

		[Display(Name = "Долгота координат базы")]
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
		public virtual GMap.NET.PointLatLng GmapPoint => new GMap.NET.PointLatLng((double)BaseLatitude, (double)BaseLongitude);
		public virtual PointCoordinates PointCoordinates => new PointCoordinates(BaseLatitude, BaseLongitude);

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

		private string GetCreationDateTitle => CreationDate.ToString("dd.MM.yyyy HH.mm");

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!BaseCoordinatesExist)
			{
				yield return new ValidationResult(
					$"В версии на {GetCreationDateTitle} необходимо указать координаты базы",
					new[] { nameof(BaseLatitude), nameof(BaseLongitude) }
				);
			}

			if(CashSubdivision == null)
			{
				yield return new ValidationResult($"В версии на {GetCreationDateTitle} необходимо указать кассу", new[] { nameof(CashSubdivision) });
			}
			else if(!CashSubdivision.IsCashSubdivision)
			{
				yield return new ValidationResult($"В версии на {GetCreationDateTitle} выбранное подразделение не является кассой", new[] { nameof(CashSubdivision) });
			}

			if(Warehouse == null)
			{
				yield return new ValidationResult($"В версии на {GetCreationDateTitle} необходимо указать склад", new[] { nameof(Warehouse) });
			}
		}

		#endregion IValidatableObject implementation
	}
}
