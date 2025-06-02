using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Fuel
{
	public class FuelApiRequest : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual DateTime RequestDateTime { get; set; }
		public virtual User Author { get; set; }
		public virtual FuelApiRequestFunction RequestFunction { get; set; }
		public virtual FuelApiResponseResult ResponseResult { get; set; }
		public virtual string ErrorResponseMessage { get; set; }
	}
}
