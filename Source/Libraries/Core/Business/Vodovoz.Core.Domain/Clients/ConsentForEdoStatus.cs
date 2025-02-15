﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum ConsentForEdoStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Отправлено")]
		Sent,
		[Display(Name = "Согласен")]
		Agree,
		[Display(Name = "Отклонено")]
		Rejected
	}
}
