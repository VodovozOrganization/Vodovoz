﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintDiscussionStatuses
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "На проверке")]
		Checking,
		[Display(Name = "Закрыт")]
		Closed
	}

	public class ComplaintDiscussionStatusesStringType : NHibernate.Type.EnumStringType
	{
		public ComplaintDiscussionStatusesStringType() : base(typeof(ComplaintDiscussionStatuses))
		{
		}
	}
}
