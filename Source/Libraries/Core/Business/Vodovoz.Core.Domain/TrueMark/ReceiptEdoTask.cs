using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vodovoz.Core.Domain.TrueMark
{
	public class ReceiptEdoTask : EdoTask
	{
		private int _orderId;

		[Display(Name = "Код заказа")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}
	}
}
