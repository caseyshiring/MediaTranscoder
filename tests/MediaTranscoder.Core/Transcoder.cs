using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaTranscoder.Core.Models;
using MediaTranscoder.Core.Interfaces;
using MediaTranscoder.Core.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MediaTranscoder.Core.Services
{
    /// <summary>
    /// Core service responsible for managing the media transcoding process
    /// </summary>
    public class Transcoder : ITranscoder
    {
        private readonly IChunkProcessor _chunkProcessor;
        private readonly IMediaReader _mediaReader;
        private readonly IMediaWriter _mediaWriter;
        private readonly ILogger _logger;
        private readonly TranscoderOptions _options;

        /// <summary>
        /// Initializes a new instance of the Transcoder class
        /// </summary>
        /// <param name="chunkProcessor">Service for processing media chunks</param>
        /// <param name="mediaReader">Service for reading media files</param>
        /// <param name="mediaWriter">Service for writing media files</param>
        /// <param name="logger">Logging service</param>
        /// <param name="options">Transcoder configuration options</param>
        public Transcoder(
            IChunkProcessor chunkProcessor,
            IMediaReader mediaReader,
            IMediaWriter mediaWriter,
            ILogger logger,
            TranscoderOptions options)
        {
            _chunkProcessor = chunkProcessor ?? throw new ArgumentNullException(nameof(chunkProcessor));
            _mediaReader = mediaReader ?? throw new ArgumentNullException(nameof(mediaReader));
            _mediaWriter = mediaWriter ?? throw new ArgumentNullException(nameof(mediaWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Transcodes a media file to the target format
        /// </summary>
        /// <param name="inputFile">Source media file</param>
        /// <param name="outputPath">Destination path</param>
        /// <param name="targetFormat">Format to transcode to</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Statistics about the completed transcoding operation</returns>
        public async Task<TranscodeResult> TranscodeAsync(
            MediaFile inputFile,
            string outputPath,
            TranscodeOptions targetFormat,
            IProgress<TranscodeProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (inputFile == null) throw new ArgumentNullException(nameof(inputFile));
            if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException("Output path cannot be empty", nameof(outputPath));
            if (targetFormat == null) throw new ArgumentNullException(nameof(targetFormat));

            // Ensure the input file has been analyzed
            if (!inputFile.IsAnalyzed)
            {
                await inputFile.AnalyzeAsync(cancellationToken);
            }

            _logger.Information($"Starting transcoding of {Path.GetFileName(inputFile.FilePath)} to {Path.GetFileName(outputPath)}");
            var stopwatch = Stopwatch.StartNew();

            // Calculate optimal chunk size based on file size and available memory
            int chunkSize = CalculateOptimalChunkSize(inputFile.FileSize);
            int chunkCount = (int)Math.Ceiling((double)inputFile.FileSize / chunkSize);
            
            _logger.Debug($"Using chunk size: {FormatByteSize(chunkSize)}, total chunks: {chunkCount}");

            // Initialize writer
            await _mediaWriter.InitializeAsync(outputPath, targetFormat, inputFile.Format, cancellationToken);

            // Process chunks in parallel with controlled concurrency
            var chunksProcessed = 0;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.MaxParallelism,
                CancellationToken = cancellationToken
            };

            var chunks = GenerateChunks(inputFile.FileSize, chunkSize);
            
            await Parallel.ForEachAsync(chunks, parallelOptions, async (chunk, token) =>
            {
                // Read chunk from source file
                var mediaChunk = await _mediaReader.ReadChunkAsync(inputFile, chunk.Offset, chunk.Size, token);
                
                // Process the chunk
                var processedChunk = await _chunkProcessor.ProcessChunkAsync(
                    mediaChunk, 
                    inputFile.Format, 
                    targetFormat,
                    token);
                
                // Write processed chunk
                await _mediaWriter.WriteChunkAsync(processedChunk, token);

                // Update progress
                var currentChunksProcessed = Interlocked.Increment(ref chunksProcessed);
                progress?.Report(new TranscodeProgress(
                    (double)currentChunksProcessed / chunkCount,
                    currentChunksProcessed,
                    chunkCount));
            });

            // Finalize the output file
            await _mediaWriter.FinalizeAsync(cancellationToken);

            // Create and return result
            stopwatch.Stop();
            var result = new TranscodeResult
            {
                InputFile = inputFile,
                OutputPath = outputPath,
                DurationMs = stopwatch.ElapsedMilliseconds,
                ChunksProcessed = chunksProcessed,
                InputSize = inputFile.FileSize,
                OutputSize = new FileInfo(outputPath).Length
            };

            _logger.Information($"Transcoding completed in {result.DurationMs / 1000.0:F2} seconds. " +
                              $"Output size: {FormatByteSize(result.OutputSize)}");

            return result;
        }

        /// <summary>
        /// Calculates the optimal chunk size based on file size and available memory
        /// </summary>
        /// <param name="fileSize">Size of the file in bytes</param>
        /// <returns>Optimal chunk size in bytes</returns>
        private int CalculateOptimalChunkSize(long fileSize)
        {
            // If custom chunk size is set in options, use that
            if (_options.ChunkSizeBytes > 0)
                return _options.ChunkSizeBytes;

            // Get available memory (with safety margin)
            var availableMemory = GetAvailableMemory() * 0.7;
            
            // Base chunk size on file size and parallelism
            var baseChunkSize = fileSize / Math.Max(1, _options.MaxParallelism * 2);
            
            // Ensure chunk size isn't too large for available memory
            var maxChunkByMemory = availableMemory / _options.MaxParallelism;
            var calculatedSize = Math.Min(baseChunkSize, maxChunkByMemory);
            
            // Clamp within reasonable bounds
            var minChunkSize = 1 * 1024 * 1024; // 1 MB
            var maxChunkSize = 64 * 1024 * 1024; // 64 MB
            
            return (int)Math.Clamp(calculatedSize, minChunkSize, maxChunkSize);
        }

        /// <summary>
        /// Gets an estimate of available system memory
        /// </summary>
        /// <returns>Available memory in bytes</returns>
        private long GetAvailableMemory()
        {
            // A simple approximation - in production code you would use a more accurate method
            // specific to the operating system
            return 1024L * 1024L * 1024L; // Assume 1 GB available
        }

        /// <summary>
        /// Formats a byte count into a human-readable string
        /// </summary>
        /// <param name="bytes">Byte count</param>
        /// <returns>Formatted string (e.g., "4.2 MB")</returns>
        private string FormatByteSize(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblBytes = bytes;
            while (dblBytes >= 1024 && i < suffix.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }
            return $"{dblBytes:0.##} {suffix[i]}";
        }

        /// <summary>
        /// Generates a sequence of chunk definitions for a file
        /// </summary>
        /// <param name="fileSize">Total file size in bytes</param>
        /// <param name="chunkSize">Size of each chunk in bytes</param>
        /// <returns>Sequence of chunk definitions</returns>
        private IEnumerable<(long Offset, int Size)> GenerateChunks(long fileSize, int chunkSize)
        {
            long offset = 0;
            while (offset < fileSize)
            {
                int size = (int)Math.Min(chunkSize, fileSize - offset);
                yield return (offset, size);
                offset += size;
            }
        }
    }

    /// <summary>
    /// Represents the result of a transcoding operation
    /// </summary>
    public class TranscodeResult
    {
        /// <summary>
        /// Gets or sets the input media file
        /// </summary>
        public MediaFile? InputFile { get; set; }
        
        /// <summary>
        /// Gets or sets the output file path
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the duration of the transcoding operation in milliseconds
        /// </summary>
        public long DurationMs { get; set; }
        
        /// <summary>
        /// Gets or sets the number of chunks processed
        /// </summary>
        public int ChunksProcessed { get; set; }
        
        /// <summary>
        /// Gets or sets the input file size in bytes
        /// </summary>
        public long InputSize { get; set; }
        
        /// <summary>
        /// Gets or sets the output file size in bytes
        /// </summary>
        public long OutputSize { get; set; }
        
        /// <summary>
        /// Gets the processing throughput in MB/s
        /// </summary>
        public double ThroughputMBps => InputSize / (1024.0 * 1024.0) / (DurationMs / 1000.0);
        
        /// <summary>
        /// Gets the compression ratio (input size / output size)
        /// </summary>
        public double CompressionRatio => (double)InputSize / OutputSize;
    }

    /// <summary>
    /// Reports progress during transcoding operations
    /// </summary>
    public class TranscodeProgress
    {
        /// <summary>
        /// Gets the overall progress as a fraction (0.0 to 1.0)
        /// </summary>
        public double Percentage { get; }
        
        /// <summary>
        /// Gets the number of chunks processed so far
        /// </summary>
        public int ChunksProcessed { get; }
        
        /// <summary>
        /// Gets the total number of chunks to process
        /// </summary>
        public int TotalChunks { get; }

        /// <summary>
        /// Initializes a new instance of the TranscodeProgress class
        /// </summary>
        /// <param name="percentage">Overall progress (0.0 to 1.0)</param>
        /// <param name="chunksProcessed">Number of chunks processed</param>
        /// <param name="totalChunks">Total number of chunks</param>
        public TranscodeProgress(double percentage, int chunksProcessed, int totalChunks)
        {
            Percentage = percentage;
            ChunksProcessed = chunksProcessed;
            TotalChunks = totalChunks;
        }
    }
}
