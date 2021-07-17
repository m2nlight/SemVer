using System;
using Xunit;

namespace SemVer.Tests
{
    public class SemanticVersionTest
    {
        [Fact]
        public void Parse_Str_ReturnsAllComponentsIsRight()
        {
            // Arrange
            var segVerStr = "1.2.3-alpha.1-1.0+exp.sha.5114f85-001";

            // Act
            var segVer = SemanticVersion.Parse(segVerStr);

            // Assert
            Assert.NotNull(segVer);
            Assert.Equal(1, segVer.Major);
            Assert.Equal(2, segVer.Minor);
            Assert.Equal(3, segVer.Patch);
            Assert.Equal("alpha.1-1.0", segVer.Prerelease);
            Assert.Equal("exp.sha.5114f85-001", segVer.Build);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("1.0")]
        [InlineData("1.0.0")]
        [InlineData("1.0.0-alpha")]
        [InlineData("1.0.0-alpha.1")]
        [InlineData("1.0.0-0.3.7")]
        [InlineData("1.0.0-x.7.z.92")]
        [InlineData("1.0.0+exp.sha.5114f85")]
        [InlineData("1.0.0-alpha+exp.sha.5114f85")]
        [InlineData("1.0.0-alpha-0.0.0+exp.sha.5114f85")]
        [InlineData("1.0.0-alpha-0.0.0+exp.sha.5114f85-001")]
        [InlineData("1.0.0+001")]
        public void Parse_RightString_ReturnsNotNull(string rightStr)
        {
            var segVer = SemanticVersion.Parse(rightStr);

            Assert.NotNull(segVer);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("1.")]
        [InlineData("1.-0")]
        [InlineData("1.0.0-")]
        [InlineData("1.0.0-alpha.01")]
        [InlineData("1.0.0-alpha..1")]
        [InlineData("1.0.0-alpha%")]
        [InlineData("1.0.0-alpha+")]
        [InlineData("1.0.0+")]
        [InlineData("1.0.0+exp%")]
        [InlineData("1.0.0+exp.sha.5114f85+")]
        [InlineData("1.0.0.0")]
        public void Parse_WrongString_ReturnsNull(string wrongStr)
        {
            var segVer = SemanticVersion.Parse(wrongStr);

            Assert.Null(segVer);
        }

        [Theory]
        [InlineData(-1, 0, 0, "", "")]
        [InlineData(0, -1, 0, "", "")]
        [InlineData(0, 0, -1, "", "")]
        [InlineData(1, 0, 0, "alpha..1", "")]
        [InlineData(1, 0, 0, "alpha%", "")]
        [InlineData(1, 0, 0, null, "")]
        [InlineData(1, 0, 0, "", "exp%")]
        [InlineData(1, 0, 0, "", "exp..1")]
        [InlineData(1, 0, 0, "", null)]
        public void New_WrongArgs_ThrowException(int major, int minor, int patch, string prerelease, string build)
        {
            var testCode = new Action(() => new SemanticVersion(major, minor, patch, prerelease, build));
            Assert.ThrowsAny<Exception>(testCode);
        }

        [Theory]
        [InlineData("1.0", "1.0")]
        [InlineData("1.0", "1.0.0")]
        [InlineData("1.0-rc", "1.0.0-rc")]
        [InlineData("1.0-rc", "1.0.0-RC")]
        [InlineData("1.0+exp.sha.5114f85", "1.0.0+001")]
        public void Equal_ReturnsTrue(string left, string right)
        {
            var leftVer = SemanticVersion.Parse(left);
            var rightVer = SemanticVersion.Parse(right);

            Assert.Equal(leftVer, rightVer);
            Assert.True(leftVer == rightVer);
            Assert.True(((object)leftVer).Equals(rightVer));
            Assert.True(leftVer.CompareTo(rightVer) == 0);
        }

        [Theory]
        [InlineData("1.0", "1.0")]
        [InlineData("1.0", "1.0.0")]
        [InlineData("1.0-rc", "1.0.0-rc")]
        [InlineData("1.0-rc", "1.0.0-RC")]
        [InlineData("1.0+exp.sha.5114f85", "1.0.0+001")]
        public void GetHashCode_ReturnsIsEquals(string left, string right)
        {
            var leftVer = SemanticVersion.Parse(left);
            var rightVer = SemanticVersion.Parse(right);

            Assert.Equal(leftVer.GetHashCode(), rightVer.GetHashCode());
        }

        [Fact]
        public void NotEqual_ReturnsTrue()
        {
            var leftVer = new SemanticVersion(1, 0, 0);
            var rightVer = new SemanticVersion(1, 0, 0, "alpha");

            Assert.NotEqual(leftVer, rightVer);
            Assert.True(leftVer != rightVer);
        }

        [Fact]
        public void Equal_NullIsNull_ReturnsTrue()
        {
            SemanticVersion leftVer = null;
            SemanticVersion rightVer = null;

            Assert.Null(leftVer);
            Assert.True(leftVer == null);
            Assert.True(null == leftVer);
            Assert.True(leftVer == rightVer);
            Assert.True(rightVer == leftVer);
        }

