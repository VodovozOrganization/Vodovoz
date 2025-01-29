using System;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Journals.Mappings
{
	/// <summary>
	/// Класс для хранения соответствия Сущность - Журнал
	/// </summary>
	public abstract class EntityToJournalMapping
	{
		protected EntityToJournalMapping(Type entityType)
		{
			EntityType = entityType;
		}
		
		public Type EntityType { get; }
		public Type JournalType { get; protected set; } 
	}
	
	/// <summary>
	/// Типизированный класс для более удобного оформления соответствия
	/// </summary>
	/// <typeparam name="TEntity">Сущность</typeparam>
	public class EntityToJournalMapping<TEntity> : EntityToJournalMapping
		where TEntity : IDomainObject
	{
		protected EntityToJournalMapping() : base(typeof(TEntity)) { }
		
		protected void Journal(Type journalType)
		{
			JournalType = journalType;
		}
	}
}
