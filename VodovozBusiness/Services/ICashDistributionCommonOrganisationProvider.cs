using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.Services
{
    public interface ICashDistributionCommonOrganisationProvider
    {
        Organization GetCommonOrganisation(IUnitOfWork uow);
    }
}