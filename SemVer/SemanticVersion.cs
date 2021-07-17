using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SemVer
{
    /// <summary>
    /// 语义化版本 2.0.0 的一个实现
    /// <para><see href="https://semver.org" /></para>
    /// </summary>
    public class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>, IComparable, IFormattable
    {
        private static readonly Regex SemVerRegex =
            new(
                @"^(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(\-(?<pre>[0-9A-Za-z\-\.]+))?(\+(?<build>[0-9A-Za-z\-\.]+))?$"
                , RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        private static readonly Regex PreOrBuildRegex =
            new(@"^[0-9A-Za-z\-\.]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private string _build = "";

        private string _prerelease = "";
        internal string[] PreIdentifiers;

        /// <summary>
        /// 创建一个 Semantic Versioning 对象
        /// </summary>
        /// <param name="major">主版本号</param>
        /// <param name="minor">次版本号</param>
        /// <param name="patch">修订号</param>
        /// <param name="prerelease">先行版本号</param>
        /// <param name="build">版本编译信息</param>
        public SemanticVersion(int major, int minor, int patch, string prerelease = "", string build = "")
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor));
            if (patch < 0)
                throw new ArgumentOutOfRangeException(nameof(patch));

            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = prerelease;
            Build = build;
        }

        /// <summary>
        /// 主版本号
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// 次版本号
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// 修订号
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// 先行版本号
        /// </summary>
        public string Prerelease
        {
            get => _prerelease;
            set
            {
                var identifiers = value.Split('.');
                if (!IsValidPrerelease(value, identifiers))
                    throw new ArgumentException(nameof(Prerelease));

                _prerelease = value;
                PreIdentifiers = identifiers;
            }
        }

        /// <summary>
        /// 版本编译信息
        /// </summary>
        public string Build
        {
            get => _build;
            set
            {
                if (!IsValidBuild(value))
                    throw new ArgumentException(nameof(Build));

                _build = value;
            }
        }

        /// <summary>
        /// 是否为稳定版本
        /// </summary>
        public bool IsStable => Major > 0 && Prerelease.Length == 0;

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="obj">一个 SemanticVersion 对象</param>
        /// <returns>比较结果</returns>
        /// <exception cref="ArgumentException"></exception>
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;
            if (ReferenceEquals(this, obj))
                return 0;
            return obj is SemanticVersion other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(SemanticVersion)}");
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public int CompareTo(SemanticVersion other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (ReferenceEquals(null, other))
                return 1;

            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0)
                return majorComparison;

            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0)
                return minorComparison;

            var patchComparison = Patch.CompareTo(other.Patch);
            if (patchComparison != 0)
                return patchComparison;

            return ComparePrerelease(this, other);
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public bool Equals(SemanticVersion other)
        {
            return (object)other != null &&
                   Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   ComparePrerelease(this, other) == 0;
        }

        /// <summary>
        /// 按格式转换为字符串
        /// </summary>
        /// <param name="format">支持的格式</param>
        /// <param name="formatProvider">格式化器，默认是 SemanticVersionFormatProvider</param>
        /// <returns>字符串</returns>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            return SemanticVersionFormat.Default.Format(format, this, formatProvider ?? SemanticVersionFormat.Default);
        }

        /// <summary>
        /// 比较 prerelease
        /// </summary>
        private static int ComparePrerelease(SemanticVersion semVer, SemanticVersion otherSemVer)
        {
            if (semVer.Prerelease == otherSemVer.Prerelease)
                return 0;

            if (semVer.Prerelease.Length == 0)
                return 1;

            if (otherSemVer.Prerelease.Length == 0)
                return -1;

            var minLen = Math.Min(semVer.PreIdentifiers.Length, otherSemVer.PreIdentifiers.Length);
            for (var i = 0; i < minLen; i++)
            {
                var seg = semVer.PreIdentifiers[i];
                var otherSeg = otherSemVer.PreIdentifiers[i];
                var segIsNum = IsNumericString(seg, out var segValue);
                var otherSegIsNum = IsNumericString(otherSeg, out var otherSegValue);

                if (segIsNum && otherSegIsNum)
                {
                    // 数值比较
                    var valueComparison = segValue.CompareTo(otherSegValue);
                    if (valueComparison == 0)
                        continue;

                    return valueComparison;
                }

                // 数值比非数值低
                if (segIsNum)
                    return -1;

                if (otherSegIsNum)
                    return 1;

                // 非数值比较（忽略大小写）
                var strComparison = string.Compare(seg, otherSeg, StringComparison.OrdinalIgnoreCase);
                if (strComparison != 0)
                    return strComparison;
            }

            // segment 多的比少的大
            return semVer.Prerelease.Length.CompareTo(otherSemVer.Prerelease.Length);
        }

        /// <summary>
        /// 验证 prerelease 是否有效
        /// </summary>
        private static bool IsValidPrerelease([NotNull] string prerelease, [NotNull] string[] identifiers)
        {
            if (prerelease.Length == 0)
                return true;

            if (prerelease.Contains("..", StringComparison.Ordinal))
                return false; // 不允许空的标识符，也就是有连续的英文名点符号

            if (!PreOrBuildRegex.IsMatch(prerelease))
                return false;

            foreach (var segment in identifiers)
            {
                if (IsNumericString(segment, out var value) &&
                    (value != 0 && segment.StartsWith('0') ||
                     value == 0 && segment.Length != 1))
                    return false; // 要求数字时，不能有0前缀
            }

            return true;
        }

        /// <summary>
        /// 验证 build 是否有效
        /// </summary>
        private static bool IsValidBuild(string build)
        {
            if (build == null)
                return false;

            if (build == "")
                return true;

            if (build.Contains("..", StringComparison.Ordinal))
                return false; // 不允许空的标识符，也就是有连续的英文名点符号

            if (!PreOrBuildRegex.IsMatch(build))
                return false;

            return true;
        }

        /// <summary>
        /// 是否为一个数值字符串（数值必须大于或等于0）
        /// </summary>
        /// <param name="segment">字符串</param>
        /// <param name="value">返回数值</param>
        /// <returns>是否为一个数值字符串</returns>
        private static bool IsNumericString(string segment, out int value)
        {
            return int.TryParse(segment, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value >= 0;
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as SemanticVersion);
        }

        /// <summary>
        /// 获得HashCode。根据定义，版本编译信息不会参与生成HashCode。
        /// </summary>
        public override int GetHashCode()
        {
            var hashCode = 168745411;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Patch.GetHashCode();
            hashCode = hashCode * -1521134295 + Prerelease.ToUpper().GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            if ((object)left == null && (object)right == null)
                return true;

            return (object)left != null && left.Equals(right);
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            return Comparer<SemanticVersion>.Default.Compare(left, right) < 0;
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return Comparer<SemanticVersion>.Default.Compare(left, right) > 0;
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            return Comparer<SemanticVersion>.Default.Compare(left, right) <= 0;
        }

        /// <summary>
        /// 比较 SemanticVersion。根据定义，版本编译信息不会被比较。
        /// </summary>
        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return Comparer<SemanticVersion>.Default.Compare(left, right) >= 0;
        }

        /// <summary>
        /// 将字符串解析为SemanticVersion
        /// </summary>
        /// <param name="segVerStr">字符串</param>
        /// <param name="value">返回SemanticVersion对象</param>
        /// <returns>是否成功</returns>
        public static bool TryParse(string segVerStr, out SemanticVersion value)
        {
            value = null;
            var match = SemVerRegex.Match(segVerStr);
            if (!match.Success)
                return false;

            var major = int.Parse(match.Groups["major"].Value, NumberStyles.None, CultureInfo.InvariantCulture);

            if (!int.TryParse(match.Groups["minor"].Value, NumberStyles.None, CultureInfo.InvariantCulture,
                out var minor))
                minor = 0;

            if (!int.TryParse(match.Groups["patch"].Value, NumberStyles.None, CultureInfo.InvariantCulture,
                out var patch))
                patch = 0;

            var prerelease = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;
            try
            {
                value = new SemanticVersion(major, minor, patch, prerelease, build);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 将字符串解析为SemanticVersion。失败返回null
        /// </summary>
        /// <param name="segVerStr">字符串</param>
        /// <returns>返回SemanticVersion对象或null</returns>
        public static SemanticVersion Parse(string segVerStr)
        {
            return TryParse(segVerStr, out var value) ? value : null;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            var noBuild = string.IsNullOrWhiteSpace(Build);
            var noPre = string.IsNullOrWhiteSpace(Prerelease);

            if (noBuild && noPre)
                return $"{Major}.{Minor}.{Patch}";

            if (noBuild)
                return $"{Major}.{Minor}.{Patch}-{Prerelease}";

            if (noPre)
                return $"{Major}.{Minor}.{Patch}+{Build}";

            return $"{Major}.{Minor}.{Patch}-{Prerelease}+{Build}";
        }
    }
}