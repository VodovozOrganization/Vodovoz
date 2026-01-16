using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Organizations;

namespace Vodovoz.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Типы форм собственности",
		Nominative = "Тип формы собственности"
	)]
	[EntityPermission]
	public class OrganizationOwnershipType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _abbreviation;
		private string _fullName;
		private string _code;
		private bool _isArchive;
		
		public OrganizationOwnershipType()
		{
			Abbreviation = string.Empty;
			FullName = string.Empty;
		}

		public virtual int Id { get; set; }

		[Display(Name = "Аббревиатура")]
		public virtual string Abbreviation
		{
			get => _abbreviation;
			set => SetField(ref _abbreviation, value);
		}

		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}
		
		[Display(Name = "Код ОПФ")]
		public virtual string Code
		{
			get => _code;
			set => SetField(ref _code, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual string Title => Abbreviation;

		#region IValidatableObject implementation
		public virtual bool CheckForAbbreviationDuplicate(IUnitOfWork uow, IOrganizationRepository organizationRepository)
		{
			var organizationOwnershipTypes = organizationRepository.GetOrganizationOwnershipTypeByAbbreviation(uow, Abbreviation);
			if(organizationOwnershipTypes == null)
			{
				return false;
			}	
			if(organizationOwnershipTypes.Any(x => x.Id != Id))
			{
				return true;
			}
			return false;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Abbreviation))
			{
				yield return new ValidationResult("Необходимо заполнить поле \"Аббревиатура\".");
			}

			if(string.IsNullOrWhiteSpace(FullName))
			{
				yield return new ValidationResult("Необходимо заполнить поле \"Полное название\".");
			}

			if(!(validationContext.GetService(typeof(IUnitOfWork)) is IUnitOfWork uow))
			{
				throw new ArgumentException($"Для валидации типа организации должен быть доступен UnitOfWork");
			}

			if(!(validationContext.GetService(typeof(IOrganizationRepository)) is IOrganizationRepository organizationRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(IOrganizationRepository)}");
			}

			if (CheckForAbbreviationDuplicate(uow, organizationRepository))
			{
				yield return new ValidationResult($"Запись для формы собственности \"{Abbreviation}\" уже существует.");
			}
		}
		#endregion
	}
}
