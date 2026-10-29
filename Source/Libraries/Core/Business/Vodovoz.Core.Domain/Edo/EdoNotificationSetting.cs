using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Настройка уведомления ЭДО: текст (шаблон), получатели (email, диалоги Битрикс), отключено ли, можно ли повторять.
	/// У каждого уведомления свой пустой класс-наследник (папка NotificationSettings). Имя класса — это тип уведомления:
	/// оно хранится в колонке edo_notification_type, по нему NHibernate выбирает класс, а EdoNotificationsWorker находит
	/// настройку для пришедшего сообщения. Так же устроены источники проблем ЭДО (<see cref="EdoTaskProblemDescriptionSourceEntity"/>).
	///
	/// Как добавить новое уведомление (менять enum и схему таблицы не нужно):
	/// <list type="number">
	/// <item>Класс-наследник в Vodovoz.Core.Domain/Edo/NotificationSettings, например
	/// <c>public class XxxEdoNotificationSetting : EdoNotificationSetting { }</c>. Имя должно оканчиваться на EdoNotificationSetting.</item>
	/// <item>Маппинг в Vodovoz.Core.Data.NHibernate/Mapping/Edo/NotificationSettings:
	/// <c>SubclassMap&lt;XxxEdoNotificationSetting&gt;</c> с <c>DiscriminatorValue(nameof(XxxEdoNotificationSetting))</c>.</item>
	/// <item>Строка в edo_notification_settings — SQL-скриптом к выкладке (синхронизация модели БД данные не переносит).
	/// Обязательные колонки: <c>edo_notification_type</c> = имя класса и <c>name</c> (журнал, тема письма).
	/// Удобно сразу заполнить <c>template</c> — начальный текст с плейсхолдерами вида {OrderId}, их знает разработчик,
	/// и вставить строку выключенной (<c>notification_disabled</c> = 1), чтобы уведомление не рассылалось до настройки.
	/// Без строки EdoNotificationsWorker не найдёт настройку и сообщение упадёт с ошибкой.</item>
	/// <item>Публикация там, где возникает событие: <c>IEdoNotificationMessageFactory.Create&lt;XxxEdoNotificationSetting&gt;(("OrderId", ...), ...)</c>
	/// и <c>INotificationPublisher&lt;EdoNotificationMessage&gt;.TryPublishAsync(uow, notification)</c> в UoW, где меняются данные.
	/// Ключи параметров — те же плейсхолдеры, что в шаблоне.</item>
	/// <item>После выкладки в десктопе (диалог «Настройка уведомления по ЭДО») указать получателей, при необходимости
	/// поправить шаблон и снять «Уведомление отключено».</item>
	/// </list>
	/// Журнал и диалог настроек на десктопе менять не нужно: новая строка появится в журнале сама.
	/// Настройки, нужные только одному уведомлению, добавляются свойствами его класса (с маппингом и колонкой).
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки ЭДО уведомлений ",
		Nominative = "настройка ЭДО уведомления")]
	[EntityPermission]
	[HistoryTrace]
	public class EdoNotificationSetting : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _name;
		private string _template;
		private bool _notificationDisabled;
		private bool _allowDuplicateNotifications;
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
		/// Тип уведомления — имя класса настройки (колонка edo_notification_type, заполняет NHibernate)
		/// </summary>
		[Display(Name = "Тип уведомления")]
		public virtual string EdoNotificationType { get; protected set; }

		/// <summary>
		/// Название уведомления (журнал настроек, тема письма)
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
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
