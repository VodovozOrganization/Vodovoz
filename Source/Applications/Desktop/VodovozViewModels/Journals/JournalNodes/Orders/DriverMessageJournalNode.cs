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

		public DateTime CommentDate { get; set; }
		public string DriverName { get; set; }
		public string DriverPhone { get; set; }
		public int RouteListId { get; set; }
		public int OrderId { get; set; }
		public int BottlesReturn { get; set; }
		public int? ActualBottlesReturn { get; set; }
		public int AddressBottlesDebt { get; set; }
		public string DriverComment { get; set; }
		public string CommentManager { get; set; }
		public DateTime LastCommentManagerEditedTime { get; set; }
		public Employee LastCommentManagerEditor { get; set; }
		public string ResponseTime
		{
			get
			{
				if (LastCommentManagerEditedTime.TimeOfDay == TimeSpan.Zero)
				{
					return "Ожидает комментария ОП/ОСК";
				}
				return new DateTime(LastCommentManagerEditedTime.Subtract(CommentDate).Ticks).ToString("HH:mm:ss");
			}
		}
	}
}
