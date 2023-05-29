using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class PremiumJournalNode<TEntity> : PremiumJournalNode
	where TEntity : class, IDomainObject
	{
		public PremiumJournalNode() : base(typeof(TEntity)) { }
	}

	public class PremiumJournalNode : JournalEntityNodeBase
	{
		public PremiumJournalNode(Type entityType) : base(entityType)
		{
			if(entityType == typeof(Premium))
				ViewType = "Премия";

			if(entityType == typeof(PremiumRaskatGAZelle))
				ViewType = "Автопремия для раскатных газелей";
		}

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public DateTime Date { get; set; }
		public string EmployeesName { get; set; }
		public string PremiumReason { get; set; }
		public decimal PremiumSum { get; set; }
		public string ViewType { get; set; }
	}
}

