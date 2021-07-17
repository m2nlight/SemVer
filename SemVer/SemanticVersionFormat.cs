using System;

namespace SemVer
{
    /// <summary>
    /// SemanticVersion 格式化器
    /// </summary>
    public sealed class SemanticVersionFormat : IFormatProvider, ICustomFormatter
    {
        private static readonly Lazy<SemanticVersionFormat> SDefault =
            new(() => new SemanticVersionFormat());

        /// <summary>
        /// 获得默认格式化器
        /// </summary>
        public static SemanticVersionFormat Default => SDefault.Value;

        /// <summary>
        /// 格式化 SemanticVersion
        /// </summary>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is SemanticVersion semVer)
            {
                if (string.IsNullOrEmpty(format))
                    return semVer.ToString();

                if ("N".Equals(format, StringComparison.Ordinal))
                    return $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}";

                throw new FormatException($"{nameof(format)} is not support format: {format}");
            }

            throw new FormatException($"{nameof(arg)} must is a SemanticVersion");
        }

        /// <summary>
        /// 获得格式化器对象
        /// </summary>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}