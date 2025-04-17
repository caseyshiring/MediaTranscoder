using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTranscoder.Core.Models
{
    /// <summary>
    /// Represents a media file in the system with metadata and processing capabilities
    /// </summary>
    public class MediaFile
    {
        /// <summary>
        /// Gets the unique identifier for this media file
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the file path of the media file
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the file size in bytes
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// Gets the media format information
        /// </summary>
        public MediaFormat Format { get; private set; }

        /// <summary>
        /// Gets the creation timestamp of the media file
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets whether the file has been fully analyzed
        /// </summary>
        public bool IsAnalyzed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MediaFile class
        /// </summary>
        /// <param name="filePath">Path to the media file</param>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        public MediaFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Media file not found: {filePath}");

            Id = Guid.NewGuid();
            FilePath = filePath;
            FileSize = new FileInfo(filePath).Length;
            CreatedAt = DateTime.UtcNow;
            Format = new MediaFormat();
            IsAnalyzed = false;
        }

        /// <summary>
        /// Analyzes the media file to extract format information
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AnalyzeAsync(CancellationToken cancellationToken = default)
        {
            // In a real implementation, this would analyze the file headers
            // and extract metadata about codecs, resolution, etc.
            await Task.Delay(100, cancellationToken); // Simulated work
            
            // For demonstration, we'll create mock data based on file extension
            string extension = Path.GetExtension(FilePath).ToLowerInvariant();
            
            Format = extension switch
            {
                ".mp4" => new MediaFormat 
                { 
                    Container = "MP4",
                    VideoCodec = "H.264",
                    AudioCodec = "AAC",
                    Width = 1920,
                    Height = 1080,
                    FrameRate = 30
                },
                ".mov" => new MediaFormat 
                {
                    Container = "QuickTime",
                    VideoCodec = "ProRes",
                    AudioCodec = "PCM",
                    Width = 3840,
                    Height = 2160,
                    FrameRate = 24
                },
                _ => new MediaFormat
                {
                    Container = "Unknown",
                    VideoCodec = "Unknown",
                    AudioCodec = "Unknown"
                }
            };
            
            IsAnalyzed = true;
        }

        /// <summary>
        /// Creates a file stream for reading
        /// </summary>
        /// <returns>FileStream for the media file</returns>
        public Stream OpenRead()
        {
            return new FileStream(
                FilePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);
        }
    }
}
