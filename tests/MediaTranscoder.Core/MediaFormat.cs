namespace MediaTranscoder.Core.Models
{
    /// <summary>
    /// Contains metadata about a media file's format and encoding
    /// </summary>
    public class MediaFormat
    {
        /// <summary>
        /// Gets or sets the container format (e.g., MP4, MKV, MOV)
        /// </summary>
        public string Container { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the video codec (e.g., H.264, ProRes, VP9)
        /// </summary>
        public string VideoCodec { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the audio codec (e.g., AAC, MP3, PCM)
        /// </summary>
        public string AudioCodec { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the video width in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the video height in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the frame rate in frames per second
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of the video
        /// </summary>
        public int BitDepth { get; set; } = 8;

        /// <summary>
        /// Gets or sets the duration of the media in seconds
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the overall bitrate in bits per second
        /// </summary>
        public long Bitrate { get; set; }

        /// <summary>
        /// Gets the resolution as a formatted string (e.g., "1920x1080")
        /// </summary>
        public string Resolution => $"{Width}x{Height}";

        /// <summary>
        /// Returns a string representation of the media format
        /// </summary>
        public override string ToString()
        {
            return $"{Container} | Video: {VideoCodec} ({Resolution}@{FrameRate:F2}fps) | Audio: {AudioCodec}";
        }
    }
}
