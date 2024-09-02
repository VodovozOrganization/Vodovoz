using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Vodovoz.Presentation.ViewModels.Administration
{
	public abstract partial class AdministrativeOperationViewModelBase : DialogViewModelBase
	{
		protected readonly ILogger<AdministrativeOperationViewModelBase> _logger;
		private DateTime _startDateTime;
		private DateTime _endDateTime;

		protected AdministrativeOperationViewModelBase(
			ILogger<AdministrativeOperationViewModelBase> logger,
			INavigationManager navigation)
			: base(navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			LogStrings = new ObservableList<LogNode>();
		}

		public ObservableList<LogNode> LogStrings { get; }

		[PropertyChangedAlso(
			nameof(StartDateTimeMessage),
			nameof(EndDateTimeAndDiffMessage))]
		public DateTime StartDateTime
		{
			get => _startDateTime;
			set => SetField(ref _startDateTime, value);
		}

		[PropertyChangedAlso(nameof(EndDateTimeAndDiffMessage))]
		public DateTime EndDateTime
		{
			get => _endDateTime;
			set => SetField(ref _endDateTime, value);
		}

		public string StartDateTimeMessage =>
			StartDateTime == DateTime.MinValue
			? ""
			: $"Операция запущена в {StartDateTime:G}";

		public string EndDateTimeAndDiffMessage =>
			StartDateTime == DateTime.MinValue
			|| EndDateTime == DateTime.MinValue
			? ""
			: $"Операция завершена в {EndDateTime:G} (Выполнение заняло {(EndDateTime-StartDateTime):G})";

		public DelegateCommand RunCommand { get; protected set; }

		public void Log(LogLevel logLevel, string messageFormat, params object[] parameters)
		{
			_logger.Log(logLevel, messageFormat, parameters);
			AddLog(logLevel, messageFormat, parameters);
		}

		#region LogLevelsFunctions

		public void LogCritical(Exception exception, string messageFormat, params object[] parameters)
		{
			_logger.LogCritical(exception, messageFormat, parameters);
			AddLog(LogLevel.Critical, messageFormat, parameters);
		}

		public void LogCritical(string messageFormat, params object[] parameters) => Log(LogLevel.Critical, messageFormat, parameters);

		public void LogError(string messageFormat, params object[] parameters) => Log(LogLevel.Error, messageFormat, parameters);

		public void LogError(Exception exception, string messageFormat, params object[] parameters)
		{
			_logger.LogError(exception, messageFormat, parameters);
			AddLog(LogLevel.Error, messageFormat, parameters);
		}

		public void LogWarning(string messageFormat, params object[] parameters) => Log(LogLevel.Warning, messageFormat, parameters);

		public void LogDebug(string messageFormat, params object[] parameters) => Log(LogLevel.Debug, messageFormat, parameters);

		public void LogInformation(string messageFormat, params object[] parameters) => Log(LogLevel.Information, messageFormat, parameters);

		public void LogTrace(string messageFormat, params object[] parameters) => Log(LogLevel.Trace, messageFormat, parameters);

		#endregion LogLevelsFunctions

		public void ClearLog()
		{
			LogStrings.Clear();
		}

		private void AddLog(LogLevel logLevel, string messageFormat, params object[] parameters)
		{
			LogStrings.Add(
				new LogNode
				{
					DateTime = DateTime.Now,
					LogLevel = logLevel,
					Message = parameters.Any() ? ReformatMessage(messageFormat, parameters) : messageFormat
				});
		}

		private string ReformatMessage(string text, params object[] args)
		{
			var argReplacement = new List<string>();

			var pattern = "({.*?})";

			var matches = Regex.Matches(text, pattern);

			foreach(Match match in matches)
			{
				if(!argReplacement.Contains(match.Value))
				{
					argReplacement.Add(match.Value);
				}
			}

			var matchEveluator = new MatchEvaluator((match) => "{" + argReplacement.IndexOf(match.Value) + "}");

			var result = Regex.Replace(text, pattern, matchEveluator);

			return string.Format(result, args);
		}
	}
}
