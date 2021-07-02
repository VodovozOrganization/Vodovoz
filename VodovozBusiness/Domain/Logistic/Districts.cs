using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic
{
	public class Districts : PropertyChangedBase, IDomainObject
	{
		public Districts()
		{
		}

		public int Id { get; set; }
	}
}
