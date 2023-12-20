using Pacs.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Commands
{
	public abstract class OperatorCommand
	{
		public int OperatorId { get; set; }
	}

	public class Connect : OperatorCommand
	{
	}
	public class Disconnect : OperatorCommand
	{
	}

	public class StartWorkShift : OperatorCommand
	{
		public string PhoneNumber { get; set; }
	}

	public class EndWorkShift : OperatorCommand
	{
	}

	public class StartBreak : OperatorCommand
	{
		public OperatorBreakType BreakType { get; set; }
	}

	public class EndBreak : OperatorCommand
	{
	}

	public class AdminStartBreak : OperatorCommand
	{
		public int AdminId{ get; set; }
		public OperatorBreakType BreakType { get; set; }
		public string Reason { get; set; }
	}

	public class AdminEndBreak : OperatorCommand
	{
		public int AdminId{ get; set; }
		public string Reason { get; set; }
	}

	public class ChangePhone : OperatorCommand
	{
		public string PhoneNumber { get; set; }
	}



}
