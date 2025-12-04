using System;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.Mappings
{
	/// <summary>
	/// Класс для хранения соответствия Сущность - Журнал
	/// </summary>
	public abstract class EntityToJournalMapping
	{
		protected EntityToJournalMapping(Type entityType, Type journalType)
		{
			EntityType = entityType;
			JournalType = journalType;
		}
		
		public Type EntityType { get; }
		public Type JournalType { get; }

		public abstract object GetJournalConfigureAction();
	}
	
	/// <summary>
	/// Типизированный класс для более удобного оформления соответствия
	/// </summary>
	/// <typeparam name="TEntity">Сущность</typeparam>
	/// <typeparam name="TJournal">Открываемый журнал сущности</typeparam>
	public class EntityToJournalMapping<TEntity, TJournal> : EntityToJournalMapping
		where TEntity : IDomainObject
		where TJournal : JournalViewModelBase
	{
		protected EntityToJournalMapping() : base(typeof(TEntity), typeof(TJournal)) { }
		
		public Action<TJournal> JournalConfigureAction { get; private set; }
		
		public void ConfigureJournal(Action<TJournal> journalConfigureAction)
		{
			JournalConfigureAction = journalConfigureAction;
		}
		
		public override object GetJournalConfigureAction()
		{
			return JournalConfigureAction;
		}
	}
}
