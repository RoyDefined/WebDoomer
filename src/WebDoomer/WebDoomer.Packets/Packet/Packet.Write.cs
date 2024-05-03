using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDoomer.Packets;

public partial class Packet
{
	/// <summary>
	/// Writes a <see cref="byte"/> into the packet.
	/// </summary>
	/// <param name="value">The <see cref="byte"/> to write.</param>
	/// <returns>The updated packet so additional calls can be chained.</returns>
	public Packet Write(byte value)
    {
		if (this.RemainingWriteableBytes == 0)
		{
			throw new InvalidOperationException("Unable to write byte into the packet as the remaining byte size of the packet is 0.");
		}

        this.ByteBuffer[this.WritePosition++] = value;
        return this;
    }

	/// <summary>
	/// Writes a, <see cref="IEnumerable{T}"/> of type <see cref="byte"/> into the packet. This does specify the number of bytes written in the collection with the initial byte.
	/// </summary>
	/// <param name="value">The <see cref="byte"/> collection to write.</param>
	/// <param name="addLength">If true, prefix the buffer with the length of the <see cref="IEnumerable{T}"/>. Defaults to <see langword="true"/>.</param>
	/// <returns>The updated packet so additional calls can be chained.</returns>
	public Packet Write(byte[] value, bool addLength = true)
    {
		ArgumentNullException.ThrowIfNull(value, nameof(value));

		var length = value.Length;
		if (addLength)
        {
			if (this.RemainingWriteableBytes < length + 1)
			{
				throw new InvalidOperationException($"Unable to write bytes + length into the packet as the remaining byte size of the packet is smaller than the required bytes ({this.RemainingWriteableBytes} < {length + 1}).");
			}

			// Ensure the first byte can specify the length of the string (which must be a maximum of 255 (not 256 because we start at 0)).
			if (length > 255)
            {
                throw new InvalidOperationException($"Cannot write length of the given collection as it exceeds the maximum length [{length}/{sizeof(byte) - 1}].");
            }

            _ = this.Write(Convert.ToByte(length));
        }
		else
		{
			if (this.RemainingWriteableBytes < length)
			{
				throw new InvalidOperationException($"Unable to write bytes into the packet as the remaining byte size of the packet is smaller than the required bytes ({this.RemainingWriteableBytes} < {length}).");
			}
		}

		foreach(var @byte in value)
		{
			_ = this.Write(@byte);
		}
        
        return this;
    }

    /// <summary>
    /// Writes a <see cref="Guid"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(Guid value)
    {
        try
        {
            return this.Write(value.ToByteArray(), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Guid)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="char"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="char"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(char value)
    {
        try
        {
            // `GetBytes` will return a single byte.
            return this.Write(Encoding.ASCII.GetBytes(new[] { value })[0]);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Char)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="bool"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="bool"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(bool value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value).Single());
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Boolean)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="string"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to write.</param>
    /// <param name="identificationType">The type of identification to use when writing the string.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(string value, PacketStringLengthIdentifier identificationType = PacketStringLengthIdentifier.LengthPrefix)
    {
        try
        {
            if (identificationType == PacketStringLengthIdentifier.NullTerminated) {
                value += '\0';
            }

            var addLength = identificationType == PacketStringLengthIdentifier.LengthPrefix;
            return this.Write(Encoding.ASCII.GetBytes(value), addLength);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(String)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="float"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="float"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(float value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Single)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="double"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="double"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(double value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Double)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="short"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="short"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(short value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Int16)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="int"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="int"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(int value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Int32)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="long"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="long"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(long value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(Int64)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="ushort"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(ushort value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(UInt16)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="uint"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="uint"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained.</returns>
    public Packet Write(uint value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(UInt32)} into the packet.", exception);
        }
    }

    /// <summary>
    /// Writes a <see cref="ulong"/> into the packet.
    /// </summary>
    /// <param name="value">The <see cref="ulong"/> to write.</param>
    /// <returns>The updated packet so additional calls can be chained </returns>
    public Packet Write(ulong value)
    {
        try
        {
            return this.Write(BitConverter.GetBytes(value), false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Exception while writing a {nameof(UInt64)} into the packet.", exception);
        }
    }
}
