using System;
using System.Threading;
using System.Threading.Tasks;
using MediaTranscoder.Core.Models;
using MediaTranscoder.Core.Services;

namespace MediaTranscoder.Core.Interfaces
{
    /// <summary>
    /// Defines operations for logging application events
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">Optional exception associated with the error</param>
        void Error(string message, Exception? exception = null);
        
        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        void Warning(string message);
        
        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        void Information(string message);
        
        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        void Debug(string message);
    }

    /// <summary>
    /// Defines operations for transcoding media files
    /// </summary>
    public interface ITranscoder
    {
        /// <summary>
        /// Transcodes a media file to the target format
        /// </summary>
        /// <param name="inputFile">Source media file</param>
        /// <param name="outputPath">Destination path</param>
        /// <param name="targetFormat">Format to transcode to</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Statistics about the completed transcoding operation</returns>
        Task<TranscodeResult> TranscodeAsync(
            MediaFile inputFile,
            string outputPath,
            TranscodeOptions targetFormat,
            IProgress<TranscodeProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines operations for reading media files in chunks
    /// </summary>
    public interface IMediaReader
    {
        /// <summary>
        /// Reads a chunk of data from a media file
        /// </summary>
        /// <param name="file">The media file to read from</param>
        /// <param name="offset">The byte offset to start reading from</param>
        /// <param name="size">The number of bytes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The read media chunk</returns>
        Task<MediaChunk> ReadChunkAsync(
            MediaFile file,
            long offset,
            int size,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines operations for writing processed media chunks to an output file
    /// </summary>
    public interface IMediaWriter
    {
        /// <summary>
        /// Initializes the writer for a new output file
        /// </summary>
        /// <param name="outputPath">Path where the output file will be written</param>
        /// <param name="targetFormat">Target format settings</param>
        /// <param name="sourceFormat">Source format information</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task InitializeAsync(
            string outputPath, 
            TranscodeOptions targetFormat,
            MediaFormat sourceFormat,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes a processed media chunk to the output file
        /// </summary>
        /// <param name="chunk">The processed media chunk to write</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task WriteChunkAsync(
            MediaChunk chunk,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finalizes the output file after all chunks have been written
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task FinalizeAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines operations for processing media chunks
    /// </summary>
    public interface IChunkProcessor
    {
        /// <summary>
        /// Processes a media chunk according to the specified target format
        /// </summary>
        /// <param name="chunk">The input media chunk</param>
        /// <param name="sourceFormat">The source format information</param>
        /// <param name="targetOptions">The target format options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The processed media chunk</returns>
        Task<MediaChunk> ProcessChunkAsync(
            MediaChunk chunk,
            MediaFormat sourceFormat,
            TranscodeOptions targetOptions,
            CancellationToken cancellationToken = default);
    }
}
