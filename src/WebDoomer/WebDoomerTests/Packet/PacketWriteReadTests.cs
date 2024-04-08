using System.Diagnostics;

namespace WebDoomerTests;

// Below are a bunch of tests that aim to test writing and then reading values with packets.
// It will the integration of packets and wether or not the values persist correctly.

public class PacketWriteReadTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public void ShortReturnsSameValueWhenPackedAndUnpacked(short expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetShort();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void IntReturnsSameValueWhenPackedAndUnpacked(int expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetInt();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void LongReturnsSameValueWhenPackedAndUnpacked(long expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetLong();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData("018ba974-7605-4339-8430-4300eb026ba1")]
    [InlineData("33af162b-0395-469d-a0ff-14fa1b716508")]
    [InlineData("870ca111-3189-4e51-b7c7-9f13c07f8290")]
    [InlineData("f0bf65bd-4389-4593-b3aa-5974d28aa56c")]
    [InlineData("e7481592-ce34-4037-a20b-731e640b4b3e")]
    public void GuidReturnsSameValueWhenPackedAndUnpacked(string expectedValueUnparsed)
    {
		Debug.Assert(expectedValueUnparsed != null);

		var expectedValue = Guid.Parse(expectedValueUnparsed);

        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetGuid();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData("Hello world!")]
    [InlineData("This should definitely work.")]
    [InlineData("Because if it doesn't...")]
    [InlineData("Then we should probably fix it.")]
    public void CharsReturnSameValueWhenPackedAndUnpacked(string expectedValue)
    {
		Debug.Assert(expectedValue != null);

        var testPacket = new Packet();

        // Write the characters individually
        foreach (var @char in expectedValue)
        {
            testPacket = testPacket.Write(@char);
        }

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the values back
        var unpackedValue = string.Empty;
        var expectedCharCount = expectedValue.Length;
        while (unpackedValue.Length < expectedCharCount)
        {
            unpackedValue += testPacket.GetChar();
        }

        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BoolReturnsSameValueWhenPackedAndUnpacked(bool expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetBool();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData("Hello world!")]
    [InlineData("Chupapi munyanyo")]
    [InlineData("This should definitely work.")]
    [InlineData("Because if it doesn't...")]
    [InlineData("Why are we even trying?")]
    public void StringReturnsSameValueWhenPackedAndUnpacked(string expectedValue)
    {
		Debug.Assert(expectedValue != null);

		var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetString();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData("Hello world!")]
    [InlineData("Chupapi munyanyo")]
    [InlineData("This should definitely work.")]
    [InlineData("Because if it doesn't...")]
    [InlineData("Why are we even trying?")]
    public void FixedLengthStringReturnsSameValueWhenPackedAndUnpacked(string expectedValue)
    {
		Debug.Assert(expectedValue != null);

		var testPacket = new Packet()
            .Write(expectedValue, PacketStringLengthIdentifier.None);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var stringLength = expectedValue.Length;
        var unpackedValue = testPacket.GetString(stringLength);
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData("Hello world!")]
    [InlineData("Chupapi munyanyo")]
    [InlineData("This should definitely work.")]
    [InlineData("Because if it doesn't...")]
    [InlineData("Why are we even trying?")]
    public void NullTerminatedStringReturnsSameValueWhenPackedAndUnpacked(string expectedValue)
    {
		Debug.Assert(expectedValue != null);

		var testPacket = new Packet()
            .Write(expectedValue, PacketStringLengthIdentifier.NullTerminated);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetString(true);
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public void FloatReturnsSameValueWhenPackedAndUnpacked(float expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetFloat();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void DoubleReturnsSameValueWhenPackedAndUnpacked(double expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetDouble();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData((ushort)0)]
    [InlineData((ushort)1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(ushort.MaxValue)]
    public void UShortReturnsSameValueWhenPackedAndUnpacked(ushort expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetUShort();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData((uint)0)]
    [InlineData((uint)1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(ushort.MinValue)]
    [InlineData(ushort.MaxValue)]
    [InlineData(uint.MaxValue)]
    public void UIntReturnsSameValueWhenPackedAndUnpacked(uint expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetUInt();
        Assert.Equal(unpackedValue, expectedValue);
    }

    [Theory]
    [InlineData((ulong)0)]
    [InlineData((ulong)1)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    [InlineData(ushort.MinValue)]
    [InlineData(ushort.MaxValue)]
    [InlineData(uint.MinValue)]
    [InlineData(uint.MaxValue)]
    [InlineData(ulong.MaxValue)]
    public void ULongReturnsSameValueWhenPackedAndUnpacked(ulong expectedValue)
    {
        var testPacket = new Packet()
            .Write(expectedValue);

        // Pack and unpack
        var preparedBytes = testPacket.GetBuffer();
        testPacket = new Packet(preparedBytes);

        // Get the value back
        var unpackedValue = testPacket.GetULong();
        Assert.Equal(unpackedValue, expectedValue);
    }
}