using System.Collections.Generic;
using Mango.Business.Models;
using Mango.Contracts.V1.Response;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticParser
	{
		List<CallEntity> Parse(CallsResponse response, MangoReferenceData referenceData);
	}
}
