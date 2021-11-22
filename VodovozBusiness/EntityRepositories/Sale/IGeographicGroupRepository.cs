using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IGeographicGroupRepository
	{
		GeographicGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<District> source);
		IList<GeographicGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow);
		IList<GeographicGroup> GeographicGroupsWithCoordinatesExceptEast(
			IUnitOfWork uow, IGeographicGroupParametersProvider geographicGroupParametersProvider);
	}
}
