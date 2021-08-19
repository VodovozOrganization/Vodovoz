using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IGeographicGroupRepository
	{
		GeographicGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<District> source);
		IList<GeographicGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow);
	}
}