using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using QSHistoryLog;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[IgnoreHistoryClone]
	public class OperationBase: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		DateTime operationTime;

		public virtual DateTime OperationTime {
			get { return operationTime; }
			set { SetField (ref operationTime, value, () => OperationTime); }
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}
}

