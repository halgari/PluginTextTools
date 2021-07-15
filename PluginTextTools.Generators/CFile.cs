using System.IO;
using System.Text;

namespace PluginTextTools.Generators
{
    public class CFile
    {
        private int _indentLevel;
        private readonly StringBuilder _sb;

        public CFile()
        {
            _indentLevel = 0;
            _sb = new StringBuilder();

        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public void WriteFile(string filename)
        {
            File.WriteAllText(filename, _sb.ToString());
        }

        public void Code(string s)
        {
            for (var idx = 0; idx < _indentLevel; idx += 1)
            {
                _sb.Append("  ");
            }
            if (s == "}")
                _indentLevel -= 1;
            if (s.EndsWith("{"))
                _indentLevel += 1;
            _sb.Append(s);
            _sb.Append('\n');
        }
    }
}