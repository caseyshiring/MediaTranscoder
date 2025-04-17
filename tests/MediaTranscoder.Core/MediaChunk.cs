using System;

namespace MediaTranscoder.Core.Models
{
    /// <summary>
    /// Represents a memory-efficient chunk of a media file
    /// </summary>
    public class MediaChunk : IDisposable
    {
        /// <summary>
        /// Gets the unique identifier for this chunk
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the byte offset of this chunk in the source file
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Gets the size of this chunk in bytes
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets a value indicating whether this chunk represents the end of the media file
        /// </summary>
        public bool IsLastChunk { get; }

        /// <summary>
        /// Gets the byte data buffer containing the chunk data
        /// </summary>
        /// <remarks>
        /// This may be a pooled buffer to reduce GC pressure. Use <see cref="Span"/> to access the valid data.
        /// </remarks>
        public byte[] Buffer { get; }

        /// <summary>
        /// Gets a span over the valid portion of the <see cref="Buffer"/>
        /// </summary>
        public Span<byte> Span => new Span<byte>(Buffer, 0, Size);

        /// <summary>
        /// Gets or sets application-specific metadata for this chunk
        /// </summary>
        public object? Metadata { get; set; }

        /// <summary>
        /// Gets a value indicating whether this chunk has been processed
        /// </summary>
        public bool IsProcessed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MediaChunk class
        /// </summary>
        /// <param name="buffer">Byte buffer containing the chunk data</param>
        /// <param name="offset">Byte offset of this chunk in the source file</param>
        /// <param name="size">Size of this chunk in bytes</param>
        /// <param name="isLastChunk">Whether this is the last chunk in the file</param>
        public MediaChunk(byte[] buffer, long offset, int size, bool isLastChunk = false)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (size > buffer.Length) throw new ArgumentException("Size cannot exceed buffer length", nameof(size));
            
            Id = Guid.NewGuid();
            Buffer = buffer;
            Offset = offset;
            Size = size;
            IsLastChunk = isLastChunk;
            IsProcessed = false;
        }

        /// <summary>
        /// Creates a processed version of this chunk with new content
        /// </summary>
        /// <param name="processedBuffer">The processed buffer data</param>
        /// <param name="size">The size of the processed data</param>
        /// <returns>A new MediaChunk containing the processed data</returns>
        public MediaChunk WithProcessedData(byte[] processedBuffer, int size)
        {
            var processedChunk = new MediaChunk(processedBuffer, Offset, size, IsLastChunk)
            {
                IsProcessed = true,
                Metadata = this.Metadata
            };
            
            return processedChunk;
        }

        /// <summary>
        /// Releases any resources associated with this chunk
        /// </summary>
        /// <remarks>
        /// This can be used to return pooled buffers to a memory pool for reuse
        /// </remarks>
        public void Dispose()
        {
            // In a real implementation with buffer pooling, this would return the buffer to the pool
            GC.SuppressFinalize(this);
        }
    }
}
