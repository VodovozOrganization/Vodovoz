using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public interface IAccountableSlipsFilter : IRepresentationFilter
	{
		decimal? RestrictDebt { get;}

		Employee RestrictAccountable { get;}

		ExpenseCategory RestrictExpenseCategory { get;}

		DateTime? RestrictStartDate { get;}

		DateTime? RestrictEndDate { get;}
	}
}

