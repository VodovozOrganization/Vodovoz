using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces;

namespace Vodovoz.ViewModels.ViewModels.Common
{
	/// <inheritdoc/>
	public class EntityViewModelContext : IEntityViewModelContext
	{
		/// <inheritdoc/>
		public Type EntityType { get; private set; }
		/// <inheritdoc/>
		public int? EntityId { get; private set; }
		/// <inheritdoc/>
		public IUnitOfWorkFactory UowFactory { get; private set; }

		public static EntityViewModelContext Create(Type entityType, int? entityId, IUnitOfWorkFactory uowFactory) =>
			new EntityViewModelContext
			{
				EntityType = entityType,
				EntityId = entityId,
				UowFactory = uowFactory
			};
	}
}
