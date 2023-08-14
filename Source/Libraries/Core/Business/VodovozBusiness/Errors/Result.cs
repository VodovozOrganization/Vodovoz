using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Errors
{
	public class Result
	{
		protected internal Result(bool isSuccess, Error error)
		{
			var errors = new Error[] { error };

			ThrowIfNotValidInput(isSuccess, errors);

			IsSuccess = isSuccess;
			Errors = errors;
		}

		protected internal Result(bool isSuccess, IEnumerable<Error> errors)
		{
			ThrowIfNotValidInput(isSuccess, errors);

			IsSuccess = isSuccess;
			Errors = errors.Distinct();
		}

		public bool IsSuccess { get; }

		public bool IsFailure => !IsSuccess;

		public IEnumerable<Error> Errors { get; }

		private void ThrowIfNotValidInput(bool isSuccess, IEnumerable<Error> errors)
		{
			if(isSuccess
				&& !errors.All(x => x == Error.None)
				&& !errors.Any(x => x == Error.None))
			{
				throw new InvalidOperationException($"Success result must contain {nameof(Error.None)}");
			}

			if(!isSuccess && errors.Any(x => x == Error.None))
			{
				throw new InvalidOperationException($"Failure result shouldn't contain {nameof(Error.None)}");
			}
		}

		public void Match(Action successAction, Action<IEnumerable<Error>> errorsHandlingAction)
		{
			if(IsSuccess)
			{
				successAction();
			}
			else
			{
				errorsHandlingAction(Errors);
			}
		}

		public static Result Success() => new Result(true, Error.None);

		public static Result Failure(Error error) => new Result(false, error);

		public static Result Failure(IEnumerable<Error> errors) => new Result(false, errors);

		public static Result<TValue> Success<TValue>(TValue value) =>
			new Result<TValue>(value, true, Error.None);

		public static Result<TValue> Failure<TValue>(Error error) =>
			new Result<TValue>(default, false, error);

		public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) =>
			new Result<TValue>(default, false, errors);
	}
}
