using System;
using System.Text;

namespace TaxcomEdo.Client
{
	public partial class TaxcomApiClient
	{
		public class HttpQueryBuilder : IHttpQueryBuilder
		{
			private const string _parameterIdentifier = "?";
			private const string _parameterEquals = "=";
			private const string _parameterSeparator = "&";
			private readonly StringBuilder _queryBuilder = new StringBuilder();
			private bool _hasFirstParameter;

			public IHttpQueryBuilder AddParameter<T>(T parameter, string parameterName)
			{
				if(parameter == null)
				{
					return this;
				}
				
				if(_queryBuilder.Length > 0)
				{
					_queryBuilder.Append(_parameterSeparator);
				}

				if(_queryBuilder.Length == 0)
				{
					_queryBuilder.Append(_parameterIdentifier);
				}
				
				_queryBuilder
					.Append(parameterName)
					.Append(_parameterEquals);

				if(parameter is DateTime dateTime)
				{
					_queryBuilder.AppendFormat("{0:o}", dateTime);
				}
				else
				{
					_queryBuilder.Append(parameter);
				}

				return this;
			}

			public override string ToString()
			{
				return _queryBuilder.ToString();
			}

			public static IHttpQueryBuilder Create() => new HttpQueryBuilder();
		}
	}
}
