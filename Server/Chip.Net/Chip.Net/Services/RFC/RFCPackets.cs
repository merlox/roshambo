using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Services.RFC
{
	public class RFCExecute : Packet
	{
		public byte FunctionId { get; set; }
		public byte[] FunctionParameters { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);

			buffer.Write((byte)FunctionId);
			buffer.Write((byte[])FunctionParameters);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);

			FunctionId = buffer.ReadByte();
			FunctionParameters = buffer.ReadByteArray();
		}
	}

	public class RFCExecuteFunction : RFCExecute {
		public short ExpectedResponseId { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((short)ExpectedResponseId);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			ExpectedResponseId = buffer.ReadInt16();
		}
	}

	public class RFCExecuteFunctionResponse : Packet {
		public short ResponseId { get; set; }
		public byte[] ResponseData { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((byte[])ResponseData);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			ResponseData = buffer.ReadByteArray();
		}
	}
}
