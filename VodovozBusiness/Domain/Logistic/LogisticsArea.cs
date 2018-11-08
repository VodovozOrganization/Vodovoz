using System;
using System.ComponentModel.DataAnnotations;
using GeoAPI.Geometries;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace Vodovoz.Domain.Logistic
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "логистические районы",
		Nominative = "логистический район")]
	public class LogisticsArea: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		private bool isCity;

		[Display(Name = "Черта города")]
		public virtual bool IsCity {
			get { return isCity; }
			set { SetField(ref isCity, value, () => IsCity); }
		}

		private IGeometry geometry;

		[Display(Name = "Полигон района")]
		public virtual IGeometry Geometry {
			get { return geometry; }
			set { SetField(ref geometry, value, () => Geometry); }
		}

		#endregion

		public LogisticsArea ()
		{
			Name = String.Empty;
		}
	}
}

