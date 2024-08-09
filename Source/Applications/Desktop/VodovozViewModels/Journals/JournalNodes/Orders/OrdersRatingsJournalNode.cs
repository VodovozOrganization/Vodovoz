using System;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OrdersRatingsJournalNode : JournalEntityNodeBase
	{
		public static string IdColumn = "Номер";
		public static string OnlineOrderIdColumn = "Онлайн заказ";
		public static string OrderIdColumn = "Заказ";
		public static string CreatedColumn = "Дата создания";
		public static string StatusColumn = "Статус";
		public static string RatingColumn = "Оценка";
		public static string ReasonsColumn = "Причины оценки";
		public static string CommentColumn = "Комментарий";
		public static string SourceColumn = "Источник";
		
		public override string Title => string.Empty;
		
		public int? OnlineOrderId { get; set; }
		public int? OrderId { get; set; }
		public DateTime OrderRatingCreated { get; set; }
		public OrderRatingStatus OrderRatingStatus { get; set; }
		public int Rating { get; set; }
		public string OrderRatingReasons { get; set; }
		public string ProcessedByEmployee { get; set; }
		public string OrderRatingComment { get; set; }
		public Core.Domain.Clients.Source OrderRatingSource { get; set; }
	}
}
