using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IGeographicGroupRepository
	{
		GeoGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<District> source);
		IList<GeoGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow, bool isActiveOnly = false);
		IList<GeoGroupVersion> GetGeographicGroupVersionsOnDate(IUnitOfWork uow, DateTime date);
		IList<GeoGroup> GeographicGroupsWithCoordinatesExceptEast(
			IUnitOfWork uow, IGeographicGroupSettings geographicGroupSettings);
		IList<GeoGroup> GeographicGroupsWithoutEast(
			IUnitOfWork uow, IGeographicGroupSettings geographicGroupSettings);
	}
}
