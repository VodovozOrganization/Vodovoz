using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoUpdInventPosition : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderItemEntity _assignedOrderItem;
		private IList<EdoUpdInventPositionCode> _codes = new List<EdoUpdInventPositionCode>();

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Назначено на")]
		public virtual OrderItemEntity AssignedOrderItem
		{
			get => _assignedOrderItem;
			set => SetField(ref _assignedOrderItem, value);
		}

		[Display(Name = "Коды")]
		public virtual IList<EdoUpdInventPositionCode> Codes
		{
			get => _codes;
			set => SetField(ref _codes, value);
		}
	}
}
