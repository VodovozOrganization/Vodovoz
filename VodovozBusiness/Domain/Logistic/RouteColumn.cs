using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "колонки в маршрутном листе",
		Nominative = "колонка маршрутного листа")]
	public class RouteColumn : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		[StringLength(20)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		public RouteColumn ()
		{
			Name = String.Empty;
		}
	}
}