        [Fact]
        public void Equal_NullIsNotNull_ReturnsTrue()
        {
            var leftVer = new SemanticVersion(1, 0, 0);
            var rightVer = (SemanticVersion)null;

            Assert.NotNull(leftVer);
            Assert.True(leftVer != null);
            Assert.True(null != leftVer);
            Assert.True(leftVer != rightVer);
            Assert.True(rightVer != leftVer);
        }

        [Theory]
        [InlineData("2.0.0", "10.0.0")]
        [InlineData("2.2.0", "2.10.0")]
        [InlineData("2.2.2", "2.2.10")]
        [InlineData("1.0.0", "1.0.1")]
        [InlineData("1.0.0-alpha", "1.0.0-alpha.1")]
        [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta")]
        [InlineData("1.0.0-alpha.beta", "1.0.0-beta")]
        [InlineData("1.0.0-beta", "1.0.0-beta.2")]
        [InlineData("1.0.0-beta.2", "1.0.0-beta.11")]
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1")]
        [InlineData("1.0.0-rc.1", "1.0.0")]
        [InlineData("1.0.0-rc.1.0", "1.0.0-rc.1.1")]
        [InlineData("1.0.0-alpha.2", "1.0.0-alpha.10")]
        [InlineData("1.0.0-alpha.2", "1.0.0-alpha.beta")]
        public void Compare_Less_ReturnsTrue(string left, string right)
        {
            var leftVer = SemanticVersion.Parse(left);
            var rightVer = SemanticVersion.Parse(right);

            Assert.True(leftVer < rightVer);
        }

        [Fact]
        public void Compare_LessOrEqual_ReturnsFalse()
        {
            var leftVer = SemanticVersion.Parse("1.0.0-rc.1.a");
            var rightVer = SemanticVersion.Parse("1.0.0-rc.1.1");

            Assert.False(leftVer <= rightVer);
        }

        [Fact]
        public void Compare_LargeOrEqual_ReturnsTrue()
        {
            var leftVer = SemanticVersion.Parse("1.0.0-rc.1.a");
            var rightVer = SemanticVersion.Parse("1.0.0-rc.1.1");

            Assert.True(leftVer >= rightVer);
            Assert.True(leftVer > rightVer);
        }

        [Fact]
        public void CompareTo_OtherType_ThrowsArgumentException()
        {
            var testCode = new Action(() => new SemanticVersion(1, 0, 0).CompareTo(new object()));

            Assert.Throws<ArgumentException>(testCode);
        }

        [Fact]
        public void CompareTo_NullOrReference_ReturnsTrue()
        {
            var semVer = new SemanticVersion(1, 0, 0);

            var result1 = semVer.CompareTo(null);
            var result2 = semVer.CompareTo(semVer);

            var result3 = semVer.CompareTo((object)null);
            var result4 = semVer.CompareTo((object)semVer);

            Assert.True(result1 == 1);
            Assert.True(result2 == 0);
            Assert.True(result3 == 1);
            Assert.True(result4 == 0);
        }

        [Theory]
        [InlineData("0.1.0")]
        [InlineData("1.0.0-alpha")]
        public void IsStable_MajorLess0_or_hasPreRelease_ReturnsFalse(string semVerStr)
        {
            var semVer = SemanticVersion.Parse(semVerStr);

            Assert.False(semVer.IsStable);
        }

        [Theory]
        [InlineData("1.0.0")]
        [InlineData("1.0.0+exp.sha.5114f85")]
        public void IsStable_MajorLarge0_and_noPreRelease_ReturnsTrue(string semVerStr)
        {
            var semVer = SemanticVersion.Parse(semVerStr);

            Assert.True(semVer.IsStable);
        }

        [Theory]
        [InlineData("1", "1.0.0")]
        [InlineData("1.0", "1.0.0")]
        [InlineData("1.0.0", "1.0.0")]
        [InlineData("1.0-alpha", "1.0.0-alpha")]
        [InlineData("1.0+exp.sha.5114f85", "1.0.0+exp.sha.5114f85")]
        [InlineData("1.0-alpha+exp.sha.5114f85", "1.0.0-alpha+exp.sha.5114f85")]
        public void ToString_ReturnsIsEqualExpected(string semVerStr, string expected)
        {
            var semVer = SemanticVersion.Parse(semVerStr);

            Assert.Equal(expected, semVer.ToString());
        }

        [Theory]
        [InlineData("1.0.0-alpha", "1.0.0", "N")]
        [InlineData("1.0", "1.0.0", "N")]
        [InlineData("1.0", "1.0.0", "")]
        [InlineData("1.0", "1.0.0", null)]
        public void ToString_WithFormat_ReturnsIsEqualExpected(string semVerStr, string expected, string format)
        {
            var semVer = SemanticVersion.Parse(semVerStr);
            var format1 = $"{{0:{format}}}";

            Assert.Equal(expected, semVer.ToString(format));
            Assert.Equal(expected, string.Format(format1, semVer));
        }

        [Fact]
        public void ToString_ByStringFormat_unknown_ThrowsFormatException()
        {
            var semVer = new SemanticVersion(1, 0, 0, "alpha");

            var testCode1 = new Action(() => semVer.ToString("n"));
            var testCode2 = new Action(() => string.Format("{0:n}", semVer));

            Assert.Throws<FormatException>(testCode1);
            Assert.Throws<FormatException>(testCode2);
        }
    }
}