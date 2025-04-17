namespace MediaTranscoder.Core.Configuration
{
    /// <summary>
    /// Defines options for transcoding operations
    /// </summary>
    public class TranscodeOptions
    {
        /// <summary>
        /// Gets or sets the target container format (e.g., MP4, MKV)
        /// </summary>
        public string Container { get; set; } = "MP4";

        /// <summary>
        /// Gets or sets the target video codec to use
        /// </summary>
        public string VideoCodec { get; set; } = "H.264";

        /// <summary>
        /// Gets or sets the target audio codec to use
        /// </summary>
        public string AudioCodec { get; set; } = "AAC";

        /// <summary>
        /// Gets or sets the target video width in pixels (0 for keeping source width)
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the target video height in pixels (0 for keeping source height)
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the target frame rate (0 for keeping source frame rate)
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// Gets or sets the target video bitrate in bits per second (0 for automatic)
        /// </summary>
        public int VideoBitrate { get; set; }

        /// <summary>
        /// Gets or sets the target audio bitrate in bits per second (0 for automatic)
        /// </summary>
        public int AudioBitrate { get; set; }

        /// <summary>
        /// Gets or sets the target video quality factor (0-100, higher is better quality, 0 for using bitrate)
        /// </summary>
        public int VideoQuality { get; set; } = 80;

        /// <summary>
        /// Gets or sets a value indicating whether to preserve the original aspect ratio when resizing
        /// </summary>
        public bool MaintainAspectRatio { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of passes for encoding (1 or 2, 2 is higher quality but slower)
        /// </summary>
        public int EncodingPasses { get; set; } = 1;

        /// <summary>
        /// Gets or sets the preset speed for the encoder (e.g., "ultrafast", "medium", "slow")
        /// </summary>
        public string EncoderPreset { get; set; } = "medium";

        /// <summary>
        /// Gets or sets additional codec-specific parameters as a string
        /// </summary>
        public string? AdditionalParameters { get; set; }

        /// <summary>
        /// Creates a new instance of TranscodeOptions with default values for common HD video
        /// </summary>
        /// <returns>A new TranscodeOptions instance configured for HD video</returns>
        public static TranscodeOptions CreateHdPreset()
        {
            return new TranscodeOptions
            {
                Container = "MP4",
                VideoCodec = "H.264",
                AudioCodec = "AAC",
                Width = 1920,
                Height = 1080,
                FrameRate = 30,
                VideoBitrate = 5_000_000, // 5 Mbps
                AudioBitrate = 192_000,   // 192 kbps
                VideoQuality = 0,         // Use bitrate
                EncoderPreset = "medium"
            };
        }

        /// <summary>
        /// Creates a new instance of TranscodeOptions with default values for 4K video
        /// </summary>
        /// <returns>A new TranscodeOptions instance configured for 4K video</returns>
        public static TranscodeOptions Create4kPreset()
        {
            return new TranscodeOptions
            {
                Container = "MP4",
                VideoCodec = "H.265", // HEVC for better compression at 4K
                AudioCodec = "AAC",
                Width = 3840,
                Height = 2160,
                FrameRate = 30,
                VideoBitrate = 15_000_000, // 15 Mbps
                AudioBitrate = 320_000,    // 320 kbps
                VideoQuality = 0,          // Use bitrate
                EncoderPreset = "slow"     // Higher quality for 4K
            };
        }

        /// <summary>
        /// Creates a new instance of TranscodeOptions with default values for web video
        /// </summary>
        /// <returns>A new TranscodeOptions instance configured for web video</returns>
        public static TranscodeOptions CreateWebPreset()
        {
            return new TranscodeOptions
            {
                Container = "MP4",
                VideoCodec = "H.264",
                AudioCodec = "AAC",
                Width = 1280,
                Height = 720,
                FrameRate = 30,
                VideoBitrate = 2_500_000, // 2.5 Mbps
                AudioBitrate = 128_000,   // 128 kbps
                VideoQuality = 0,         // Use bitrate
                EncoderPreset = "fast"    // Faster encoding for web
            };
        }
    }
}
