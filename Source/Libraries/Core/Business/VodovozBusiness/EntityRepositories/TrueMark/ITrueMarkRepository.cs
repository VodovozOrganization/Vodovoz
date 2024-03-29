﻿using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds);

		ISet<string> GetAllowedCodeOwnersInn();
	}
}
