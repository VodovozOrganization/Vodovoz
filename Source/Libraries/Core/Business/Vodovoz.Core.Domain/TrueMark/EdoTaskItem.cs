using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark
{
	public class EdoTaskItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _orderEdoTaskId;
		private int _orderItemId;
		private int _codeId;
		private int _transferEdoTaskId;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Код ЭДО задачи заказа")]
		public virtual int OrderEdoTaskId
		{
			get => _orderEdoTaskId;
			set => SetField(ref _orderEdoTaskId, value);
		}

		[Display(Name = "Код товара в заказе")]
		public virtual int OrderItemId
		{
			get => _orderItemId;
			set => SetField(ref _orderItemId, value);
		}

		[Display(Name = "Код ЧЗ")]
		public virtual int CodeId
		{
			get => _codeId;
			set => SetField(ref _codeId, value);
		}

		[Display(Name = "Код ЭДО задачи перемещения")]
		public virtual int TransferEdoTaskId
		{
			get => _transferEdoTaskId;
			set => SetField(ref _transferEdoTaskId, value);
		}
	}
}
