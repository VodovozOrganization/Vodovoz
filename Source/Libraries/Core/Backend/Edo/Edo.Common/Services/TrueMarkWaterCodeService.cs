using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Common.Services
{
	public class TrueMarkWaterCodeService : ITrueMarkWaterCodeService
	{
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _trueMarkWaterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkTransportCode> _trueMarkTransportCodeRepository;

		public TrueMarkWaterCodeService(
			ILogger<TrueMarkWaterCodeService> logger,
			IUnitOfWork uow,
			IGenericRepository<TrueMarkWaterGroupCode> trueMarkWaterGroupCodeRepository,
			IGenericRepository<TrueMarkTransportCode> trueMarkTransportCodeRepository
			)
		{
			_trueMarkWaterGroupCodeRepository = trueMarkWaterGroupCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterGroupCodeRepository));
			_trueMarkTransportCodeRepository = trueMarkTransportCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkTransportCodeRepository));
		}

		public async Task DisaggregateRelatedCodesAsync(
			IUnitOfWork unitOfWork,
			TrueMarkAnyCode anyCode,
			CancellationToken cancellationToken = default)
		{
			if(anyCode == null)
			{
				return;
			}

			var root = GetParentGroupCode(unitOfWork, anyCode);

			var allCodes = root.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode })
			.ToArray();

			foreach(var code in allCodes)
			{
				await DisaggregateSingleCodeAsync(unitOfWork, code, cancellationToken);
			}
		}

		private async Task DisaggregateSingleCodeAsync(
			IUnitOfWork unitOfWork,
			TrueMarkAnyCode code,
			CancellationToken cancellationToken)
		{
			await code.Match(
				transportCode =>
				{
					transportCode.ParentTransportCodeId = null;
					return unitOfWork.SaveAsync(transportCode, cancellationToken: cancellationToken);
				},
				groupCode =>
				{
					groupCode.ParentTransportCodeId = null;
					groupCode.ParentWaterGroupCodeId = null;
					return unitOfWork.SaveAsync(groupCode, cancellationToken: cancellationToken);
				},
				waterCode =>
				{
					waterCode.ParentTransportCodeId = null;
					waterCode.ParentWaterGroupCodeId = null;
					return unitOfWork.SaveAsync(waterCode, cancellationToken: cancellationToken);
				}
			);
		}


		public TrueMarkAnyCode GetParentGroupCode(IUnitOfWork unitOfWork, TrueMarkAnyCode trueMarkAnyCode)
		{
			if(trueMarkAnyCode == null)
			{
				throw new ArgumentNullException(nameof(trueMarkAnyCode), "Передано пустое значение в параметр кода");
			}

			return trueMarkAnyCode.Match(
				transportCode =>
				{
					if(transportCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == transportCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					return transportCode;
				},
				groupCode =>
				{
					if(groupCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == groupCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					if(groupCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkWaterGroupCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == groupCode.ParentWaterGroupCodeId,
									1)
								.FirstOrDefault());
					}

					return groupCode;
				},
				waterCode =>
				{
					if(waterCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(unitOfWork,
							_trueMarkWaterGroupCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == waterCode.ParentWaterGroupCodeId,
									1)
								.FirstOrDefault());
					}

					if(waterCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == waterCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					return waterCode;
				});
		}
	}
}
