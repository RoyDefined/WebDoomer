using System.Collections.ObjectModel;

namespace WebDoomer.Packets;

/// <summary>
/// Represents the main packet class responsible for building, sending, retrieving and handling any form of data send over the network.
/// </summary>
public partial class Packet
{
    /// <summary>
	/// Creates a new empty packet with a set size.
	/// </summary>
	/// <param name="size">The size to give the internal array buffer.</param>
    public Packet(int size = 500)
    {
        this.ByteBuffer = new byte[size];
	}

	/// <summary>
	/// Created a new packet from the byte array provided.
	/// </summary>
	/// <param name="bytes">The data that represents the packet to create.</param>
	public Packet(ReadOnlySpan<byte> bytes)
		: this(bytes.Length)
	{
		_ = this.Write(bytes.ToArray(), false);
	}

	/// <summary>
	/// Created a new packet from the byte array provided and the specified length.
	/// </summary>
	/// <param name="bytes">The data that represents the packet to create.</param>
	/// <param name="size">The size to give the internal array buffer.</param>
	/// <remarks>Note this constructor variant is missing from <see cref="HuffmanPacket"/> as the encoding makes for a difference in length.</remarks>
	public Packet(ReadOnlySpan<byte> bytes, int size)
		: this(size)
	{
		_ = this.Write(bytes.ToArray(), false);
	}

	/// <summary>
	/// The data contained in this packet.
	/// </summary>
	protected byte[] ByteBuffer { get; init; }

	/// <summary>
	/// The current read position for the <see cref="_byteBuffer"/>.
	/// </summary>
	private int _readPosition;

	/// <summary>
	/// The current write position for the <see cref="_byteBuffer"/>.
	/// </summary>
	private int _writePosition;

	/// <summary>
	/// The number of bytes in the current packet.
	/// </summary>
	public int PacketSize => this._writePosition;

    /// <summary>
    /// The amount of unread bytes left in the packet.
    /// </summary>
    public int UnreadBytes => this.PacketSize - this._readPosition;

	/// <summary>
	/// Returns the number of writeable bytes that are left in the packet.
	/// </summary>
	public int RemainingWriteableBytes => this.ByteBuffer.Length - this._writePosition;

    /// <summary>
    /// Resets the read position of the packet back to the place where it can start reading for data.
    /// </summary>
    public void ResetReadPosition()
    {
        this._readPosition = 0;
    }

	/// <summary>
	/// Returns the buffer of this packet.
	/// </summary>
	/// <returns>The buffer as a <see cref="ReadOnlySpan{T}"/>.</returns>
	public virtual ReadOnlySpan<byte> GetBuffer()
	{
		return this.ByteBuffer.AsSpan(0..this._writePosition);
	}
}
