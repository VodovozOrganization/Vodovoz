using System;
using System.Net;

namespace VodovozHealthCheck.Utils
{
	public class UrlExistsChecker
	{
		public static bool UrlExists(string url)
		{
			try
			{
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
				webRequest.Timeout = 30000;
				webRequest.Method = "GET";

				HttpWebResponse response = null;

				try
				{
					response = (HttpWebResponse)webRequest.GetResponse();

					int statusCode = (int)response.StatusCode;
					if(statusCode >= 100 && statusCode < 400) //Good requests
					{
						return true;
					}
					else if(statusCode >= 500 && statusCode <= 510) //Server Errors
					{
						return false;
					}
				}
				catch(WebException webException)
				{
					return false;
				}
				finally
				{
					if(response != null)
					{
						response.Close();
					}
				}
			}
			catch(Exception ex)
			{
				return false;
			}

			return false;
		}
	}
}
