﻿using MoreLinq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;

namespace Vodovoz.Infrastructure.Persistance.TrueMark
{
	internal sealed class TrueMarkRepository : ITrueMarkRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public ISet<string> GetAllowedCodeOwnersInn()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Organization organizationAlias = null;
				var queryOrganization = uow.Session.QueryOver(() => organizationAlias)
					.Select(Projections.Property(() => organizationAlias.INN));
				var organizations = queryOrganization.List<string>();

				Counterparty counterpartyAlias = null;
				var queryCounterparty = uow.Session.QueryOver(() => counterpartyAlias)
					.Where(() => counterpartyAlias.CounterpartyType == CounterpartyType.Supplier)
					.Select(Projections.Property(() => counterpartyAlias.INN));
				var counterparties = queryCounterparty.List<string>();

				var innList = organizations.Union(counterparties);
				var result = innList.Distinct().ToHashSet();
				return result;
			}
		}

		public IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(codeIds)
					.List();
				return result;
			}
		}

		public IQueryable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode)
		{
			var query = uow.Session.Query<TrueMarkWaterIdentificationCode>()
				.Where(x => x.GTIN == gtin && x.SerialNumber == serialNumber && x.CheckCode == checkCode);

			return query;
		}
	}
}
