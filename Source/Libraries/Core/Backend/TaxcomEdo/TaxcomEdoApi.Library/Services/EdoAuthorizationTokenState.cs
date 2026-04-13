using System;

namespace TaxcomEdoApi.Library.Services;

public class EdoAuthorizationTokenState
{
	private EdoAuthorizationTokenState(string token, DateTime expires)
	{
		Token = token;
		Expires = expires;
	}

	public string Token { get; private set; }
	public DateTime Expires { get; private set; }

	public void UpdateExpires(DateTime expires)
	{
		Expires = expires;
	}
		
	public static EdoAuthorizationTokenState Create(string token, DateTime expires) =>
		new EdoAuthorizationTokenState(token, expires);
}
