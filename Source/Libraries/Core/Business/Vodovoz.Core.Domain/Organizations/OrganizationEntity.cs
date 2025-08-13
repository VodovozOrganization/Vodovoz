using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.StoredResources;

namespace Vodovoz.Core.Domain.Organizations
{
	/// <summary>
	/// Организация
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация")]
	[EntityPermission]
	[HistoryTrace]
	public class OrganizationEntity : AccountOwnerBase, INamedDomainObject, IValidatableObject
	{
		private int _id;
		private string _name;
		private string _fullName;
		private string _iNN;
		private string _kPP;
		private string _oGRN;
		private string _oKPO;
		private string _oKVED;
		private string _email;
		private int? _cashBoxId;
		private bool _withoutVAT;
		private int? _avangardShopId;
		private OrganizationEdoType _organizationEdoType;
		private Guid? _cashBoxTokenFromTrueMark;

		private OrganizationVersionEntity _activeOrganizationVersion;
		private StoredResource _stamp;

		private IObservableList<PhoneEntity> _phones = new ObservableList<PhoneEntity>();
		private IObservableList<OrganizationVersionEntity> _organizationVersions = new ObservableList<OrganizationVersionEntity>();

		public OrganizationEntity()
		{
			Name = "Новая организация";
			FullName = string.Empty;
			INN = string.Empty;
			KPP = string.Empty;
			OGRN = string.Empty;
			Email = string.Empty;
		}

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
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Полное название
		/// </summary>
		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		/// <summary>
		/// ИНН
		/// </summary>
		[Display(Name = "ИНН")]
		public virtual string INN
		{
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		/// <summary>
		/// КПП
		/// </summary>
		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kPP;
			set => SetField(ref _kPP, value);
		}

		/// <summary>
		/// ОГРН/ОГРНИП
		/// </summary>
		[Display(Name = "ОГРН/ОГРНИП")]
		public virtual string OGRN
		{
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}

		/// <summary>
		/// ОКПО
		/// </summary>
		[Display(Name = "ОКПО")]
		public virtual string OKPO
		{
			get => _oKPO;
			set => SetField(ref _oKPO, value);
		}

		/// <summary>
		/// ОКВЭД
		/// </summary>
		[Display(Name = "ОКВЭД")]
		public virtual string OKVED
		{
			get => _oKVED;
			set => SetField(ref _oKVED, value);
		}

		/// <summary>
		/// E-mail адреса
		/// </summary>
		[Display(Name = "E-mail адреса")]
		public virtual string Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		/// <summary>
		/// ID Кассового аппарата
		/// </summary>
		[Display(Name = "ID Кассового аппарата")]
		public virtual int? CashBoxId
		{
			get => _cashBoxId;
			set => SetField(ref _cashBoxId, value);
		}

		/// <summary>
		/// Без НДС
		/// </summary>
		[Display(Name = "Без НДС")]
		public virtual bool WithoutVAT
		{
			get => _withoutVAT;
			set => SetField(ref _withoutVAT, value);
		}

		/// <summary>
		/// Печать
		/// </summary>
		[Display(Name = "Печать")]
		public virtual StoredResource Stamp
		{
			get => _stamp;
			set => SetField(ref _stamp, value);
		}

		/// <summary>
		/// Id организации в Авангарде
		/// </summary>
		[IgnoreHistoryTrace]
		[Display(Name = "Id организации в Авангарде")]
		public virtual int? AvangardShopId
		{
			get => _avangardShopId;
			set => SetField(ref _avangardShopId, value);
		}

		/// <summary>
		/// Телефоны
		/// </summary>
		[Display(Name = "Телефоны")]
		public virtual IObservableList<PhoneEntity> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		[Display(Name = "Тип участия в ЭДО")]
		public virtual OrganizationEdoType OrganizationEdoType
		{
			get => _organizationEdoType;
			set => SetField(ref _organizationEdoType, value);
		}

