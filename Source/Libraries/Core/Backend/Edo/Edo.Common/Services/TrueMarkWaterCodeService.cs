using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Common.Services
{
	public class TrueMarkWaterCodeService : ITrueMarkWaterCodeService
	{
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _trueMarkWaterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkTransportCode> _trueMarkTransportCodeRepository;
		private readonly IGenericRepository<EdoUpdInventPositionCode> _edoUpdInventPositionCodeRepository;
		private readonly IGenericRepository<FiscalInventPosition> _fiscalInventPositionRepository;
		private readonly IGenericRepository<TransferOrderTrueMarkCode> _transferOrderTrueMarkCodeRepository;

		public TrueMarkWaterCodeService(
			ILogger<TrueMarkWaterCodeService> logger,
			IUnitOfWork uow,
			IGenericRepository<TrueMarkWaterGroupCode> trueMarkWaterGroupCodeRepository,
			IGenericRepository<TrueMarkTransportCode> trueMarkTransportCodeRepository,
			IGenericRepository<EdoUpdInventPositionCode> edoUpdInventPositionCodeRepository,
			IGenericRepository<FiscalInventPosition> fiscalInventPositionRepository,
			IGenericRepository<TransferOrderTrueMarkCode> transferOrderTrueMarkCodeRepository
			)
		{
			_trueMarkWaterGroupCodeRepository = trueMarkWaterGroupCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterGroupCodeRepository));
			_trueMarkTransportCodeRepository = trueMarkTransportCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkTransportCodeRepository));
			_edoUpdInventPositionCodeRepository = edoUpdInventPositionCodeRepository
				?? throw new ArgumentNullException(nameof(edoUpdInventPositionCodeRepository));
			_fiscalInventPositionRepository = fiscalInventPositionRepository
				?? throw new ArgumentNullException(nameof(fiscalInventPositionRepository));
			_transferOrderTrueMarkCodeRepository = transferOrderTrueMarkCodeRepository
				?? throw new ArgumentNullException(nameof(transferOrderTrueMarkCodeRepository));
		}

		/// <inheritdoc/>
		public async Task DeleteRelatedGroupAndTransportCodesAsync(
			IUnitOfWork unitOfWork,
			TrueMarkAnyCode anyCode,
			CancellationToken cancellationToken = default)
		{
			if(anyCode == null)
			{
				return;
			}

			var root = GetParentGroupCode(unitOfWork, anyCode);

			if(root.IsTrueMarkWaterIdentificationCode)
			{
				return;
			}

			var allCodes = root.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode })
				.ToArray();
			var groupCodeIds = allCodes
				.Where(x => x.IsTrueMarkWaterGroupCode)
				.Select(x => x.TrueMarkWaterGroupCode.Id)
				.ToArray();

			await ClearEdoDocumentGroupCodeReferencesAsync(
				unitOfWork,
				groupCodeIds,
				cancellationToken);

			root.Match(
				transportCode =>
				{
					transportCode.ClearAllCodes();
					return true;
				},
				groupCode =>
				{
					groupCode.ClearAllCodes();
					return true;
				},
				waterCode => true);

			foreach(var code in allCodes)
			{
				await DisaggregateSingleCodeAsync(unitOfWork, code, cancellationToken);
			}

			foreach(var code in allCodes.Reverse())
			{
				await code.Match(
					transportCode => unitOfWork.DeleteAsync(transportCode, cancellationToken),
					groupCode => unitOfWork.DeleteAsync(groupCode, cancellationToken),
					waterCode => Task.CompletedTask);
			}
		}

		private async Task ClearEdoDocumentGroupCodeReferencesAsync(
			IUnitOfWork unitOfWork,
			IReadOnlyCollection<int> groupCodeIds,
			CancellationToken cancellationToken)
		{
			if(groupCodeIds.Count == 0)
			{
				return;
			}

			var updPositionCodes = _edoUpdInventPositionCodeRepository.Get(
				unitOfWork,
				x => x.GroupCode != null && groupCodeIds.Contains(x.GroupCode.Id));

			foreach(var positionCode in updPositionCodes)
			{
				positionCode.GroupCode = null;
				await unitOfWork.SaveAsync(positionCode, cancellationToken: cancellationToken);
			}

			var fiscalPositions = _fiscalInventPositionRepository.Get(
				unitOfWork,
				x => x.GroupCode != null && groupCodeIds.Contains(x.GroupCode.Id));

			foreach(var position in fiscalPositions)
			{
				position.GroupCode = null;
				await unitOfWork.SaveAsync(position, cancellationToken: cancellationToken);
			}

			var transferOrderCodes = _transferOrderTrueMarkCodeRepository.Get(
				unitOfWork,
				x => x.GroupCode != null && groupCodeIds.Contains(x.GroupCode.Id));

			foreach(var transferOrderCode in transferOrderCodes)
			{
				transferOrderCode.GroupCode = null;
				await unitOfWork.SaveAsync(transferOrderCode, cancellationToken: cancellationToken);
			}
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
