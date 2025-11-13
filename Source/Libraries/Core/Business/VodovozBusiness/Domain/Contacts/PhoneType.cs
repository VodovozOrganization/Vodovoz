using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.EntityRepositories;

namespace VodovozBusiness.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы телефонов",
		Nominative = "тип телефона")]
	[EntityPermission]
	public class PhoneType : PhoneTypeEntity, IValidatableObject
	{
		public virtual ValidationContext ConfigureValidationContext(IUnitOfWork uow, IPhoneRepository phoneRepository)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(phoneRepository == null)
			{
				throw new ArgumentNullException(nameof(phoneRepository));
			}

			if(PhonePurpose == PhonePurpose.ForReceipts)
			{
				if(!(validationContext.GetService(typeof(IUnitOfWorkFactory)) is IUnitOfWorkFactory uowFactory))
				{
					throw new ArgumentException($"Для валидации должен быть доступен {nameof(IUnitOfWorkFactory)}");
				}

				if(type == typeof(IPhoneRepository))
				{
					throw new ArgumentException($"Для валидации должен быть доступен репозиторий {nameof(IPhoneRepository)}");
				}
				
				var phoneForReceipts = PhonePurpose.ForReceipts.GetEnumTitle();
				using(var uow = uowFactory.CreateWithoutRoot($"Проверка дублей типов телефонов {phoneForReceipts}"))
				{
					var existsForReceipts = phoneRepository.PhoneTypeWithPurposeExists(uow, PhonePurpose.ForReceipts);

				return null;
			});

			return context;
		}

		#region IValidatableObject

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
				yield return new ValidationResult($"Укажите название типа телефона");

			if(validationContext.Items.ContainsKey("Reason") && validationContext.Items["Reason"] as string == nameof(ConfigureValidationContext))
			{
				if(!(validationContext.GetService(typeof(IUnitOfWork)) is IUnitOfWork uow))
				{
					throw new ArgumentException($"Для валидации отправки должен быть доступен UnitOfWork");
				}
				if(!(validationContext.GetService(typeof(IPhoneRepository)) is IPhoneRepository phoneRepository))
				{
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IPhoneRepository)}");
				}

				PhoneType phoneType = this;
				if(Id != 0)
					phoneType = uow.GetById<PhoneType>(Id);

				var existsForReceipts = phoneRepository.PhoneTypeWithPurposeExists(uow, PhonePurpose.ForReceipts);
				if(existsForReceipts != null && PhonePurpose == PhonePurpose.ForReceipts && phoneType.Id != existsForReceipts.Id)
				{
					yield return new ValidationResult($"В базе уже создан тип телефона с назначением " +
						$"'{PhonePurpose.ForReceipts.GetEnumTitle()}'. " +
						$"Не может быть создано более 1 типа телефона с назначением '{PhonePurpose.ForReceipts.GetEnumTitle()}'");
				}
			}
			else
			{
				throw new ArgumentException("Неверно передан ValidationContext");
			}
		}

		#endregion
	}
}
