using QS.DomainModel.Entity;
using QS.Project.Journal;
using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class DriverMessageJournalNode<TEntity> : DriverMessageJournalNode
		where TEntity : class, IDomainObject
	{
		public DriverMessageJournalNode() : base(typeof(TEntity)) { }
	}

	public class DriverMessageJournalNode : JournalEntityNodeBase
	{
		public DriverMessageJournalNode(Type entityType) : base(entityType)
		{
		}

		public override string Title => "No title";

		public DateTime CommentDate { get; set; }
		public string DriverName { get; set; }
		public string DriverPhone { get; set; }
		public int RouteListId { get; set; }
		public int OrderId { get; set; }
		public int BottlesReturn { get; set; }
		public int? ActualBottlesReturn { get; set; }
		public int AddressBottlesDebt { get; set; }
		public string DriverComment { get; set; }
		public string OPComment { get; set; }
		public DateTime CommentOPManagerUpdatedAt { get; set; }
		public string CommentOPManagerChangedBy { get; set; }
		public string ResponseTime
		{
			get
			{
				if(string.IsNullOrWhiteSpace(OPComment)
				|| (CommentDate > CommentOPManagerUpdatedAt))
				{
					return "Ожидает комментария ОП/ОСК";
				}

				var timeSpan = CommentOPManagerUpdatedAt.Subtract(CommentDate);

				return $"{timeSpan.TotalHours:N0}:{timeSpan.Minutes:D2}";
			}
		}
	}
}
