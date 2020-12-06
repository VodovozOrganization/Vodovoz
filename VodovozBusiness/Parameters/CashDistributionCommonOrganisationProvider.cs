using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class CashDistributionCommonOrganisationProvider : ICashDistributionCommonOrganisationProvider
    {
        private readonly OrganisationParametersProvider organisationParametersProvider;

        public CashDistributionCommonOrganisationProvider(OrganisationParametersProvider organisationParametersProvider)
        {
            this.organisationParametersProvider =
                organisationParametersProvider ?? throw new ArgumentNullException(nameof(organisationParametersProvider));
        }

        public Organization GetCommonOrganisation(IUnitOfWork uow)
        {
            return uow.GetById<Organization>(organisationParametersProvider.CommonCashOrganisationDistributionId);
        }
    }
}