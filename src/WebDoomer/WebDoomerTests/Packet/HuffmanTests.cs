using System.Diagnostics;
using System.Text;

namespace WebDoomerTests;

public class HuffmanTests
{
	/// <summary>
	/// Runs a test that compares the string against one that has been encoded and decoded by the converter.
	/// </summary>
	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void StringReturnsSameValueWhenEncodedAndDecodedThroughConverter(string value)
	{
		var bytes = Encoding.ASCII.GetBytes(value);

		var converter = new HuffmanConverter();
		var data = converter.Encode(bytes);
		var compareValue = Encoding.ASCII.GetString(converter.Decode(data));

		Assert.Equal(value, compareValue);
	}

	/// <summary>
	/// Runs a test that compares the string against one that has been encoded by the converter and then decoded in the packet.
	/// Additionally the data is retrieved from the packet's buffer.
	/// </summary>
	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void StringReturnsSameValueWhenEncodedAndDecodedThroughPacket(string value)
	{
		var bytes = Encoding.ASCII.GetBytes(value);

		var converter = new HuffmanConverter();
		var encodedBytes = converter.Encode(bytes);

		var packet = new HuffmanPacket(encodedBytes);
		var compareBytes = converter.Decode(packet.GetBuffer());
		var compareValue = Encoding.ASCII.GetString(compareBytes);

		Assert.Equal(value, compareValue);
	}

	/// <summary>
	/// Runs a test that compares the string against one that has been encoded by the converter and then fetched from the packet's compressed buffer.
	/// This tests if the packet properly encodes the data again and allows it to be properly fetched from the packet.
	/// </summary>
	[Theory]
	[InlineData("Hello world!")]
	[InlineData("This should definitely work.")]
	[InlineData("Because if it doesn't...")]
	[InlineData("Then we should probably fix it.")]
	public void StringAsByteArrayReturnsSameValueWhenEncodedAndReEncodedThroughPacket(string value)
	{
		var bytes = Encoding.ASCII.GetBytes(value);

		var converter = new HuffmanConverter();
		var encodedBytes = converter.Encode(bytes);

		var packet = new HuffmanPacket(encodedBytes);
		var compareBytes = packet.GetBuffer();

		Assert.Equal(encodedBytes.ToArray(), compareBytes.ToArray());
	}

	[Fact]
	public void PacketWithCustomSizeAllowsWritingAndReading()
	{
		Packet? testPacket = null;

		// Writing 5 bytes into a packet that accepts 5 bytes should work.
		var exception = Record.Exception(() =>
			testPacket = new HuffmanPacket(5)
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
}