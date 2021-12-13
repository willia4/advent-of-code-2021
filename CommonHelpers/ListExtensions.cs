using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonHelpers
{
    public static class ListExtensions
    {
        private static IEnumerable<T> GetMatrixColumn<T>(this List<List<T>> matrix, int colIndex)
        {
            foreach (var row in matrix)
            {
                yield return row[colIndex];
            }
        }
        
        public static List<List<T>> Transpose<T>(this List<List<T>> matrix)
        {
            if (!matrix.Any()) { return new List<List<T>>(); }

            var origMatrixColCount = matrix.First().Count;
            var origMatrixRowCount = matrix.Count;
            
            // transpose all of the rows into columns, so we'll return a list of what was the columns
            var newMatrix = new List<List<T>>(origMatrixColCount);

            for (var origColIndex = 0; origColIndex < origMatrixColCount; origColIndex++)
            {
                newMatrix.Add(new List<T>(origMatrixRowCount));

                for (var origRowIndex = 0; origRowIndex < origMatrixRowCount; origRowIndex++)
                {
                    if (matrix[origRowIndex].Count != origMatrixColCount)
                    {
                        throw new InvalidOperationException("Transpose can only operate on non-jagged matrices");
                    }

                    var origValue = matrix[origRowIndex][origColIndex];
                    newMatrix[origColIndex].Add(origValue);
                }
            }

            return newMatrix;
        }
    }
}