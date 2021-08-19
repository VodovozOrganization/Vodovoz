using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IGeographicGroupRepository
	{
		GeographicGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<SectorVersion> source);
		IList<GeographicGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow);
	}
}