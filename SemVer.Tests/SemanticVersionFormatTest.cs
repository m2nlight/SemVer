using System;
using Xunit;

namespace SemVer.Tests
{
    public class SemanticVersionFormatTest
    {
        [Fact]
        public void GetFormat_TypeIsICustomFormatter_ReturnsSemanticVersionFormat()
        {
            var format = SemanticVersionFormat.Default.GetFormat(typeof(ICustomFormatter));

            Assert.IsType<SemanticVersionFormat>(format);
        }

        [Fact]
        public void GetFormat_TypeIsInt_ReturnsNull()
        {
            var format = SemanticVersionFormat.Default.GetFormat(typeof(int));

            Assert.Null(format);
        }

        [Fact]
        public void Format_WithFormat_n_ThrowsFormatException()
        {
            var testCode = new Action(() =>
                SemanticVersionFormat.Default.Format("n", new SemanticVersion(1, 0, 0), null));

            Assert.Throws<FormatException>(testCode);
        }

        [Fact]
        public void Format_WithArg_Null_ThrowsFormatException()
        {
            var testCode = new Action(() =>
                SemanticVersionFormat.Default.Format("", null, null));

            Assert.Throws<FormatException>(testCode);
        }
    }
}
