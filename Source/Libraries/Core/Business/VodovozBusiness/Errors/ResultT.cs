using System;
using System.Collections.Generic;

namespace Vodovoz.Errors
{
	public class Result<TValue> : Result
	{
		private readonly TValue _value;

		protected internal Result(TValue value, bool isSuccess, Error error)
			: base(isSuccess, error) =>
			_value = value;

		protected internal Result(TValue value, bool isSuccess, IEnumerable<Error> errors)
			: base(isSuccess, errors) =>
			_value = value;

		public TValue Value => IsSuccess
			? _value
			: throw new InvalidOperationException("The value of a failure result can't be accessed");

		public static implicit operator Result<TValue>(TValue value) => Create(value);

		private static Result<TValue> Create(TValue value) =>
			new Result<TValue>(value, true, Error.None);

		public void Match(Action<TValue> successAction, Action<IEnumerable<Error>> errorsHandlingAction)
		{
			if(IsSuccess)
			{
				successAction(_value);
			}
			else
			{
				errorsHandlingAction(Errors);
			}
		}
	}
}
