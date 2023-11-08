using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

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
	}
	public class EndBreak : OperatorCommand
	{
	}

	public class ChangePhone : OperatorCommand
	{
		public string PhoneNumber { get; set; }
	}



}
