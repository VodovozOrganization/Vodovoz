using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Organizations;
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
		private IList<OrganizationVersion> _organizationVersions = new List<OrganizationVersion>();
		private GenericObservableList<OrganizationVersion> _observableOrganizationVersions;
		private IObservableList<VatRateVersion> _observableVatRateVersions = new ObservableList<VatRateVersion>();
		private string _suffix;

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

		public virtual GenericObservableList<OrganizationVersion> ObservableOrganizationVersions => _observableOrganizationVersions
			?? (_observableOrganizationVersions = new GenericObservableList<OrganizationVersion>(OrganizationVersions));
		
		public virtual IObservableList<VatRateVersion> ObservableVatRateVersions 
			=> _observableVatRateVersions ?? (_observableVatRateVersions = new ObservableList<VatRateVersion>(VatRateVersions));

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
	}
}
