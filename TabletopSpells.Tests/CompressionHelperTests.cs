using TabletopSpells.Helpers;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class CompressionHelperTests
{
    [Fact]
    public void Compress_And_Decompress_RoundTrip_Works()
    {
        var original = "This is a test string for compression!";
        var compressed = CompressionHelper.CompressString(original);
        var decompressed = CompressionHelper.DecompressString(compressed);
        decompressed.Should().Be(original);
    }
}

