using System;
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
		private string _name;
		private PhonePurpose _phonePurpose;

		public virtual int Id { get; set; }


		[Display(Name = "Тип телефона")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Назначение типа телефона")]
		public virtual PhonePurpose PhonePurpose
		{
			get => _phonePurpose;
			set => SetField(ref _phonePurpose, value);
		}

		public PhoneType()
		{
			Name = string.Empty;
		}

		#region IValidatableObject

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Укажите название типа телефона");
			}

			if(PhonePurpose == PhonePurpose.ForReceipts)
			{
				if(!(validationContext.GetService(typeof(IUnitOfWorkFactory)) is IUnitOfWorkFactory uowFactory))
				{
					throw new ArgumentException($"Для валидации должен быть доступен {nameof(IUnitOfWorkFactory)}");
				}

				if(!(validationContext.GetService(typeof(IPhoneRepository)) is IPhoneRepository phoneRepository))
				{
					throw new ArgumentException($"Для валидации должен быть доступен репозиторий {nameof(IPhoneRepository)}");
				}
				
				var phoneForReceipts = PhonePurpose.ForReceipts.GetEnumTitle();
				using(var uow = uowFactory.CreateWithoutRoot($"Проверка дублей типов телефонов {phoneForReceipts}"))
				{
					var existsForReceipts = phoneRepository.PhoneTypeWithPurposeExists(uow, PhonePurpose.ForReceipts);

					if(existsForReceipts != null && Id != existsForReceipts.Id)
					{
						yield return new ValidationResult(
							"В базе уже создан тип телефона с назначением " +
							$"'{phoneForReceipts}'. " +
							$"Не может быть создано более 1 типа телефона с назначением '{phoneForReceipts}'");
					}
				}
			}
		}

		#endregion
	}
}
