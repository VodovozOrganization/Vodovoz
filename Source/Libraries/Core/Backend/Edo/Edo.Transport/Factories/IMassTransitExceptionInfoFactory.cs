using System;
using System.Collections.Generic;
using Edo.Problems.Exception;
using MassTransit;

namespace Edo.Transport.Factories
{
	/// <summary>
	/// Фабрика информации об исключении
	/// </summary>
	public interface IMassTransitExceptionInfoFactory
	{
		/// <summary>
		/// Получение списка данных по исключениям из <see cref="ExceptionInfo"/>
		/// </summary>
		/// <param name="exceptionInfos">Данные по ошибкам</param>
		/// <returns></returns>
		IEnumerable<MassTransitExceptionInfo> Create(IEnumerable<ExceptionInfo> exceptionInfos);
		/// <summary>
		/// Получение информации об исключении из самой ошибки
		/// </summary>
		/// <param name="exception">Ошибка</param>
		/// <returns></returns>
		MassTransitExceptionInfo Create(Exception exception);
	}
}
