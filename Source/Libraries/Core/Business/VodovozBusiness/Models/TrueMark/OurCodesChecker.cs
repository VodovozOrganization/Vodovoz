using System;
using System.Collections.Generic;
using System.Threading;
using Vodovoz.EntityRepositories.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public class OurCodesChecker
	{
		private readonly Lazy<ISet<string>> _ownersInn;
		private readonly Lazy<ISet<string>> _ourGtins;

		public OurCodesChecker(ITrueMarkRepository trueMarkRepository)
		{
			if(trueMarkRepository is null)
			{
				throw new ArgumentNullException(nameof(trueMarkRepository));
			}

			_ownersInn = new Lazy<ISet<string>>(trueMarkRepository.GetAllowedCodeOwnersInn, LazyThreadSafetyMode.PublicationOnly);
			_ourGtins = new Lazy<ISet<string>>(trueMarkRepository.GetAllowedCodeOwnersGtins, LazyThreadSafetyMode.PublicationOnly);
		}

		public bool IsOurOrganizationOwner(string inn) => _ownersInn.Value.Contains(inn);

		public bool IsOurGtinOwner(string gtin) => _ourGtins.Value.Contains(gtin);
	}
}
