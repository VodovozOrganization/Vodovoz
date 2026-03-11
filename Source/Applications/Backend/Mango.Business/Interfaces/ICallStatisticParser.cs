using System.Collections.Generic;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticParser
	{
		List<CallEntity> Parse(string json);
	}
}
