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

	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void PacketShouldHaveCorrectPacketSize(string value)
	{
		Debug.Assert(value != null);
		Packet? testPacket = null;

		// Writing the value into the packet should have the exact packet size that is written into it.
		var data = Encoding.ASCII.GetBytes(value);
		var exception = Record.Exception(() =>
			testPacket = new Packet(data));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		Assert.Equal(testPacket.PacketSize, value.Length);
	}

	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void PacketShouldHaveCorrectWriteOffset(string value)
	{
		Debug.Assert(value != null);
		Packet? testPacket = null;

		// Writing the value into the packet should have the right write offset.
		var data = Encoding.ASCII.GetBytes(value);
		var exception = Record.Exception(() =>
			testPacket = new Packet(data));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		Assert.Equal(testPacket.WritePosition, value.Length);
	}

	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void PacketShouldHaveCorrectPacketSizeWithAdjustedWriteOffset(string value)
	{
		Debug.Assert(value != null);
		Packet? testPacket = null;

		// Writing the value into the packet should have the right write offset.
		var data = Encoding.ASCII.GetBytes(value);
		var exception = Record.Exception(() =>
			testPacket = new Packet(data, 500));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		Assert.Equal(testPacket.WritePosition, value.Length);

		// Adjust write position and validate again.
		testPacket.WritePosition -= 5;
		Assert.Equal(testPacket.WritePosition, value.Length - 5);

		// Adjust so that the packet size also increases.
		testPacket.WritePosition += 10;
		Assert.Equal(testPacket.WritePosition, value.Length + 5);
		Assert.Equal(testPacket.PacketSize, value.Length + 5);

		// Decrease again and the packet size should remain the same.
		testPacket.WritePosition -= 5;
		Assert.Equal(testPacket.WritePosition, value.Length);
		Assert.Equal(testPacket.PacketSize, value.Length + 5);
	}

	[Fact]
	public void PacketShouldThrowExceptionIfWritePositionIsInvalid()
	{
		Packet? testPacket = null;
		var @string = "Hello, World!";

		// Writing the value into the packet should have the right write offset.
		var data = Encoding.ASCII.GetBytes(@string);
		var exception = Record.Exception(() =>
			testPacket = new Packet(data));

		Assert.Null(exception);
		Debug.Assert(testPacket != null);

		Assert.Equal(testPacket.WritePosition, @string.Length);

		// Should throw an exception if below 0
		exception = Record.Exception(() => testPacket.WritePosition = -1);

		Assert.NotNull(exception);
		_ = Assert.IsType<ArgumentOutOfRangeException>(exception);

		// Should throw an exception if above the packet size.
		exception = Record.Exception(() => testPacket.WritePosition = @string.Length + 1);

		Assert.NotNull(exception);
		_ = Assert.IsType<ArgumentOutOfRangeException>(exception);

		// Should not throw if set to 0.
		exception = Record.Exception(() => testPacket.WritePosition = 0);

		Assert.Null(exception);

		// Should not throw if set to packet size.
		exception = Record.Exception(() => testPacket.WritePosition = testPacket.PacketSize);

		Assert.Null(exception);
	}
}