﻿namespace Vodovoz.Core.Domain.Edo
{
	public class OrderEdoDocument : OutgoingEdoDocument
	{
		private int _documentTaskId;
		public virtual int DocumentTaskId
		{
			get => _documentTaskId;
			set => SetField(ref _documentTaskId, value);
		}
	}
}
