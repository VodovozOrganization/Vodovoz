using Edo.Common.Services;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Documents.Services
{
	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public class UpdTransportCodeService : IUpdTransportCodeService
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodeHierarchyService _trueMarkCodeService;

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="trueMarkCodeService">Сервис иерархии кодов маркировки.</param>
		public UpdTransportCodeService(IUnitOfWork uow, ITrueMarkCodeHierarchyService trueMarkCodeService)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodeService = trueMarkCodeService ?? throw new ArgumentNullException(nameof(trueMarkCodeService));
		}

		/// <inheritdoc />
		public async Task DisaggregateCodeAsync(TrueMarkAnyCode code, CancellationToken cancellationToken)
		{
			var root = _trueMarkCodeService.GetParentGroupCode(_uow, code);
			if(root.IsTrueMarkTransportCode)
			{
				foreach(var innerCode in root.TrueMarkTransportCode.GetAllCodes().ToArray())
				{
					await innerCode.Match(
						transportCode => ClearTransportCodeAsync(transportCode, cancellationToken),
						groupCode => ClearTransportCodeAsync(groupCode, cancellationToken),
						identificationCode => ClearTransportCodeAsync(identificationCode, cancellationToken));
				}
			}

			await _trueMarkCodeService.DisaggregateRelatedCodesAsync(_uow, code, cancellationToken);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task DetachIncompleteTransportCodesAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken)
		{
			if(documentEdoTask is null)
			{
				throw new ArgumentNullException(nameof(documentEdoTask));
			}

			if(documentEdoTask.DocumentType != EdoDocumentType.UPD)
			{
				return;
			}

			var transportCodes = new Dictionary<int, TrueMarkTransportCode>();
			var documentGroupCounts = new Dictionary<int, int>();
			var documentIdentificationCodeCounts = new Dictionary<int, int>();
			var invalidTransportCodeIds = new HashSet<int>();

			foreach(var position in documentEdoTask.Upd.InventPositions)
			{
				var positionTransportCodes = new List<TrueMarkTransportCode>();
				var positionHasWrongQuantity = position.Codes.Sum(x => x.Quantity) != position.AssignedOrderItem.CurrentCount;

				foreach(var code in position.Codes)
				{
					if(code.GroupCode != null)
					{
						var groupCodes = code.GroupCode.GetAllCodes().ToArray();
						var identificationCodes = groupCodes.Where(x => x.IsTrueMarkIdentificationCode).ToArray();
						positionHasWrongQuantity |= identificationCodes.Length != code.Quantity;

						foreach(var groupCode in groupCodes.Where(x => x.IsTrueMarkGroupCode))
						{
							AddCount(documentGroupCounts, groupCode.TrueMarkGroupCode.Id);
						}

						foreach(var identificationCode in identificationCodes)
						{
							AddCount(documentIdentificationCodeCounts, identificationCode.TrueMarkIdentificationCode.Id);
						}

						AddTransportCode(code.GroupCode, transportCodes, positionTransportCodes);
					}
					else if(code.IndividualCode != null)
					{
						positionHasWrongQuantity |= code.Quantity != 1;
						AddCount(documentIdentificationCodeCounts, code.IndividualCode.Id);
						AddTransportCode(code.IndividualCode, transportCodes, positionTransportCodes);
					}
				}

				if(!positionHasWrongQuantity)
				{
					continue;
				}

				foreach(var transportCode in positionTransportCodes)
				{
					invalidTransportCodeIds.Add(transportCode.Id);
				}
			}

			foreach(var taskItem in documentEdoTask.Items)
			{
				if(taskItem.ProductCode.SourceCode != null)
				{
					AddTransportCode(taskItem.ProductCode.SourceCode, transportCodes);
				}

				if(taskItem.ProductCode.ResultCode != null)
				{
					AddTransportCode(taskItem.ProductCode.ResultCode, transportCodes);
				}
			}

			foreach(var transportCode in transportCodes.Values)
			{
				var allCodes = transportCode.GetAllCodes().ToArray();
				var hasWrongGroup = allCodes
					.Where(x => x.IsTrueMarkGroupCode)
					.Any(x => !documentGroupCounts.TryGetValue(x.TrueMarkGroupCode.Id, out var count) || count != 1);
				var hasWrongIdentificationCode = allCodes
					.Where(x => x.IsTrueMarkIdentificationCode)
					.Any(x => !documentIdentificationCodeCounts.TryGetValue(x.TrueMarkIdentificationCode.Id, out var count) || count != 1);

				if(!hasWrongGroup && !hasWrongIdentificationCode && !invalidTransportCodeIds.Contains(transportCode.Id))
				{
					continue;
				}

				foreach(var code in allCodes)
				{
					await code.Match(
						innerTransportCode => ClearTransportCodeAsync(innerTransportCode, cancellationToken),
						groupCode => ClearTransportCodeAsync(groupCode, cancellationToken),
						identificationCode => ClearTransportCodeAsync(identificationCode, cancellationToken));
				}
			}
		}

		private void AddTransportCode(
			TrueMarkAnyCode code,
			IDictionary<int, TrueMarkTransportCode> transportCodes,
			ICollection<TrueMarkTransportCode> positionTransportCodes = null)
		{
			var root = _trueMarkCodeService.GetParentGroupCode(_uow, code);
			if(!root.IsTrueMarkTransportCode)
			{
				return;
			}

			var transportCode = root.TrueMarkTransportCode;
			transportCodes[transportCode.Id] = transportCode;
			positionTransportCodes?.Add(transportCode);
		}

		private static void AddCount(IDictionary<int, int> counts, int id)
		{
			counts.TryGetValue(id, out var count);
			counts[id] = count + 1;
		}

		private async Task ClearTransportCodeAsync(TrueMarkTransportCode code, CancellationToken cancellationToken)
		{
			if(code.ParentTransportCodeId is null)
			{
				return;
			}

			code.ParentTransportCodeId = null;
			await _uow.SaveAsync(code, cancellationToken: cancellationToken);
		}

		private async Task ClearTransportCodeAsync(TrueMarkGroupCode code, CancellationToken cancellationToken)
		{
			if(code.ParentTransportCodeId is null)
			{
				return;
			}

			code.ParentTransportCodeId = null;
			await _uow.SaveAsync(code, cancellationToken: cancellationToken);
		}

		private async Task ClearTransportCodeAsync(TrueMarkIdentificationCode code, CancellationToken cancellationToken)
		{
			if(code.ParentTransportCodeId is null)
			{
				return;
			}

			code.ParentTransportCodeId = null;
			await _uow.SaveAsync(code, cancellationToken: cancellationToken);
		}
	}
}
