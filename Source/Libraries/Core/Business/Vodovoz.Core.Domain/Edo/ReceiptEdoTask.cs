using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vodovoz.Core.Domain.Edo
{
	public class ReceiptEdoTask : OrderEdoTask
	{
		private int _cashReceiptId;

		// Допускается связь с чеком, для интеграции со старой системой чеков
		[Display(Name = "Код чека")]
		public virtual int CashReceiptId
		{
			get => _cashReceiptId;
			set => SetField(ref _cashReceiptId, value);
		}
	}
}
