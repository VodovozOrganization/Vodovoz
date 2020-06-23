using System;
using QS.Dialog;
using QS.Services;
using NLog;
namespace VodovozAndroidDriverService
{
	public class LoggerInteractiveService : IInteractiveService
	{
		private readonly LoggerInteractiveMessage interactiveMessage = new LoggerInteractiveMessage();
		private readonly LoggerQuestion loggerQuestion = new LoggerQuestion();

		public bool Question(string message, string title = null)
		{
			return loggerQuestion.Question(message,title);
		}

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			interactiveMessage.ShowMessage(ImportanceLevel.Info, message, title);
		}
	}

	public class LoggerInteractiveMessage : IInteractiveMessage
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			switch(level) {
			case ImportanceLevel.Info:
				logger.Info(message);
				break;
			case ImportanceLevel.Warning:
				logger.Warn(message);
				break;
			case ImportanceLevel.Error:
				logger.Error(message);
				break;
			default:
				break;
			}
		}
	}

	public class LoggerQuestion : IInteractiveQuestion
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		public bool Question(string message, string title = null)
		{
			logger.Warn($"Поступил запрос действия пользователю (\"{message}\"). Для службы нет реализации запроса действий пользователя, поэтому ответ по умлчанию - Нет");
			return false;
		}
	}
}
