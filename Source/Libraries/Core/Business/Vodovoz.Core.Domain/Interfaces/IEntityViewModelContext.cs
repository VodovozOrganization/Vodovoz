using System;
using QS.DomainModel.UoW;

namespace Vodovoz.Core.Domain.Interfaces
{
	/// <summary>
	/// Контекст для передачи во вью модель с конкретной сущностью
	/// </summary>
	public interface IEntityViewModelContext
	{
		/// <summary>
		/// Тип сущности
		/// </summary>
		Type EntityType { get; }
		/// <summary>
		/// Идентификатор сущности, если null - создание новой
		/// </summary>
		int? EntityId { get; }
		/// <summary>
		/// Фабрика unit of work
		/// </summary>
		IUnitOfWorkFactory UowFactory { get; }
	}
}
