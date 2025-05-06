using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Results
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

		public static implicit operator Result<TValue>(TValue value) => Success(value);

		public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

		private static Result<TValue> Success(TValue value) =>
			new Result<TValue>(value, true, Error.None);
	}
}
