using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoCoderApi.Client.Extensions
{
	public static class UriExtensions
	{
		private static char _parametersStartSymbol = '?';
		private static char _parametersAdditionSymbol = '&';
		private static char _parametersAssignSymbol = '=';

		public static Uri WithParameter(this Uri uri, string parameterName, object parameterValue)
			=> new Uri(uri.Query.WithParameter(parameterName, parameterValue));

		public static Uri WithParameters(this Uri uri, IDictionary<string, object> parameters)
			=> new Uri(uri.Query.WithParameters(parameters));

		public static string WithParameter(this string uri, string parameterName, object parameterValue)
		{
			if(string.IsNullOrWhiteSpace(uri))
			{
				uri = "";
			}

			if(uri.Contains(_parametersStartSymbol))
			{
				uri += $"{_parametersAdditionSymbol}{parameterName}{_parametersAssignSymbol}{parameterValue}";
			}
			else
			{
				uri += $"{_parametersStartSymbol}{parameterName}{_parametersAssignSymbol}{parameterValue}";
			}

			return uri;
		}

		public static string WithParameters(this string uri, IDictionary<string, object> parameters)
		{
			if(string.IsNullOrWhiteSpace(uri))
			{
				uri = "";
			}

			foreach(var parameterPair in parameters)
			{
				uri = uri.WithParameter(parameterPair.Key, parameterPair.Value);
			}

			return uri;
		}
	}
}
