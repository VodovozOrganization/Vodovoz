using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using DataAnnotationsExtensions;
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
		}

		#region Свойства

		public virtual int Id { get; set; }

		private string _name;
		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название организации должно быть заполнено.")]
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
		[Digits(ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength(12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN {
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		private string _kPP;
		[Display(Name = "КПП")]
		[Digits(ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength(9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP {
			get => _kPP;
			set => SetField(ref _kPP, value);
		}

		private string _oGRN;
		[Display(Name = "ОГРН/ОГРНИП")]
		[Digits(ErrorMessage = "ОГРН/ОГРНИП может содержать только цифры.")]
		[StringLength(15, MinimumLength = 0, ErrorMessage = "Номер ОГРНИП не должен превышать 15 цифр.")]
		public virtual string OGRN {
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}

		private string _oKPO;
		[Display(Name = "ОКПО")]
		[Digits(ErrorMessage = "ОКПО может содержать только цифры.")]
		[StringLength(10, MinimumLength = 8, ErrorMessage = "Номер ОКПО не должен превышать 10 цифр.")]
		public virtual string OKPO {
			get => _oKPO;
			set => SetField(ref _oKPO, value);
		}

		private string _oKVED;
		[Display(Name = "ОКВЭД")]
		[StringLength(100, ErrorMessage = "Номера ОКВЭД не должны превышать 100 знаков.")]
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
		}

		private IEnumerable<string> GetDuplicatedBankAccountNames() => Accounts
			.GroupBy(a => a.Name)
			.Where(g => g.Key != null && g.Count() > 1)
			.Select(g => g.Key)
			.ToList();
	}
}
