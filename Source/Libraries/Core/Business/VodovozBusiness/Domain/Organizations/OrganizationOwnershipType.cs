﻿using System;
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
		private IUnitOfWork _uow;

		public OrganizationOwnershipType()
		{
			_uow = UnitOfWorkFactory.CreateWithoutRoot();

			Abbreviation = string.Empty;
			FullName = string.Empty;
		}

		#region Свойства
		public virtual int Id { get; set; }

		string _abbreviation;
		[Display(Name = "Аббревиатура")]
		public virtual string Abbreviation
		{
			get => _abbreviation;
			set => SetField(ref _abbreviation, value);
		}

		string _fullName;
		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		bool _isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		#endregion

		public virtual string Title => Abbreviation;

		public static IUnitOfWorkGeneric<OrganizationOwnershipType> Create() => UnitOfWorkFactory.CreateWithNewRoot<OrganizationOwnershipType>();

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

			if(!(validationContext.ServiceContainer.GetService(typeof(IOrganizationRepository)) is IOrganizationRepository organizationRepository))
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
