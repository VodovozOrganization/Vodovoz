using System.Collections.Generic;
using Vodovoz.EntityRepositories.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public class OurCodesChecker
	{
		private ISet<string> _ownersInn;
		private ISet<string> _ourGtins;

		public OurCodesChecker(ITrueMarkRepository trueMarkRepository)
		{
			_ownersInn = trueMarkRepository.GetAllowedCodeOwnersInn();
			_ourGtins = trueMarkRepository.GetAllowedCodeOwnersGtins();
		}

		public bool IsOurOrganizationOwner(string inn) => _ownersInn.Contains(inn);

		public bool IsOurGtinOwner(string gtin) => _ourGtins.Contains(gtin);
	}
}
