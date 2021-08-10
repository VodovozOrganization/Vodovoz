using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sectors
{
	public interface ISectorsRepository
	{

		/// <summary>
		/// Возвращает список районов в границы которых входят указанные координаты
		/// </summary>

		IList<SectorVersion> GetSectorVersionInCoordinates(IUnitOfWork uow, decimal latitude, decimal longitude);
		
		IList<SectorVersion> GetSectorVersions(IUnitOfWork uow, DateTime? fromActivationDate, SectorsSetStatus? status);

		IList<SectorDeliveryRuleVersion> GetSectorDeliveryRules(IUnitOfWork uow, Sector sector);

		IList<SectorWeekDayScheduleVersion> GetSectorWeekDayRules(IUnitOfWork uow, Sector sector);
		
		IList<DeliveryPointSectorVersion> GetDeliveryPointSectorVersions(IUnitOfWork uow, Sector sector);
	}
}
