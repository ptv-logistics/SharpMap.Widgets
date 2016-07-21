//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Ptv.XServer.Demo.Clustering
{
    /// <summary> Class to store one CSV row. </summary>
    public class CsvRow : List<string>
    {
        /// <summary> Gets or sets the text of a single line in the csv file. </summary>
        public string LineText { get; set; }
    }

    /// <summary> Class to read data from a CSV file. </summary>
    public class CsvFileReader : StreamReader
    {
        /// <summary> Character up to which a line should be read. </summary>
        private readonly char delimiter;

        /// <summary> Initializes a new instance of the <see cref="CsvFileReader"/> class and sets the file to read as
        /// well as the line delimiter. </summary>
        /// <param name="stream"> The file to read as a stream. </param>
        /// <param name="delimiter"> Character up to which a line should be read. </param>
        public CsvFileReader(Stream stream, char delimiter)
            : base(stream)
        {
            this.delimiter = delimiter;
        }

        /// <summary> Initializes a new instance of the <see cref="CsvFileReader"/> class and sets the file name as
        /// well as the line delimiter. </summary>
        /// <param name="filename"> Name of the file to read. </param>
        /// <param name="delimiter"> Character up to which a line of the file should be read. </param>
        public CsvFileReader(string filename, char delimiter)
            : base(filename)
        {
            this.delimiter = delimiter;
        }

        /// <summary> Reads a row of data from a CSV file. </summary>
        /// <param name="row"> A line of text. </param>
        /// <returns> A value indicating whether the row has been successfully read. </returns>
        public bool ReadRow(CsvRow row)
        {
            row.LineText = ReadLine();
            if (String.IsNullOrEmpty(row.LineText))
                return false;

            int pos = 0;
            int rows = 0;

            while (pos < row.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (row.LineText[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < row.LineText.Length)
                    {
                        // Test for quote character
                        if (row.LineText[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = row.LineText.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < row.LineText.Length && row.LineText[pos] != delimiter)
                        pos++;
                    value = row.LineText.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < row.LineText.Length && row.LineText[pos] != delimiter)
                    pos++;
                if (pos < row.LineText.Length)
                    pos++;
            }
            // Delete any unused items
            while (row.Count > rows)
                row.RemoveAt(rows);

            // Return true if any columns read
            return (row.Count > 0);
        }
    }
}
