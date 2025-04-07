using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	internal sealed class TrueMarkCodeRepository : ITrueMarkCodeRepository
	{
		private readonly IUnitOfWork _uow;

		private Dictionary<int, TrueMarkWaterGroupCode> _waterGroupCodes = new Dictionary<int, TrueMarkWaterGroupCode>();
		private Dictionary<int, TrueMarkTransportCode> _transportCodes = new Dictionary<int, TrueMarkTransportCode>();

		public TrueMarkCodeRepository(IUnitOfWork uow)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
		}

		public async Task PreloadCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			_waterGroupCodes.Clear();
			_transportCodes.Clear();

			var transportCodeIds = new HashSet<int>(
				codes.Where(x => x.ParentTransportCodeId.HasValue)
					.Select(x => x.ParentTransportCodeId.Value)
			);

			var groupCodeIds = codes.Where(x => x.ParentWaterGroupCodeId.HasValue)
				.Select(x => x.ParentWaterGroupCodeId.Value)
				.Distinct()
				.ToArray();

			while(groupCodeIds.Any())
			{
				var groupCodes = await _uow.Session.QueryOver<TrueMarkWaterGroupCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(groupCodeIds)
					.ListAsync(cancellationToken);
				groupCodeIds = groupCodes.Where(x => x.ParentWaterGroupCodeId.HasValue)
					.Select(x => x.ParentWaterGroupCodeId.Value)
					.Distinct()
					.ToArray();

				foreach(var groupCode in groupCodes)
				{
					if(!_waterGroupCodes.ContainsKey(groupCode.Id))
					{
						_waterGroupCodes.Add(groupCode.Id, groupCode);
					}
				}

				foreach(var groupCode in groupCodes.Where(x => x.ParentTransportCodeId.HasValue))
				{
					if(!transportCodeIds.Contains(groupCode.ParentTransportCodeId.Value))
					{
						transportCodeIds.Add(groupCode.ParentTransportCodeId.Value);
					}
				}
			}

			var transportCodes = await _uow.Session.QueryOver<TrueMarkTransportCode>()
				.WhereRestrictionOn(x => x.Id).IsIn(transportCodeIds.ToArray())
				.ListAsync(cancellationToken);
			_transportCodes = transportCodes.ToDictionary(x => x.Id);
		}

		public async Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterIdentificationCode code, 
			CancellationToken cancellationToken
			)
		{
			if(code.ParentTransportCodeId.HasValue)
			{
				return await GetTransportCode(code.ParentTransportCodeId.Value, cancellationToken);
			}

			if(code.ParentWaterGroupCodeId.HasValue)
			{
				return null;
			}

			var nextGroupCodeId = code.ParentWaterGroupCodeId;
			while(nextGroupCodeId.HasValue)
			{
				var groupCode = await GetGroupCode(nextGroupCodeId.Value, cancellationToken);

				if(groupCode.ParentTransportCodeId.HasValue)
				{
					return await GetTransportCode(groupCode.ParentTransportCodeId.Value, cancellationToken);
				}

				if(groupCode.ParentWaterGroupCodeId.HasValue)
				{
					return null;
				}

				nextGroupCodeId = groupCode.ParentWaterGroupCodeId;
			}

			return null;
		}

		public async Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterGroupCode code,
			CancellationToken cancellationToken
			)
		{
			if(code.ParentTransportCodeId.HasValue)
			{
				return await GetTransportCode(code.ParentTransportCodeId.Value, cancellationToken);
			}

			if(code.ParentWaterGroupCodeId.HasValue)
			{
				return null;
			}

			var nextGroupCodeId = code.ParentWaterGroupCodeId;
			while(nextGroupCodeId.HasValue)
			{
				var groupCode = await GetGroupCode(nextGroupCodeId.Value, cancellationToken);

				if(groupCode.ParentTransportCodeId.HasValue)
				{
					return await GetTransportCode(groupCode.ParentTransportCodeId.Value, cancellationToken);
				}

				if(groupCode.ParentWaterGroupCodeId.HasValue)
				{
					return null;
				}

				nextGroupCodeId = groupCode.ParentWaterGroupCodeId;
			}

			return null;
		}

		public async Task<TrueMarkWaterGroupCode> GetGroupCode(int id, CancellationToken cancellationToken)
		{
			if(!_waterGroupCodes.TryGetValue(id, out var groupCode))
			{
				groupCode = await _uow.Session.GetAsync<TrueMarkWaterGroupCode>(id, cancellationToken);
				_waterGroupCodes.Add(id, groupCode);
			}

			return groupCode;
		}

		private async Task<TrueMarkTransportCode> GetTransportCode(int id, CancellationToken cancellationToken)
		{
			if(!_transportCodes.TryGetValue(id, out var transportCode))
			{
				transportCode = await _uow.Session.GetAsync<TrueMarkTransportCode>(id, cancellationToken);
				_transportCodes.Add(id, transportCode);
			}

			return transportCode;
		}
	}
}
