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
	/// Проверяет состав транспортных кодов перед отправкой УПД заказа.
	/// </summary>
	public class UpdTransportCodeService
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		/// <summary>
		/// Создаёт сервис проверки транспортных кодов УПД.
		/// </summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="trueMarkWaterCodeService">Сервис иерархии кодов маркировки.</param>
		public UpdTransportCodeService(IUnitOfWork uow, ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

		/// <summary>
		/// Снимает связи с транспортниками, состав которых не соответствует позициям УПД.
		/// </summary>
		/// <param name="documentEdoTask">Задача создания УПД.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача проверки и сохранения изменённых кодов.</returns>
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
			var documentWaterCodeCounts = new Dictionary<int, int>();
			var invalidTransportCodeIds = new HashSet<int>();

			foreach(var position in documentEdoTask.UpdInventPositions)
			{
				var positionTransportCodes = new List<TrueMarkTransportCode>();
				var positionHasWrongQuantity = position.Codes.Sum(x => x.Quantity) != position.AssignedOrderItem.CurrentCount;

				foreach(var code in position.Codes)
				{
					if(code.GroupCode != null)
					{
						var groupCodes = code.GroupCode.GetAllCodes().ToArray();
						var waterCodes = groupCodes.Where(x => x.IsTrueMarkWaterIdentificationCode).ToArray();
						positionHasWrongQuantity |= waterCodes.Length != code.Quantity;

						foreach(var groupCode in groupCodes.Where(x => x.IsTrueMarkWaterGroupCode))
						{
							AddCount(documentGroupCounts, groupCode.TrueMarkWaterGroupCode.Id);
						}

						foreach(var waterCode in waterCodes)
						{
							AddCount(documentWaterCodeCounts, waterCode.TrueMarkWaterIdentificationCode.Id);
						}

						AddTransportCode(code.GroupCode, transportCodes, positionTransportCodes);
					}
					else if(code.IndividualCode != null)
					{
						positionHasWrongQuantity |= code.Quantity != 1;
						AddCount(documentWaterCodeCounts, code.IndividualCode.Id);
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
					.Where(x => x.IsTrueMarkWaterGroupCode)
					.Any(x => !documentGroupCounts.TryGetValue(x.TrueMarkWaterGroupCode.Id, out var count) || count != 1);
				var hasWrongWaterCode = allCodes
					.Where(x => x.IsTrueMarkWaterIdentificationCode)
					.Any(x => !documentWaterCodeCounts.TryGetValue(x.TrueMarkWaterIdentificationCode.Id, out var count) || count != 1);

				if(!hasWrongGroup && !hasWrongWaterCode && !invalidTransportCodeIds.Contains(transportCode.Id))
				{
					continue;
				}

				foreach(var code in allCodes)
				{
					await code.Match(
						innerTransportCode => ClearTransportCodeAsync(innerTransportCode, cancellationToken),
						groupCode => ClearTransportCodeAsync(groupCode, cancellationToken),
						waterCode => ClearTransportCodeAsync(waterCode, cancellationToken));
				}
			}
		}

		private void AddTransportCode(
			TrueMarkAnyCode code,
			IDictionary<int, TrueMarkTransportCode> transportCodes,
			ICollection<TrueMarkTransportCode> positionTransportCodes = null)
		{
			var root = _trueMarkWaterCodeService.GetParentGroupCode(_uow, code);
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

		private async Task ClearTransportCodeAsync(TrueMarkWaterGroupCode code, CancellationToken cancellationToken)
		{
			if(code.ParentTransportCodeId is null)
			{
				return;
			}

			code.ParentTransportCodeId = null;
			await _uow.SaveAsync(code, cancellationToken: cancellationToken);
		}

		private async Task ClearTransportCodeAsync(TrueMarkWaterIdentificationCode code, CancellationToken cancellationToken)
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
