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

		FinancialExpenseCategory FinancialExpenseCategory { get; }

		DateTime? RestrictStartDate { get;}

		DateTime? RestrictEndDate { get;}
	}
}
