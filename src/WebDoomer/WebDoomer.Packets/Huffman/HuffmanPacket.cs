namespace WebDoomer.Packets;

/// <summary>
/// A specialized packet that adds support for Huffman encoding.
/// </summary>
public sealed class HuffmanPacket: Packet
{
    private readonly HuffmanConverter _huffmanConverter;

	/// <summary>
	/// Returns the size of the encoded packet.
	/// </summary>
	public int EncodedPacketSize => this.ByteBuffer.Length;

	/// <summary>
	/// Creates a new empty Huffman packet with a set size.
	/// </summary>
	/// <param name="size">The size to give the internal array buffer.</param>
	public HuffmanPacket(int size = 500)
        : base(size)
    {
        this._huffmanConverter = new();
    }

	/// <summary>
	/// Created a new Huffman packet from the byte array provided.
	/// </summary>
	/// <param name="bytes">The data that represents the packet to create.</param>
	public HuffmanPacket(ReadOnlySpan<byte> bytes)
    {
		this._huffmanConverter = new();
		var decodedData = this._huffmanConverter.Decode(bytes);
		this.ByteBuffer = new byte[decodedData.Length];
        _ = this.Write(decodedData.ToArray(), false);
    }

	// TODO: Cache
	/// <inheritdoc />
	public override ReadOnlySpan<byte> GetBuffer()
	{
		return this._huffmanConverter.Encode(this.ByteBuffer);
	}
}
