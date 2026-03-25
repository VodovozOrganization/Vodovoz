using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Domain.Orders
{
	[HistoryTrace]
	public class OnlineOrderOperatorComments : PropertyChangedBase, IDomainObject
	{
		private DateTime _createTime;
		private string _comment;
		private EmployeeEntity _commentAuthor;
		private OnlineOrder _onlineOrder;

		/// <summary>
		/// ID
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата комментария
		/// </summary>
		[Display(Name="Дата комментария")]
		public virtual DateTime CreateTime
		{
			get => _createTime;
			set => SetField(ref _createTime, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name="Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Автор комментария
		/// </summary>
		[Display(Name="Автор комментария")]
		public virtual EmployeeEntity CommentAuthor
		{
			get => _commentAuthor;
			set => SetField(ref _commentAuthor, value);
		}

		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}
	}
}
