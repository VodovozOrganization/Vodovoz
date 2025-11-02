using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using TrueMark.Contracts.Responses;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public class Tag1260Saver
	{
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;

		public Tag1260Saver(TrueMarkWaterCodeParser trueMarkWaterCodeParser)
		{
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
		}

		public void SaveTag1260CodesCheckResult(IUnitOfWork unitOfWork, IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes,
			CodeCheckResponse codeCheckResponse)
		{
			foreach(var sourceCode in sourceCodes)
			{
				var codeForTag1260 = _trueMarkWaterCodeParser.GetProductCodeForTag1260(sourceCode);
				var codeCheckInfo = codeCheckResponse.Codes.FirstOrDefault(x => x.Cis.Equals(codeForTag1260));

				if(codeCheckInfo == null)
				{
					continue;
				}

				var tag1260CodeCheckResult = new Tag1260CodeCheckResult
				{
					ReqId = codeCheckResponse.ReqId,
					ReqTimestamp = codeCheckResponse.ReqTimestamp,
				};

				sourceCode.Tag1260CodeCheckResult = tag1260CodeCheckResult;

				unitOfWork.Save(sourceCode.Tag1260CodeCheckResult);

				sourceCode.IsTag1260Valid =
					codeCheckInfo.ErrorCode == 0 && codeCheckInfo.Found && codeCheckInfo.Valid && codeCheckInfo.Verified
					&& codeCheckInfo.ExpireDate > DateTime.Now && codeCheckInfo.Realizable && codeCheckInfo.Utilised
					&& !codeCheckInfo.IsBlocked && !codeCheckInfo.Sold;

				unitOfWork.Save(sourceCode);
			}
		}
	}
}
