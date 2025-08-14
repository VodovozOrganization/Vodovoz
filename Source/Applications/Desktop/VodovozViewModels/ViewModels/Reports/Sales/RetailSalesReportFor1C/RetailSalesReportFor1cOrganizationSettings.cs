using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1C
{
	public class RetailSalesReportFor1cOrganizationSettings
	{
		public RetailSalesReportFor1cOrganizationSettings(Organization organization)
		{
			Organization = organization;
			OrganizationVersions = Organization.OrganizationVersions.ToList();
			Phone = organization.Phones.FirstOrDefault()?.ToString();
		}
		public Organization Organization { get;  }
		public IList<OrganizationVersion> OrganizationVersions { get;  }
		public string Phone { get; }
	}
}
