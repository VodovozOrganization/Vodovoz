using System;
using System.Collections.Generic;
using System.Text;

namespace Vodovoz.Core.Domain.Edo
{
	public class SaveCodesEdoTask : EdoTask
	{
		private int _orderId;

		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}
	}
}
