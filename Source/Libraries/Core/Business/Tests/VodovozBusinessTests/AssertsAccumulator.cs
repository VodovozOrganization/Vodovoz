using System;
using System.Text;
using NUnit.Framework;

namespace VodovozBusinessTests
{
	public class AssertsAccumulator
	{
		public static AssertsAccumulator Create => new AssertsAccumulator();

		private StringBuilder errors { get; set; }
		private bool passed { get; set; }

		private String errorsMessage => errors.ToString();

		public AssertsAccumulator()
		{
			errors = new StringBuilder();
			passed = true;
		}

		private void RegisterError(string exceptionMessage)
		{
			passed = false;
			errors.AppendLine(exceptionMessage);
		}

		public AssertsAccumulator Accumulate(Action assert)
		{
			try {
				assert.Invoke();
			} catch(Exception exception) {
				RegisterError(exception.Message);
			}
			return this;
		}

		public void Release()
		{
			if(!passed) {
				throw new AssertionException(errorsMessage);
			}
		}
	}
}
