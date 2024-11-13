using System;

namespace TrueMark.Api.Services.Authorization;

public class AuthorizationTokenCache
{
	public DateTime TokenCreateTime { get; set; }
	public string CertificateThumbPrint { get; set; }
	public string Token { get; set; }

	// Срок действия полученного токена не более 10 часов с момента получения.
	public bool IsTokenFresh => (DateTime.Now - TokenCreateTime).TotalHours < 10;
}
