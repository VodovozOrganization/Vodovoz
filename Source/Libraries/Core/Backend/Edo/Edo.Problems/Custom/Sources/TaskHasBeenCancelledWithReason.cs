using QS.Services;
using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class TaskHasBeenCancelledWithReason : EdoTaskProblemCustomSource
	{
		private readonly IUserService _userService;
		private readonly string _userFullName;

		public TaskHasBeenCancelledWithReason(IUserService userService)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_userFullName = _userService.GetCurrentUser().Name;
		}

		public override string Name => $"Custom.{nameof(TaskHasBeenCancelledWithReason)}";

		public override string Message => $"Новая ручная переотправка пользователем {_userFullName}";

		public override string Description => "Документ был переотправлен, задача была отменена, коды перешли в новую задачу";

		public override string Recommendation => "Обратитесь за технической поддержкой";

		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