		/// <summary>
		/// Токен кассового аппарата, полученный в ЧЗ
		/// нужен для отправки в заголовках запросов для проверки разрешительного режима
		/// </summary>
		[Display(Name = "Токен кассового аппарата, полученный в ЧЗ")]
		public virtual Guid? CashBoxTokenFromTrueMark
		{
			get => _cashBoxTokenFromTrueMark;
			set => SetField(ref _cashBoxTokenFromTrueMark, value);
		}
		
		/// <summary>
		/// Различные параметры для ЭДО Такскома
		/// </summary>
		[IgnoreHistoryTrace]
		[Display(Name = "Конфигурации ЭДО по Такскому")]
		public virtual TaxcomEdoSettings TaxcomEdoSettings { get; }

		/// <summary>
		/// Версии
		/// </summary>
		[Display(Name = "Версии")]
		public virtual IObservableList<OrganizationVersionEntity> OrganizationVersions
		{
			get => _organizationVersions;
			set => SetField(ref _organizationVersions, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var duplicatedBankAccountNames = GetDuplicatedBankAccountNames();

			if(duplicatedBankAccountNames.Count() > 0)
			{
				yield return new ValidationResult(
					$"Название банковского счета повторяется несколько раз: {string.Join(",", duplicatedBankAccountNames)}",
					new[] { nameof(Accounts) });
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Название организации должно быть заполнено.",
					new[] { nameof(Name) });
			}

			if(!Regex.IsMatch(INN, @"^\d+$"))
			{
				yield return new ValidationResult(
					"ИНН может содержать только цифры.",
					new[] { nameof(INN) });
			}

			if(INN.Length > 12)
			{
				yield return new ValidationResult(
					"Номер ИНН не должен превышать 12.",
					new[] { nameof(INN) });
			}

			if(!Regex.IsMatch(KPP, @"^\d+$"))
			{
				yield return new ValidationResult(
					"КПП может содержать только цифры.",
					new[] { nameof(KPP) });
			}

			if(KPP.Length > 9)
			{
				yield return new ValidationResult(
					"Номер КПП не должен превышать 9 цифр.",
					new[] { nameof(KPP) });
			}

			if(!Regex.IsMatch(OGRN, @"^\d+$"))
			{
				yield return new ValidationResult(
					"ОГРН/ОГРНИП может содержать только цифры.",
					new[] { nameof(OGRN) });
			}

			if(OGRN.Length > 15)
			{
				yield return new ValidationResult(
					"Номер ОГРНИП не должен превышать 15 цифр.",
					new[] { nameof(OGRN) });
			}

			if(!Regex.IsMatch(OKPO, @"^\d+$"))
			{
				yield return new ValidationResult(
					"ОКПО может содержать только цифры.",
					new[] { nameof(OKPO) });
			}

			if(OKPO.Length < 8)
			{
				yield return new ValidationResult(
					"Номер ОКПО не должен содержать минимум 8 цифр.",
					new[] { nameof(OKPO) });
			}

			if(OKPO.Length > 10)
			{
				yield return new ValidationResult(
				"Номер ОКПО не должен превышать 10 цифр.",
					new[] { nameof(OKPO) });
			}

			if(OKVED.Length > 100)
			{
				yield return new ValidationResult(
					"Номера ОКВЭД не должны превышать 100 знаков.",
					new[] { nameof(OKVED) });
			}
		}

		/// <summary>
		/// Получение списка дубликатов названий расчетных счетов
		/// </summary>
		/// <returns>Список дублей названий расчетных счетов</returns>
		private IEnumerable<string> GetDuplicatedBankAccountNames() => Accounts
			.GroupBy(a => a.Name)
			.Where(g => g.Key != null && g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		public virtual OrganizationVersionEntity OrganizationVersionOnDate(DateTime dateTime) =>
			OrganizationVersions.LastOrDefault(x =>
				x.StartDate <= dateTime && (x.EndDate == null || x.EndDate >= dateTime));

		[Display(Name = "Активная версия")]
		public virtual OrganizationVersionEntity ActiveOrganizationVersion =>
			_activeOrganizationVersion ?? OrganizationVersionOnDate(DateTime.Now);
	}
}

