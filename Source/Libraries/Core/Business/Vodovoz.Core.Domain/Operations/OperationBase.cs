using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Operations
{
	/// <summary>
	/// Базовый класс операций
	/// </summary>
	public class OperationBase: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _operationTime;

		public virtual int Id { get; set; }

		/// <summary>
		/// Время операции
		/// </summary>
		public virtual DateTime OperationTime
		{
			get => _operationTime;
			set => SetField (ref _operationTime, value);
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}
}

