using EdoNotifications.Contracts;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Vodovoz.Core.Domain.Edo
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки ЭДО уведомлений ",
		Nominative = "настройка ЭДО уведомления")]
	[EntityPermission]
	[HistoryTrace]
	public class EdoNotificationSetting : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _template;
		private bool _notificationDisabled;
		private bool _allowDuplicateNotifications;
		private EdoNotificationType _edoNotificationType;
		private string _emails;
		private string _bitrixDialogs;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип сообщения для ЭДО уведомления
		/// </summary>
		[Display(Name = "Тип события для ЭДО уведомления")]
		public virtual EdoNotificationType EdoNotificationType
		{
			get => _edoNotificationType;
			set => SetField(ref _edoNotificationType, value);
		}

		/// <summary>
		/// Шаблон уведомления
		/// </summary>
		[Display(Name = "Шаблон уведомления")]
		public virtual string Template
		{
			get => _template;
			set => SetField(ref _template, value);
		}

		/// <summary>
		/// Email адреса для отправки через точку с запятой
		/// </summary>
		[Display(Name = "Email адреса для отправки через точку с запятой")]
		public virtual string Emails
		{
			get => _emails;
			set => SetField(ref _emails, value);
		}

		/// <summary>
		/// Идентификаторы диалогов Битрикс для отправки через точку с запятой
		/// </summary>
		[Display(Name = "Идентификаторы диалогов Битрикс для отправки через точку с запятой")]
		public virtual string BitrixDialogs
		{
			get => _bitrixDialogs;
			set => SetField(ref _bitrixDialogs, value);
		}

		/// <summary>
		/// Не отправлять
		/// </summary>
		[Display(Name = "Не отправлять")]
		public virtual bool NotificationDisabled
		{
			get => _notificationDisabled;
			set => SetField(ref _notificationDisabled, value);
		}

		/// <summary>
		/// Разрешить повторные отправки
		/// </summary>
		[Display(Name = "Разрешить повторные отправки")]
		public virtual bool AllowDuplicateNotifications
		{
			get => _allowDuplicateNotifications;
			set => SetField(ref _allowDuplicateNotifications, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Template))
			{
				yield return new ValidationResult("Шаблон уведомления должен быть заполнен)",
					new[] { nameof(Template) });
			}

			var emails = Emails
				.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(e => e.Trim())
				.ToList();

			var emailValidator = new EmailAddressAttribute();

			var invalidEmails = emails
				.Where(e => !emailValidator.IsValid(e))
				.ToList();

			if(invalidEmails.Any())
			{
				yield return new ValidationResult($"Некорректные email: {string.Join(", ", invalidEmails)}",
					new[] { nameof(Emails) });
			}
		}

		public override string ToString() =>
			$"Настройка ЭДО уведомления {Id} для типа уведомления {EdoNotificationType}";
	}
}
