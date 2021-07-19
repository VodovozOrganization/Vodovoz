using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Flyers
{
	public class FlyerRepository : IFlyerRepository
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

		public IList<Flyer> GetAllActiveFlyers(IUnitOfWork uow)
		{
			Nomenclature flyerNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.JoinAlias(f => f.FlyerActionTimes, () => flyerActionTimeAlias)
				.Where(() => flyerActionTimeAlias.StartDate <= DateTime.Today)
				.And(() => flyerActionTimeAlias.EndDate == null || flyerActionTimeAlias.EndDate.Value > DateTime.Today)
				.List();
			
			return query;
		}
		
		public IList<int> GetAllActiveFlyersNomenclaturesIds(IUnitOfWork uow)
		{
			Nomenclature flyerNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;

			var query = uow.Session.QueryOver<Flyer>()
				.JoinAlias(f => f.FlyerNomenclature, () => flyerNomenclatureAlias)
				.JoinAlias(f => f.FlyerActionTimes, () => flyerActionTimeAlias)
				.Where(() => flyerActionTimeAlias.EndDate == null || flyerActionTimeAlias.EndDate.Value > DateTime.Today)
				.And(() => flyerActionTimeAlias.StartDate <= DateTime.Today)
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