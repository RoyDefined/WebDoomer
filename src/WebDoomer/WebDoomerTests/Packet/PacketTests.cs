using System.Diagnostics;
using System.Text;

namespace WebDoomerTests;

public class PacketTests
{
    [Fact]
    public void PacketReadPositionResetResetsReadPosition()
    {
        var testPacket = new Packet()
            .Write(1);

        var exception = Record.Exception(() =>
        {
			_ = testPacket.GetInt();
            testPacket.ResetReadPosition();
			_ = testPacket.GetInt();
        });
		Assert.Null(exception);
	}

	[Fact]
	public void PacketWithCustomSizeAllowsWritingAndReading()
	{
		Packet? testPacket = null;

		// Writing 5 bytes into a packet that accepts 5 bytes should work.
		var exception = Record.Exception(() =>
			testPacket = new Packet(5)
				.Write((byte)1)
				.Write((byte)2)
				.Write((byte)3)
				.Write((byte)4)
				.Write((byte)5));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		// Writing one more byte should throw an exception.
		exception = Record.Exception(() =>
			testPacket.Write((byte)1));

		Assert.NotNull(exception);
		_ = Assert.IsType<InvalidOperationException>(exception);
	}

	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void PacketWithCustomSizeAndBytesAllowsWritingAndReading(string value)
	{
		Packet? testPacket = null;

		// Writing the value into the packet whilst providing the value + 1 manually should work fine.
		var data = Encoding.ASCII.GetBytes(value);
		var exception = Record.Exception(() =>
			testPacket = new Packet(data, data.Length + 1)
				.Write((byte)1));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		// Writing one more byte should throw an exception.
		exception = Record.Exception(() =>
			testPacket.Write((byte)1));

		Assert.NotNull(exception);
		_ = Assert.IsType<InvalidOperationException>(exception);
	}
}