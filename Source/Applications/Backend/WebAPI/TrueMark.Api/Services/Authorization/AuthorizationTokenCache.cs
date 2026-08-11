using System;

namespace TrueMark.Api.Services.Authorization;

public class AuthorizationTokenCache
{
	public DateTime TokenExpirationTime { get; set; }
	public string CertificateThumbPrint { get; set; }
	public string Token { get; set; }
	public bool IsTokenFresh => DateTime.Now < TokenExpirationTime.AddMinutes(-1);
}
