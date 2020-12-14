using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Services
{
    public interface ICashDistributionCommonOrganisationProvider
    {
        Organization GetCommonOrganisation(IUnitOfWork uow);
    }
}