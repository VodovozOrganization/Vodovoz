using System;
using System.Collections.Generic;
using System.Linq;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Contracts.V1.Response;
using Microsoft.Extensions.Options;

namespace Mango.Business.Models
{
	public class MangoReferenceDataBuilder : IMangoReferenceDataBuilder
	{
		private readonly IOptions<MangoGroupOptions> _options;

		public MangoReferenceDataBuilder(IOptions<MangoGroupOptions>  options)
		{
			_options = options;
		}
		
		public MangoReferenceData Build(GroupsResponse response)
		{
			var groupsById = new Dictionary<long, MangoGroup>();
			var operatorsById = new Dictionary<long, MangoOperatorReference>();
			var operatorsByExtension = new Dictionary<string, MangoOperatorReference>(StringComparer.OrdinalIgnoreCase);

			if (response.Groups != null)
			{
				foreach (var group in response.Groups.Where(g => _options.Value.TargetGroupIds.Contains(g.Id)))
				{
					groupsById[group.Id] = group;

					if (group.Operators == null)
						continue;

					foreach (var op in group.Operators)
					{
						var reference = new MangoOperatorReference
						{
							OperatorId = op.Id,
							OperatorName = op.Name,
							Extension = op.Extension,
							GroupId = group.Id,
							GroupName = group.Name
						};

						operatorsById[op.Id] = reference;

						if (!string.IsNullOrWhiteSpace(op.Extension))
						{
							operatorsByExtension[op.Extension] = reference;
						}
					}
				}
			}

			return new MangoReferenceData
			{
				GroupsById = groupsById,
				OperatorsById = operatorsById,
				OperatorsByExtension = operatorsByExtension
			};
		}
	}
}
