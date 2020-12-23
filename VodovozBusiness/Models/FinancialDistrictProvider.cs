using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Models
{
    public class FinancialDistrictProvider : IFinancialDistrictProvider
    {
        public FinancialDistrict GetFinancialDistrictOrNull(IUnitOfWork uow, decimal latitude, decimal longitude)
        {
            FinancialDistrict financialDistrictAlias = null;
            FinancialDistrictsSet financialDistrictsSetAlias = null;

            var districts = uow.Session.QueryOver(() => financialDistrictAlias)
                .Left.JoinAlias(() => financialDistrictAlias.FinancialDistrictsSet, () => financialDistrictsSetAlias)
                .Where(() => financialDistrictsSetAlias.Status == DistrictsSetStatus.Active)
                .List();

            Point point = new Point((double)latitude, (double)longitude);

            var availableDistricts = districts.Where(x => x.Border.Contains(point));
            
            return availableDistricts.FirstOrDefault();
        }
    }
}