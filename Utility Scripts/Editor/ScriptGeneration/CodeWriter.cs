using System;
using System.IO;
using UnityEngine;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public sealed class CodeWriter : IDisposable
    {
        private readonly StreamWriter writer;
        private int indentLevel;
        private const string INDENT = "    "; // 4 spaces
        public CodeWriter(string path)
        {
            writer = new StreamWriter(path);
        }
        public void Indent() => indentLevel++;
        public void Unindent() => indentLevel = Mathf.Max(0, indentLevel - 1);
        public void WriteLine(string line = "")
        {
            if (line.Length > 0)
            {
                for (int i = 0; i < indentLevel; i++)
                    writer.Write(INDENT);
            }
            writer.WriteLine(line);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}