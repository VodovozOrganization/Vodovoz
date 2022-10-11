using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.Models
{
    public interface IFinancialDistrictProvider
    {
        FinancialDistrict GetFinancialDistrictOrNull(IUnitOfWork uow, decimal latitude, decimal longitude);
    }
}