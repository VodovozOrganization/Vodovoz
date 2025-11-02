using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Откуда клиенты",
		Nominative = "Откуда клиент"
	)]
	[EntityPermission]
	public class ClientCameFrom : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		#endregion

		public ClientCameFrom()
		{
			Name = string.Empty;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult("Необходимо заполнить название.");
		}
	}
}
