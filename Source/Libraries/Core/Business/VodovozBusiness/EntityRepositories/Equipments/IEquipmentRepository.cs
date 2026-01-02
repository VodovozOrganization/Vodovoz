using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Equipments
{
	public interface IEquipmentRepository
	{
		QueryOver<Equipment> GetEquipmentWithKindsQuery(List<EquipmentKind> kinds);
		Equipment GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature);
		IList<Equipment> GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature, int count = 0, int[] exceptIDs = null);
		Equipment GetAvailableEquipmentForRent(IUnitOfWork uow, EquipmentKind kind, int[] excludeEquipments);
		QueryOver<Equipment> AvailableOnDutyEquipmentQuery();
		QueryOver<Equipment, Equipment> AvailableEquipmentQuery();
		QueryOver<Equipment> GetEquipmentByNomenclature(Nomenclature nomenclature);
		QueryOver<Equipment> GetEquipmentAtDeliveryPointQuery(Counterparty client, DeliveryPoint deliveryPoint);
		IList<Equipment> GetEquipmentAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint);
		IList<Equipment> GetEquipmentForClient(IUnitOfWork uow, Counterparty counterparty);
		EquipmentLocation GetLocation(IUnitOfWork uow, int equipmentId);
	}
}
