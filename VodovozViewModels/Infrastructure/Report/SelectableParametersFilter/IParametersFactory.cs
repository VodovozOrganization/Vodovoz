using System;
using System.Collections.Generic;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory
	{
		IList<SelectableParameter> GetParameters();
	}
}
