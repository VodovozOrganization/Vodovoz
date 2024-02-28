using System;
using System.Collections.Generic;
using System.Text;

namespace Pacs.Core
{
	public interface IPacsOperatorProvider
	{
		int? OperatorId { get; }
	}
}
