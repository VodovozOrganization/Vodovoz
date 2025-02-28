using MoreLinq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using VodovozBusiness.Domain.Goods;

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
					.Where(x => x.OrganizationEdoType != OrganizationEdoType.WithoutEdo)
					.Select(Projections.Property(() => organizationAlias.INN));
				var organizations = queryOrganization.List<string>();

				var result = organizations.Distinct().ToHashSet();
				return result;
			}
		}

		public ISet<string> GetAllowedCodeOwnersGtins()
		{
			using(var unitOfWork = _uowFactory.CreateWithoutRoot("Get our Gtins"))
			{
				var result =
				(
					from nomenclatures in unitOfWork.Session.Query<Gtin>()
					select nomenclatures.GtinNumber
				)
				.Distinct()
				.ToHashSet();

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

		public IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(
			IUnitOfWork uow,
			string gtin,
			string serialNumber,
			string checkCode)
		{
			var query = uow.Session.Query<TrueMarkWaterIdentificationCode>()
				.Where(x => x.GTIN == gtin && x.SerialNumber == serialNumber && x.CheckCode == checkCode);

			return query.ToList();
		}
	}
}
