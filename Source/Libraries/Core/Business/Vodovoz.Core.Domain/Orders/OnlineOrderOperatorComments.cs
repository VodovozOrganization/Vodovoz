using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Orders
{
	public class OnlineOrderOperatorComments : PropertyChangedBase, IDomainObject
	{
		private DateTime _createTime;
		private string _comment;
		private EmployeeEntity _commentAuthor;
		
		/// <summary>
		/// ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Дата комментария
		/// </summary>
		[Display(Name="Дата комментария")]
		public DateTime CreateTime
		{
			get => _createTime;
			set => SetField(ref _createTime, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name="Комментарий")]
		public string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Автор комментария
		/// </summary>
		[Display(Name="Автор комментария")]
		public EmployeeEntity CommentAuthor
		{
			get => _commentAuthor;
			set => SetField(ref _commentAuthor, value);
		}
	}
}
