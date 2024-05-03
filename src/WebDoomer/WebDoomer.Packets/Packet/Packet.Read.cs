using System.Text;

namespace WebDoomer.Packets;

public partial class Packet
{
    /// <summary>
    /// Gets the <see cref="byte"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="byte"/>.</returns>
    public byte GetByte(bool updateReadPosition = true)
    {
        if (this.UnreadBytes < sizeof(byte)) {
            throw new InvalidOperationException($"Value of type {nameof(Byte)} cannot be determined with the remaining bytes in the readable buffer (target size is {this.UnreadBytes - sizeof(byte)})");
        }

        var value = this.ByteBuffer[this._readPosition];
        if (updateReadPosition) {
            this._readPosition += sizeof(byte);
        }

        return value;
    }

    /// <summary>
    /// Gets a <see cref="byte"/> collection from the current write position. The number of bytes to retrieve is defined with <paramref name="count"/>.
    /// </summary>
    /// <param name="count">The number of bytes to retrieve.</param>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="byte"/>.</returns>
    public IEnumerable<byte> GetBytes(int length, bool updateReadPosition = true)
    {
        if (this.UnreadBytes < length) {
            throw new InvalidOperationException($"Value of type {nameof(IEnumerable<Byte>)} cannot be determined with the remaining bytes in the readable buffer (target size is {this.UnreadBytes - length})");
        }

        var value = this.ByteBuffer[this._readPosition..(this._readPosition + length)];
        if (updateReadPosition) {
            this._readPosition += length;
        }

        return value;
    }

    /// <summary>
    /// Gets the <see cref="Guid"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="Guid"/>.</returns>
    public Guid GetGuid(bool updateReadPosition = true)
    {
        // sizeof() is not possible here, as the result "depends per system". Despite that the size of a Guid is pretty much always 16 bytes.

        try
        {
            return new Guid(this.GetBytes(/* sizeof(Guid) */16, updateReadPosition).ToArray());
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Guid)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="char"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="char"/>.</returns>
    public char GetChar(bool updateReadPosition = true)
    {
        try
        {
            // Considering we are encoding in ASCII, and requesting a single byte, `GetChars` will return a single character.
            var @byte = this.GetByte(updateReadPosition);
            var character = Encoding.ASCII.GetChars([ @byte ]);
            return character[0];
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Char)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="bool"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="bool"/>.</returns>
    public bool GetBool(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToBoolean([ this.GetByte(updateReadPosition) ], 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Boolean)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="string"/> from the current write position.
    /// </summary>
    /// <param name="checkNullTerminatedByte">If true, fetch from the buffer until a null-termianted byte is encountered. If false, assume the first byte specifies length.</param>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="string"/>.</returns>
    public string GetString(bool checkNullTerminatedByte = false, bool updateReadPosition = true)
    {
        try
        {
            // If the string has a null terminated byte, recursively loop until we either found it, or run out of characters.
            if (checkNullTerminatedByte)
            {
                var returnedString = "";
                while(true)
                {
                    var nextChar = this.GetChar(updateReadPosition);
                    if (nextChar != '\0') {
                        returnedString += nextChar;
                        continue;
                    }

                    break;
                }

                return returnedString;
            }

            var stringLength = Convert.ToInt32(this.GetByte(updateReadPosition));
            return this.GetString(stringLength, updateReadPosition);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(String)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="string"/> from the current write position.
    /// </summary>
    /// <param name="length">The length of the string to retreive.</param>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="string"/>.</returns>
    public string GetString(int length, bool updateReadPosition = true)
    {
        try
        {
			var @string = Encoding.ASCII.GetString(this.GetBytes(length).ToArray());

			if (updateReadPosition)
			{
				this._readPosition += @string.Length;
			}

			return @string;
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(String)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="float"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="float"/>.</returns>
    public float GetFloat(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToSingle(this.GetBytes(sizeof(float), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Single)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="double"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="double"/>.</returns>
    public double GetDouble(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToDouble(this.GetBytes(sizeof(double), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Double)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="short"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="short"/>.</returns>
    public short GetShort(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToInt16(this.GetBytes(sizeof(short), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Int16)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="int"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="int"/>.</returns>
    public int GetInt(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToInt32(this.GetBytes(sizeof(int), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Int32)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="long"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="long"/>.</returns>
    public long GetLong(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToInt64(this.GetBytes(sizeof(long), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(Int64)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="ushort"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="ushort"/>.</returns>
    public ushort GetUShort(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToUInt16(this.GetBytes(sizeof(ushort), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(UInt16)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="uint"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="uint"/>.</returns>
    public uint GetUInt(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToUInt32(this.GetBytes(sizeof(uint), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(UInt32)}.", exception);
        }
    }

    /// <summary>
    /// Gets the <see cref="ulong"/> from the current write position.
    /// </summary>
    /// <param name="updateReadPosition">Determines if the packet's read position should be adjusted with this read. Defaults to <see langword="true"/>.</param>
    /// <returns>A <see cref="ulong"/>.</returns>
    public ulong GetULong(bool updateReadPosition = true)
    {
        try
        {
            return BitConverter.ToUInt64(this.GetBytes(sizeof(ulong), updateReadPosition).ToArray(), 0);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException($"Unable to read value of type {nameof(UInt64)}.", exception);
        }
    }
}
