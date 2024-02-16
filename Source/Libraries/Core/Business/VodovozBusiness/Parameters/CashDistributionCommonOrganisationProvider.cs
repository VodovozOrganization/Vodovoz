using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Parameters
{
	public class CashDistributionCommonOrganisationProvider : ICashDistributionCommonOrganisationProvider
    {
        private readonly IOrganizationSettings organisationParametersProvider;

        public CashDistributionCommonOrganisationProvider(IOrganizationSettings organisationParametersProvider)
        {
            this.organisationParametersProvider =
                organisationParametersProvider ?? throw new ArgumentNullException(nameof(organisationParametersProvider));
        }

        public Organization GetCommonOrganisation(IUnitOfWork uow)
        {
            return uow.GetById<Organization>(organisationParametersProvider.CommonCashDistributionOrganisationId);
        }
    }
}