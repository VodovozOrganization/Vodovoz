using System;
using Vodovoz.Domain.Logistic.Organizations;

namespace Vodovoz.Controllers
{
	public interface IOrganizationVersionsController
	{
		OrganizationVersion CreateAndAddVersion(DateTime? startDate = null);

		void ChangeVersionStartDate(OrganizationVersion version, DateTime newStartDate);

		bool IsValidDateForNewOrganizationVersion(DateTime dateTime);

		bool IsValidDateForVersionStartDateChange(OrganizationVersion version, DateTime newStartDate);

	}
}
