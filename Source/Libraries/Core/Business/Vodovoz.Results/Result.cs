using System;

namespace Vodovoz.Results
{
	/// <summary>
	/// Результат выполнения операции
	/// </summary>
	/// <typeparam name="T">Результат выполнения операции</typeparam>
	/// <typeparam name="TError">Ошибка при выполнении операции</typeparam>
	public class Result<T, TError>
	{
		private T _value;
		private TError _error;

		private Result(bool isSuccess, T value, TError error) =>
			(IsSuccess, Value, Error) = (isSuccess, value, error);

		/// <summary>
		/// Успешен ли результат выполнения операции
		/// </summary>
		public bool IsSuccess { get; }

		/// <summary>
		/// Результат выполнения операции
		/// </summary>
		public T Value
		{
			get => IsSuccess ? _value : throw new InvalidOperationException("Результат не успешен, данные результата недоступны");
			private set => _value = value;
		}

		/// <summary>
		/// Ошибка при выполнении операции
		/// </summary>
		public TError Error
		{
			get => IsFailure ? _error : throw new InvalidOperationException("Результат успешен, данные ошибки недоступны");
			private set => _error = value;
		}

		/// <summary>
		/// Провален ли результат выполнения операции
		/// </summary>
		public bool IsFailure => !IsSuccess;

		/// <summary>
		/// Создание успешного результата выполнения операции
		/// </summary>
		/// <param name="value">Значение выполнения операции</param>
		/// <returns></returns>
		public static Result<T, TError> Success(T value) =>
			new Result<T, TError>(true, value, default);

		/// <summary>
		/// Создание проваленного результата выполнения операции
		/// </summary>
		/// <param name="error">Ошибка выполнения операции</param>
		/// <returns></returns>
		public static Result<T, TError> Failure(TError error) =>
			new Result<T, TError>(false, default,  error);

		public static implicit operator Result<T, TError>(T value) => Success(value);

		public static implicit operator Result<T, TError>(TError error) => Failure(error);
	}
}
