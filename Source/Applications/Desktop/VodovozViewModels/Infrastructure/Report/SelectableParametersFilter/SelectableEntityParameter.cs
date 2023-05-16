using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableEntityParameter<TEntity> : SelectableParameter
		where TEntity : class, IDomainObject
	{
		private readonly Func<TEntity, string> titleFunc;

		private readonly bool _useFullEntity;

		public override string Title => EntityTitle;

		public TEntity Entity { get; }

		public int EntityId { get; set; }

		public string EntityTitle { get; set; }

		public override Func<object> ValueFunc => () =>
		{
			if(_useFullEntity)
			{
				return Entity;
			}

			return EntityId;
		};

		public SelectableEntityParameter()
		{
		}

		public SelectableEntityParameter(int entityId, string title)
		{
			EntityId = entityId;
			EntityTitle = title;
		}

		public SelectableEntityParameter(TEntity entity, string title)
		{
			_useFullEntity = true;
			EntityTitle = title;
			Entity = entity;
		}
	}
}
