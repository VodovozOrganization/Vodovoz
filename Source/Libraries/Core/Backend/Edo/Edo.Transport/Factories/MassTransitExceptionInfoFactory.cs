using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Problems.Exception;
using MassTransit;

namespace Edo.Transport.Factories
{
	/// <inheritdoc/>
	public class MassTransitExceptionInfoFactory : IMassTransitExceptionInfoFactory
	{
		/// <inheritdoc/>
		public IEnumerable<MassTransitExceptionInfo> Create(IEnumerable<ExceptionInfo> exceptionInfos)
		{
			return exceptionInfos
				.Select(Create)
				.ToList();
		}

		/// <inheritdoc/>
		public MassTransitExceptionInfo Create(Exception exception)
		{
			if(exception is null)
			{
				return null;
			}
			
			var exceptionInfo = new MassTransitExceptionInfo
			{
				Message = exception.Message,
				ExceptionType = exception.GetType().Name,
				Source = exception.Source,
				StackTrace = exception.StackTrace,
				Data = new Dictionary<string, object>(),
				InnerException = Create(exception.InnerException)
			};
				
			return exceptionInfo;
		}

		private MassTransitExceptionInfo Create(ExceptionInfo exceptionInfo)
		{
			if(exceptionInfo is null)
			{
				return null;
			}
			
			var info = new MassTransitExceptionInfo
			{
				Message = exceptionInfo.Message,
				ExceptionType = exceptionInfo.ExceptionType,
				Source = exceptionInfo.Source,
				StackTrace = exceptionInfo.StackTrace,
				Data = exceptionInfo.Data,
				InnerException = Create(exceptionInfo.InnerException)
			};

			return info;
		}
	}
}
