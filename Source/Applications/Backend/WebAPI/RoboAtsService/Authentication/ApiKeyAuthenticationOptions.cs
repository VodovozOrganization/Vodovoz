﻿using Microsoft.AspNetCore.Authentication;

namespace RoboatsService.Authentication
{
	public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
	{
		public const string DefaultScheme = "ClientKey";
		public const string HeaderName = "ApiKey";
	}
}
