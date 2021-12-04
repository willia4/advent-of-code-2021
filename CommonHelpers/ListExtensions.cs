using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHelpers
{
    public static class ListExtensions
    {
        public static List<List<T>> Transpose<T>(this List<List<T>> rows)
        {
            if (!rows.Any()) { return new List<List<T>>(); }

            var rowLength = rows.First().Count;
            var colLength = rows.Count;

            // transpose all of the rows into columns, so we'll return a list of what was the columns
            var columns = new List<List<T>>(colLength);
            
            // build up the result columns list. At first, each column is just an empty list 
            for (var i = 0; i < colLength; i++)
            {
                if (rows[i].Count != rowLength)
                {
                    throw new InvalidOperationException("Transpose can only operate on non-jagged matrices");
                }
                
                columns.Add(new List<T>());
            }

            foreach (var row in rows)
            {
                for (int originalColIndex = 0; originalColIndex < rowLength; originalColIndex++)
                {
                    columns[originalColIndex].Add(row[originalColIndex]);
                }
            }

            return columns;
        }
    }
}