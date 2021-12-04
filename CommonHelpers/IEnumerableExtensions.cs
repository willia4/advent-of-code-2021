using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHelpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.ToLists().Transpose();
        }

        public static List<List<T>> ToLists<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.Select(s => s.ToList()).ToList();
        }

        public static IEnumerable<IEnumerable<U>> SelectMatrix<T, U>(this IEnumerable<IEnumerable<T>> matrix, Func<T, U> transform)
        {
            return matrix.Select(row => row.Select(transform));
        }

        public static string MatrixToString<T>(this IEnumerable<IEnumerable<T>> matrix)
        {
            var stringMatrix = matrix.SelectMatrix((o) => o?.ToString() ?? "<null>");
            var stringLengths = stringMatrix.SelectMatrix(s => s.Length);

            var maxLength = stringLengths.Select(row => row.Max()).Max();

            var sb = new System.Text.StringBuilder();

            foreach (var row in stringMatrix)
            {
                string space = "";
                foreach (var item in row)
                {
                    sb.Append(space);
                    space = "  ";

                    sb.Append(item.PadLeft(maxLength));
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}