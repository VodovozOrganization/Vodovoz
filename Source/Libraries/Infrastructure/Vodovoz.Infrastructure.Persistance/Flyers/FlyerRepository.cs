using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Flyers;

namespace Vodovoz.Infrastructure.Persistance.Flyers
{
	internal sealed class FlyerRepository : IFlyerRepository
	{
		public IList<int> GetAllFlyersNomenclaturesIds(IUnitOfWork uow)
		{
			Nomenclature flyerNomenclatureAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.SelectList(list => list.Select(() => flyerNomenclatureAlias.Id))
				.List<int>();

			return query;
		}

		public IList<Flyer> GetAllActiveFlyersByDate(IUnitOfWork uow, DateTime deliveryDate)
		{
			Nomenclature flyerNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.JoinAlias(f => f.FlyerActionTimes, () => flyerActionTimeAlias)
				.Where(() => flyerActionTimeAlias.StartDate <= deliveryDate)
				.And(() => flyerActionTimeAlias.EndDate == null || flyerActionTimeAlias.EndDate.Value > deliveryDate)
				.List();

			return query;
		}

		public IList<int> GetAllActiveFlyersNomenclaturesIdsByDate(IUnitOfWork uow, DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<int>();
			}

			Nomenclature flyerNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.JoinAlias(f => f.FlyerActionTimes, () => flyerActionTimeAlias)
				.Where(() => flyerActionTimeAlias.EndDate == null || flyerActionTimeAlias.EndDate.Value > deliveryDate.Value)
				.And(() => flyerActionTimeAlias.StartDate <= deliveryDate.Value)
				.SelectList(list => list.Select(() => flyerNomenclatureAlias.Id))
				.List<int>();

			return query;
		}

		public bool ExistsFlyerForNomenclatureId(IUnitOfWork uow, int nomenclatureId)
		{
			Nomenclature flyerNomenclatureAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.Where(() => flyerNomenclatureAlias.Id == nomenclatureId)
				.SingleOrDefault();

			return query != null;
		}
	}
}
