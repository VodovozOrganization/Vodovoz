using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QSOrmProject;

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
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		#endregion

		public ClientCameFrom()
		{
			Name = String.Empty;
		}

		public static IUnitOfWorkGeneric<ClientCameFrom> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<ClientCameFrom>();
			return uow;
		}

		/*public static QueryOver<ClientCameFrom> GetClientCameFromQuery(IUnitOfWork uow)
		{
			return QueryOver.Of<ClientCameFrom>().Where(x => x.Name != "-");
		}*/

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult("Необходимо заполнить название.");
		}
	}
}
