using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Library;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Sender
{
	public class ReceiptCorrectionIndustryRequisitePreparer
	{
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IEdoReceiptSettings _edoReceiptSettings;

		public ReceiptCorrectionIndustryRequisitePreparer(
			Tag1260Checker tag1260Checker,
			IEdoReceiptSettings edoReceiptSettings)
		{
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
		}

		public async Task<string> PrepareAsync(
			IUnitOfWork uow,
			EdoFiscalDocument fiscalDocument,
			CancellationToken cancellationToken)
		{
			if(fiscalDocument.DocumentType == FiscalDocumentType.Sale
				|| fiscalDocument.DocumentType == FiscalDocumentType.SaleCorrection)
			{
				foreach(var position in fiscalDocument.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
				{
					if(GetMark(position) != null)
					{
						position.IndustryRequisiteData = null;
					}
				}
			}

			var positions = fiscalDocument.InventPositions?
				.Where(x => string.IsNullOrWhiteSpace(x.IndustryRequisiteData) && GetMark(x) != null)
				.ToList();

			if(positions == null || !positions.Any())
			{
				return null;
			}

			var token = fiscalDocument.ReceiptEdoTask?.FormalEdoRequest?.Order?.Contract?.Organization?.CashBoxTokenFromTrueMark;
			if(token == null)
			{
				return "Отсутствует токен Честного знака для разрешительного режима.";
			}

			var regulatoryDocument = uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(
				_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId);
			if(regulatoryDocument == null)
			{
				return "Не найден регламентирующий документ отраслевого реквизита.";
			}

			var result = await _tag1260Checker.CheckCodesForTag1260Async(
				positions.Select(GetMark).Distinct(),
				token.Value,
				cancellationToken);

			if(result.Code != 0)
			{
				return $"Код ошибки разрешительного режима: {result.Code}, сообщение: {result.Description}";
			}

			if(result.Codes == null || !result.Codes.Any())
			{
				return "Честный знак не вернул данные разрешительного режима.";
			}

			var docData = $"UUID={result.ReqId}&Time={result.ReqTimestamp}";
			var byMark = positions
				.GroupBy(GetMark)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var codeResult in result.Codes)
			{
				if(!CanUseMark(codeResult, fiscalDocument.DocumentType))
				{
					return $"Код {codeResult.Cis} не прошёл разрешительный режим.";
				}

				if(!byMark.TryGetValue(codeResult.Cis, out var markedPositions))
				{
					continue;
				}

				foreach(var position in markedPositions)
				{
					position.IndustryRequisiteData = docData;
					position.RegulatoryDocument = regulatoryDocument;
				}
			}

			if(positions.Any(x => string.IsNullOrWhiteSpace(x.IndustryRequisiteData)))
			{
				return "Честный знак не вернул разрешительный режим для всех кодов чека.";
			}

			return null;
		}

		private static string GetMark(FiscalInventPosition position)
		{
			var productCode = position.EdoTaskItem?.ProductCode;
			if(productCode?.ResultCode != null)
			{
				return productCode.ResultCode.FormatForCheck1260;
			}

			if(productCode?.SourceCode != null)
			{
				return productCode.SourceCode.FormatForCheck1260;
			}

			return position.GroupCode?.FormatForCheck1260;
		}

		private static bool CanUseMark(CodeCheckInfo codeResult, FiscalDocumentType documentType)
		{
			var isReturn = documentType == FiscalDocumentType.Return
				|| documentType == FiscalDocumentType.SaleReturnCorrection;

			return codeResult.ErrorCode == 0
				&& codeResult.Found
				&& codeResult.Valid
				&& codeResult.Verified
				&& codeResult.ExpireDate > DateTime.Now
				&& !codeResult.IsBlocked
				&& (isReturn || (codeResult.Realizable && codeResult.Utilised && !codeResult.Sold));
		}
	}
}
