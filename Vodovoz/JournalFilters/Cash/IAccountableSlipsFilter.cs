using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;

namespace Vodovoz
{
	public interface IAccountableSlipsFilter : IRepresentationFilter
	{
		decimal? RestrictDebt { get;}

		Employee Accountable { get;}
	}
}

