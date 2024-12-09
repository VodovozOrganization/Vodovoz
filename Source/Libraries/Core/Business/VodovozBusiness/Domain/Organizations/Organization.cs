using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация")]
	[EntityPermission]
	[HistoryTrace]
	public class Organization : AccountOwnerBase, IDomainObject, INamed, IValidatableObject
	{
		private int? _avangardShopId;
		private string _taxcomEdoAccountId;
		private IList<OrganizationVersion> _organizationVersions = new List<OrganizationVersion>();
		private OrganizationVersion _activeOrganizationVersion;
		private GenericObservableList<OrganizationVersion> _observableOrganizationVersions;

		public Organization()
		{
			Name = "Новая организация";
			FullName = string.Empty;
			INN = string.Empty;
			KPP = string.Empty;
			OGRN = string.Empty;
			Email = string.Empty;
			OKPO = string.Empty;
			OKVED = string.Empty;
		}

		#region Свойства

		public virtual int Id { get; set; }

		private string _name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		private string _fullName;
		[Display(Name = "Полное название")]
		public virtual string FullName {
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		private string _iNN;
		[Display(Name = "ИНН")]
		public virtual string INN {
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		private string _kPP;
		[Display(Name = "КПП")]
		public virtual string KPP {
			get => _kPP;
			set => SetField(ref _kPP, value);
		}

		private string _oGRN;
		[Display(Name = "ОГРН/ОГРНИП")]
		public virtual string OGRN {
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}

		private string _oKPO;
		[Display(Name = "ОКПО")]
		public virtual string OKPO {
			get => _oKPO;
			set => SetField(ref _oKPO, value);
		}

		private string _oKVED;
		[Display(Name = "ОКВЭД")]
		public virtual string OKVED {
			get => _oKVED;
			set => SetField(ref _oKVED, value);
		}

		private IList<Phone> _phones;
		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get => _phones;
			set => SetField(ref _phones, value);
		}

		private string _email;
		[Display(Name = "E-mail адреса")]
		public virtual string Email {
			get => _email;
			set => SetField(ref _email, value);
		}

		private int? _cashBoxId;
		[Display(Name = "ID Кассового аппарата")]
		public virtual int? CashBoxId {
			get => _cashBoxId;
			set => SetField(ref _cashBoxId, value);
		}

		private bool _withoutVAT;
		[Display(Name = "Без НДС")]
		public virtual bool WithoutVAT {
			get => _withoutVAT;
			set => SetField(ref _withoutVAT, value);
		}

		private StoredResource _stamp;
		[Display(Name = "Печать")]
		public virtual StoredResource Stamp
		{
			get => _stamp;
			set => SetField(ref _stamp, value);
		}

		[IgnoreHistoryTrace]
		[Display(Name = "Id организации в Авангарде")]
		public virtual int? AvangardShopId
		{
			get => _avangardShopId;
			set => SetField(ref _avangardShopId, value);
		}
		
		[IgnoreHistoryTrace]
		[Display(Name = "Id кабинета в Такскоме")]
		public virtual string TaxcomEdoAccountId
		{
			get => _taxcomEdoAccountId;
			set => SetField(ref _taxcomEdoAccountId, value);
		}

		#endregion

		public virtual IList<OrganizationVersion> OrganizationVersions
		{
			get => _organizationVersions;
			set => SetField(ref _organizationVersions, value);
		}

		public virtual GenericObservableList<OrganizationVersion> ObservableOrganizationVersions => _observableOrganizationVersions
			?? (_observableOrganizationVersions = new GenericObservableList<OrganizationVersion>(OrganizationVersions));

		public virtual OrganizationVersion OrganizationVersionOnDate(DateTime dateTime) =>
			ObservableOrganizationVersions.LastOrDefault(x =>
				x.StartDate <= dateTime && (x.EndDate == null || x.EndDate >= dateTime));

		[Display(Name = "Активная версия")]
		public virtual OrganizationVersion ActiveOrganizationVersion =>
			_activeOrganizationVersion ?? OrganizationVersionOnDate(DateTime.Now);

		public virtual void SetActiveOrganizationVersion(OrganizationVersion organizationVersion)
		{
			_activeOrganizationVersion = organizationVersion;
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
					"Номер ОКПО должен содержать минимум 8 цифр.",
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

		private IEnumerable<string> GetDuplicatedBankAccountNames() => Accounts
			.GroupBy(a => a.Name)
			.Where(g => g.Key != null && g.Count() > 1)
			.Select(g => g.Key)
			.ToList();
	}
}
