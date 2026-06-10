using System.Collections.Generic;
using Mango.Contracts.V1.Response;

namespace Mango.Business.Models
{
	public class MangoReferenceData
	{
		public Dictionary<long, MangoGroup> GroupsById { get; init; } = new();
		public Dictionary<long, MangoOperatorReference> OperatorsById { get; init; } = new();
		public Dictionary<string, MangoOperatorReference> OperatorsByExtension { get; init; } = new();
	}
}
