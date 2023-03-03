using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Формы собственности",
		Nominative = "Форма собственности"
	)]
	[EntityPermission]
	public class OrganizationOwnershipType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string abbreviation;
		[Display(Name = "Аббревиатура")]
		public virtual string Abbreviation
		{
			get => abbreviation;
			set => SetField(ref abbreviation, value, () => Abbreviation);
		}

		string fullName;
		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => fullName;
			set => SetField(ref fullName, value, () => FullName);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}
		#endregion

		public OrganizationOwnershipType()
		{
			Abbreviation = string.Empty;
			FullName = string.Empty;
		}

		public static IUnitOfWorkGeneric<OrganizationOwnershipType> Create() => UnitOfWorkFactory.CreateWithNewRoot<OrganizationOwnershipType>();

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Abbreviation))
				yield return new ValidationResult("Необходимо заполнить поле \"Аббревиатура\".");

			if(string.IsNullOrWhiteSpace(FullName))
				yield return new ValidationResult("Необходимо заполнить поле \"Полное название\".");

			//TODO Проверка наличия в базе такой аббревиатуры
		}
	}
}
