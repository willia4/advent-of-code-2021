using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonHelpers
{
    public static class IEnumerableExtensions
    {
        public static UInt64 Sum(this IEnumerable<UInt64> source)
        {
            return source.Aggregate((UInt64)0, (acc, v) => acc + v);
        }

        
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
            return matrix.Select((row) => row.Select(transform));
        }
        
        public static IEnumerable<IEnumerable<U>> SelectMatrix<T, U>(this IEnumerable<IEnumerable<T>> matrix, Func<int, int, T, U> transform)
        {
            return matrix.Select((row, y) => row.Select((value, x) => transform(x, y, value)));
        }

        public static string MatrixToString<T>(this IEnumerable<IEnumerable<T>> matrix, string spacer = "  ", Func<T, string> customToString = null)
        {
            Func<int, int, T, string> transformer = (x, y, v) =>
            {
                if (customToString == null)
                {
                    return v?.ToString() ?? "<null>";
                }

                return customToString(v);
            };

            return matrix.MatrixToString(spacer, transformer);
        }
        
        public static string MatrixToString<T>(this IEnumerable<IEnumerable<T>> matrix, string spacer, Func<int, int, T, string> customToString)
        {
            var stringMatrix = matrix.SelectMatrix((x, y, v) => customToString != null 
                                                                                ? customToString(x, y, v)
                                                                                : v?.ToString() ?? "<null>");
            var stringLengths = stringMatrix.SelectMatrix(s => s.Length);

            var maxLength = stringLengths.Select(row => row.Max()).Max();

            var sb = new System.Text.StringBuilder();

            foreach (var row in stringMatrix)
            {
                string space = "";
                foreach (var item in row)
                {
                    sb.Append(space);
                    space = spacer;

                    sb.Append(item.PadLeft(maxLength));
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static IEnumerable<IEnumerable<T>> ChunkBySeparator<T>(this IEnumerable<T> items, Func<T, bool> isSep)
        {
            isSep ??= (item => false);
            var currentChunk = Enumerable.Empty<T>();

            foreach (var item in items)
            {
                if (isSep(item))
                {
                    if (currentChunk.Any())
                    {
                        yield return currentChunk;
                        currentChunk = Enumerable.Empty<T>();
                    }
                }
                else
                {
                    currentChunk = currentChunk.Append(item);
                }
            }

            if (currentChunk.Any())
            {
                yield return currentChunk;
            }
        }

        public static IEnumerable<IEnumerable<T>> AsSlidingWindow<T>(this IEnumerable<T> items, int chunkSize)
        {
            var chunk = new List<T>();
            
            foreach (var item in items)
            {
                if (chunk.Count == chunkSize)
                {
                    yield return chunk.ToImmutableArray();
                    chunk.RemoveAt(0);
                }

                chunk.Add(item);
            }

            if (chunk.Count == chunkSize)
            {
                yield return chunk.ToImmutableArray();
            }
        }
        
        public static IList<IList<T>> ToImmutableMatrix<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return ImmutableArray<IList<T>>.Empty.AddRange(
                source.Select(row => ImmutableArray<T>.Empty.AddRange(row) as IList<T>
            ));
        }
    }
}