using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic.Organizations;

namespace Vodovoz.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация",
		GenitivePlural = "организаций")]
	[EntityPermission]
	[HistoryTrace]
	public class Organization : OrganizationEntity
	{
		private OrganizationVersion _activeOrganizationVersion;
		private IList<Phone> _phones = new List<Phone>();
		private GenericObservableList<OrganizationVersion> _observableOrganizationVersions;
		private IList<OrganizationVersion> _organizationVersions = new List<OrganizationVersion>();
		private string _suffix;
		private FinancialIncomeCategory _defaultCashIncomeCategory;

		[Display(Name = "Телефоны")]
		public virtual new IList<Phone> Phones {
			get => _phones;
			set => SetField(ref _phones, value);
		}

		public virtual new IList<OrganizationVersion> OrganizationVersions
		{
			get => _organizationVersions;
			set => SetField(ref _organizationVersions, value);
		}

		[Display(Name = "Суфикс организации")]
		public virtual string Suffix
		{
			get => _suffix;
			set => SetField(ref _suffix, value);
		}

		[Display(Name = "Статья дохода для наличной формы оплаты по умолчанию")]
		public virtual FinancialIncomeCategory DefaultCashIncomeCategory
		{
			get => _defaultCashIncomeCategory;
			set => SetField(ref _defaultCashIncomeCategory, value);
		}

		public virtual GenericObservableList<OrganizationVersion> ObservableOrganizationVersions => _observableOrganizationVersions
			?? (_observableOrganizationVersions = new GenericObservableList<OrganizationVersion>(OrganizationVersions));
		
		public virtual new OrganizationVersion OrganizationVersionOnDate(DateTime dateTime) =>
			ObservableOrganizationVersions.LastOrDefault(x =>
				x.StartDate <= dateTime && (x.EndDate == null || x.EndDate >= dateTime));

		[Display(Name = "Активная версия")]
		public virtual new OrganizationVersion ActiveOrganizationVersion =>
			_activeOrganizationVersion ?? OrganizationVersionOnDate(DateTime.Now);

		public virtual void SetActiveOrganizationVersion(OrganizationVersion organizationVersion)
		{
			_activeOrganizationVersion = organizationVersion;
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach (var result in base.Validate(validationContext))
			{
				yield return result;
			}

			if(DefaultCashIncomeCategory == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать статью дохода для наличной формы оплаты по умолчанию.",
					new[] { nameof(DefaultCashIncomeCategory) });
			}
		}
	}
}
