using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrameworkSupport
{
    public class CsvConfig
    {
        public char Delimiter { get; private set; }
        public string NewLineMark { get; private set; }
        public char QuotationMark { get; private set; }

        public CsvConfig(char delimiter, string newLineMark, char quotationMark)
        {
            Delimiter = delimiter;
            NewLineMark = newLineMark;
            QuotationMark = quotationMark;
        }

        // useful configs

        public static CsvConfig Default
        {
            get { return new CsvConfig(',', "\r\n", '\"'); }
        }

        // etc.
    }

    public class CsvReader
    {
        private CsvConfig m_config;

        public CsvReader(CsvConfig config = null)
        {
            m_config = config == null ? CsvConfig.Default : config;
        }

        public IEnumerable<string[]> Read(string csvFileContents)
        {
            using (StringReader reader = new StringReader(csvFileContents))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    yield return ParseLine(line);
                }
            }
        }

        public string[] ParseLine(string line)
        {
            Stack<string> result = new Stack<string>();

            int i = 0;
            while (true)
            {
                string cell = ParseNextCell(line, ref i);
                if (cell == null)
                {
                    break;
                }

                result.Push(cell);
            }

            // remove last elements if they're empty
            while (string.IsNullOrEmpty(result.Peek()))
            {
                result.Pop();
            }

            var resultAsArray = result.ToArray();
            Array.Reverse(resultAsArray);
            return resultAsArray;
        }

        // returns iterator after delimiter or after end of string
        private string ParseNextCell(string line, ref int i)
        {
            if (i >= line.Length)
            {
                return null;
            }

            return line[i] != m_config.QuotationMark ? ParseNotEscapedCell(line, ref i) : ParseEscapedCell(line, ref i);
        }

        // returns iterator after delimiter or after end of string
        private string ParseNotEscapedCell(string line, ref int i)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (i >= line.Length) // return iterator after end of string
                {
                    break;
                }

                if (line[i] == m_config.Delimiter)
                {
                    i++; // return iterator after delimiter
                    break;
                }
                sb.Append(line[i]);
                i++;
            }
            return sb.ToString();
        }

        // returns iterator after delimiter or after end of string
        private string ParseEscapedCell(string line, ref int i)
        {
            i++; // omit first character (quotation mark)
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (i >= line.Length)
                {
                    break;
                }

                if (line[i] == m_config.QuotationMark)
                {
                    i++; // we're more interested in the next character
                    if (i >= line.Length)
                    {
                        // quotation mark was closing cell;
                        // return iterator after end of string
                        break;
                    }
                    if (line[i] == m_config.Delimiter)
                    {
                        // quotation mark was closing cell;
                        // return iterator after delimiter
                        i++;
                        break;
                    }
                    if (line[i] == m_config.QuotationMark)
                    {
                        // it was doubled (escaped) quotation mark;
                        // do nothing -- we've already skipped first quotation mark
                    }
                }
                sb.Append(line[i]);
                i++;
            }

            return sb.ToString();
        }
    }
}