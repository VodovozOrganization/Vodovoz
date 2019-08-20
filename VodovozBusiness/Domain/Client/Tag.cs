using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "теги",
		Nominative = "тег")]
	[EntityPermission]
	public class Tag : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required(ErrorMessage = "Название тега должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string colorText;

		[Display(Name = "Цвет строки")]
		public virtual string ColorText {
			get { return colorText; }
			set { SetField(ref colorText, value, () => ColorText); }
		}

		#endregion

		public Tag()
		{
			Name = String.Empty;
		}

		public static IUnitOfWorkGeneric<Tag> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<Tag>();
			return uow;
		}
	}
}