using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	/// <summary>
	/// Статус проверки кода
	/// </summary>
	[Serializable]
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum VerificationStatusType
	{
		/// <summary>
		/// The code entered by the user is correct
		/// </summary>
		[EnumMember(Value = "code_valid")]
		CodeValid,
		/// <summary>
		/// The code entered by the user is incorrect
		/// </summary>
		[EnumMember(Value = "code_invalid")]
		CodeInvalid,
		/// <summary>
		/// The maximum number of attempts to enter the code has been exceeded
		/// </summary>
		[EnumMember(Value = "code_max_attempts_exceeded")]
		CodeMaxAttemptsExceeded,
		/// <summary>
		/// The code has expired and can no longer be used for verification
		/// </summary>
		[EnumMember(Value = "expired")]
		Expired
	}
}
