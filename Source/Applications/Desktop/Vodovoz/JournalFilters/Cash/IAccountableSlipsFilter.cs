using QSOrmProject.RepresentationModel;
using System;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public interface IAccountableSlipsFilter : IRepresentationFilter
	{
		decimal? RestrictDebt { get;}

		Employee RestrictAccountable { get;}

		FinancialExpenseCategory RestrictExpenseCategory { get;}

		DateTime? RestrictStartDate { get;}

		DateTime? RestrictEndDate { get;}
	}
}
