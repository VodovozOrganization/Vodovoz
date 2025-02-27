using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TrueMark.Library;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public class Tag1260Updater
	{
		private readonly ITrueMarkOrganizationClientSettingProvider _trueMarkOrganizationClientSettingProvider;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly Tag1260Saver _tag1260Saver;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;

		public Tag1260Updater(
			ITrueMarkOrganizationClientSettingProvider trueMarkOrganizationClientSettingProvider,
			Tag1260Checker tag1260Checker,
			Tag1260Saver tag1260Saver,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser
		)
		{
			_trueMarkOrganizationClientSettingProvider = trueMarkOrganizationClientSettingProvider
				?? throw new ArgumentNullException(nameof(trueMarkOrganizationClientSettingProvider));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_tag1260Saver = tag1260Saver ?? throw new ArgumentNullException(nameof(tag1260Saver));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
		}

		public async Task UpdateTag1260Info(IUnitOfWork unitOfWork, IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes,
			int organizationId, CancellationToken cancellationToken)
		{
			var trueMarkOrganizationClientSetting = _trueMarkOrganizationClientSettingProvider.GetModulKassaOrganizationSettings();

			var headerKey = trueMarkOrganizationClientSetting?
				.FirstOrDefault(x => x.OrganizationId == organizationId)?
				.HeaderTokenApiKey;

			if(headerKey == null)
			{
				throw new TrueMarkException(
					$"Невозможно проверить коды по тэгу 1260, т.к. в настройках отсутвует header для ораганизации {organizationId}");
			}

			var codesToCheck = sourceCodes.Select(x => _trueMarkWaterCodeParser.GetProductCodeForTag1260(x));

			var checkResult = await _tag1260Checker.CheckCodesForTag1260Async(codesToCheck, headerKey.Value, cancellationToken);

			_tag1260Saver.SaveTag1260CodesCheckResult(unitOfWork, sourceCodes, checkResult);
		}
	}
}
