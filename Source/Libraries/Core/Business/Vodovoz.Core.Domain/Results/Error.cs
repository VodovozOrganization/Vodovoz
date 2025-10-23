using System;

namespace Vodovoz.Core.Domain.Results
{
	/// <summary>
	/// Класс ошибки
	/// </summary>
	public class Error
	{
		public static readonly Error None =
			new Error(string.Empty, string.Empty);
		public static readonly Error NullValue =
			new Error(typeof(Error), nameof(NullValue), "The specified result value is null.");

		public Error(string code, string message)
		{
			Code = code;
			Message = message;
		}
		
		public Error(string code, string message, Type type)
		{
			Code = code;
			Message = message;
			Type = type;
		}

		public Error(Type type, string fieldName, string message)
			: this(GenerateCode(type, fieldName), message, type) { }

		public string Code { get; set; }
		
		public Type Type { get; }

		public string Message { get; set; }

		public static implicit operator string(Error error) => error.Code;
		
		public override string ToString() => Code;

		public override int GetHashCode() => Code.GetHashCode() + Message.GetHashCode();

		public override bool Equals(object obj) => obj is Error error && error.Code == Code && error.Message == Message;

		public static bool operator ==(Error a, Error b)
		{
			if(a is null && b is null)
			{
				return true;
			}

			if(a is null || b is null)
			{
				return false;
			}

			return a.Code == b.Code && a.Message == b.Message;
		}

		public static bool operator !=(Error a, Error b) => !(a == b);

		public static string GenerateCode(Type type, string errorField) => $"{type.Namespace}.{type.Name}.{errorField}";
	}
}
