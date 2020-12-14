using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class CashDistributionCommonOrganisationProvider : ICashDistributionCommonOrganisationProvider
    {
        private readonly OrganizationParametersProvider organisationParametersProvider;

        public CashDistributionCommonOrganisationProvider(OrganizationParametersProvider organisationParametersProvider)
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