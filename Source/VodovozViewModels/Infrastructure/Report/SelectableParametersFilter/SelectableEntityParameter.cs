using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableEntityParameter<TEntity> : SelectableParameter
		where TEntity : class, IDomainObject
	{
		private readonly Func<TEntity, string> titleFunc;

		public override string Title => EntityTitle;

		public TEntity Entity { get; }

		public int EntityId { get; set; }

		public string EntityTitle { get; set; }

		public override Func<object> ValueFunc => () => EntityId;

		public SelectableEntityParameter()
		{
		}

		public SelectableEntityParameter(int entityId, string title)
		{
			EntityId = entityId;
			EntityTitle = title;
		}
	}
}
