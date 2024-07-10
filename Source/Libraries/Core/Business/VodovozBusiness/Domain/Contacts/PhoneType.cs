﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "типы телефонов",
	Nominative = "тип телефона")]
	[EntityPermission]
	public class PhoneType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Тип телефона")]
		public virtual string Name { 
			get => name;
			set => SetField(ref name, value, () => Name); 
		}

		private PhonePurpose phonePurpose;
		[Display(Name = "Назначение типа телефона")]
		public virtual PhonePurpose PhonePurpose {
			get => phonePurpose;
			set => SetField(ref phonePurpose, value, () => PhonePurpose);
		}

		public PhoneType()
		{
			Name = String.Empty;
		}

		public virtual ValidationContext ConfigureValidationContext(IUnitOfWork uow, IPhoneRepository phoneRepository)
		{
			if(uow == null) {
				throw new ArgumentNullException(nameof(uow));
			}
			if(phoneRepository == null) {
				throw new ArgumentNullException(nameof(phoneRepository));
			}

			ValidationContext context = new ValidationContext(this, new Dictionary<object, object> {
				{"Reason", nameof(ConfigureValidationContext)}
			});

			context.InitializeServiceProvider(type =>
			{
				if(type == typeof(IUnitOfWork))
				{
					return uow;
				}

				if (type == typeof(IPhoneRepository))
				{
					return phoneRepository;
				}

				return null;
			});

			return context;
		}

		#region IValidatableObject

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Name))
				yield return new ValidationResult($"Укажите название типа телефона");

			if(validationContext.Items.ContainsKey("Reason") && (validationContext.Items["Reason"] as string) == nameof(ConfigureValidationContext)) {
				if(!(validationContext.GetService(typeof(IUnitOfWork)) is IUnitOfWork uow)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен UnitOfWork");
				}
				if(!(validationContext.GetService(typeof(IPhoneRepository)) is IPhoneRepository phoneRepository)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IPhoneRepository)}");
				}

				PhoneType phoneType = this;
				if(Id != 0)
					phoneType = uow.GetById<PhoneType>(this.Id);

				var existsForReceipts = phoneRepository.PhoneTypeWithPurposeExists(uow, PhonePurpose.ForReceipts);
				if(existsForReceipts != null && PhonePurpose == PhonePurpose.ForReceipts && phoneType.Id != existsForReceipts.Id) {
					yield return new ValidationResult($"В базе уже создан тип телефона с назначением " +
						$"'{PhonePurpose.ForReceipts.GetEnumTitle()}'. " +
						$"Не может быть создано более 1 типа телефона с назначением '{PhonePurpose.ForReceipts.GetEnumTitle()}'");
				}
			} else {
				throw new ArgumentException("Неверно передан ValidationContext");
			}

		}

		#endregion

	}

	public enum PhonePurpose
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts
	}

	public class PhonePurposeStringType : NHibernate.Type.EnumStringType
	{
		public PhonePurposeStringType() : base(typeof(PhonePurpose))
		{
		}
	}
}
