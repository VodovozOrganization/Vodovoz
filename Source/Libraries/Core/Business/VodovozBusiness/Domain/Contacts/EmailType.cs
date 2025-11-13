using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autofac;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы e-mail",
		Nominative = "тип e-mail")]
	[EntityPermission]
	public class EmailType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "E-mail")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private EmailPurpose emailPurpose;
		[Display(Name = "Дополнительный тип")]
		public virtual EmailPurpose EmailPurpose {
			get => emailPurpose;
			set => SetField(ref emailPurpose, value, () => EmailPurpose);
		}
		#endregion

		public EmailType()
		{
			Name = String.Empty;
		}

		public virtual ValidationContext ConfigureValidationContext(IUnitOfWork uow, IEmailRepository emailRepository)
		{
			if(uow == null) {
				throw new ArgumentNullException(nameof(uow));
			}
			if(emailRepository == null) {
				throw new ArgumentNullException(nameof(emailRepository));
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

				if (type == typeof(IEmailRepository))
				{
					return emailRepository;
				}

				return null;
			});

			return context;
		}

		#region IValidatableObject

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Name))
				yield return new ValidationResult($"Укажите название типа e-mail");

			if(validationContext.Items.ContainsKey("Reason") && (validationContext.Items["Reason"] as string) == nameof(ConfigureValidationContext)) {
				if(!(validationContext.GetService(typeof(IUnitOfWork)) is IUnitOfWork uow)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен UnitOfWork");
				}
				if(!(validationContext.GetService(typeof(IEmailRepository)) is IEmailRepository emailRepository)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IEmailRepository)}");
				}

				EmailType emailType = this;
				if(Id != 0)
					emailType = uow.GetById<EmailType>(this.Id);

				var existsForReceipts = emailRepository.EmailTypeWithPurposeExists(uow, EmailPurpose.ForReceipts);
				if(existsForReceipts != null && EmailPurpose == EmailPurpose.ForReceipts && emailType.Id != existsForReceipts.Id) {
					yield return new ValidationResult($"В базе уже создан тип e-mail адреса с назначением " +
						$"'{EmailPurpose.ForReceipts.GetEnumTitle()}'. " +
						$"Не может быть создано более 1 типа e-mail адреса с назначением '{EmailPurpose.ForReceipts.GetEnumTitle()}'");
				}

				var existsForBills = emailRepository.EmailTypeWithPurposeExists(uow, EmailPurpose.ForBills);
				if(existsForBills != null && EmailPurpose == EmailPurpose.ForBills && emailType.Id != existsForBills.Id) {
					yield return new ValidationResult($"В базе уже создан тип e-mail адреса с назначением " +
						$"'{EmailPurpose.ForBills.GetEnumTitle()}'. " +
						$"Не может быть создано более 1 типа e-mail адреса с назначением '{EmailPurpose.ForBills.GetEnumTitle()}'");
				}
			} else {
				throw new ArgumentException("Неверно передан ValidationContext");
			}

		}

		#endregion
	}

	public enum EmailPurpose
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts,
		[Display(Name = "Для счетов")]
		ForBills,
		[Display(Name = "Рабочий")]
		Work,
		[Display(Name = "Личный")]
		Personal
	}

	public class EmailPurposeStringType : NHibernate.Type.EnumStringType
	{
		public EmailPurposeStringType() : base(typeof(EmailPurpose))
		{
		}
	}
}
