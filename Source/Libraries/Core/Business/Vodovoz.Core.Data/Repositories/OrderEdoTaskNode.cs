using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public class OrderEdoTaskNode
	{
		private string _edoTaskTypeName;

		public DateTime RequestTime { get; set; }
		public EdoRequestSource RequestSource { get; set; }
		public int EdoTaskId { get; set; }
		public string EdoTaskTypeName
		{
			get => _edoTaskTypeName;
			set
			{
				_edoTaskTypeName = value;
				EdoTaskType = (EdoTaskType)Enum.Parse(typeof(EdoTaskType), value);
			}
		}
		public EdoTaskType EdoTaskType { get; set; }
		public EdoTaskStatus EdoTaskStatus { get; set; }
	}
}
