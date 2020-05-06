using System;
using System.Collections.Generic;
using System.Text;

namespace RealSharp
{
    class SyntaxHighlighter
    {
        public Dictionary<string, string> SyntaxMap = new Dictionary<string, string>();
        public HashSet<string> TypeSet = new HashSet<string>();
        public HashSet<string> ModifierSet = new HashSet<string>();
        public HashSet<string> StatementSet = new HashSet<string>();

        public SyntaxHighlighter()
        {
            InitStatementSet();
            InitSyntaxMap();
            InitTypeSet();
            InitModifierSet();

        }

        private void InitSyntaxMap()
        {
            SyntaxMap.Add("stringORchar", "#FF8CF7");
            SyntaxMap.Add("statement", "#CC58C3");
            SyntaxMap.Add("modifier", "#AFBBBF");
            SyntaxMap.Add("comment", "#88E57B");
            SyntaxMap.Add("default", "#F2F2F2");
            SyntaxMap.Add("type", "#33D4FF");
        }

        private void InitTypeSet()
        {
            /// <summary>
            /// System data Types
            /// </summary>
            TypeSet.Add("double");
            TypeSet.Add("string");
            TypeSet.Add("float");
            TypeSet.Add("bool");
            TypeSet.Add("int");
            TypeSet.Add("var");
            //--------
            /// <summary>
            /// Custom data types
            /// </summary>
            TypeSet.Add("namespace");
            TypeSet.Add("interface");
            TypeSet.Add("struct");
            TypeSet.Add("class");
            TypeSet.Add("enum");
            //--------
            /// <summary>
            /// System Data Values
            /// </summary>
            TypeSet.Add("false");
            TypeSet.Add("true");
            TypeSet.Add("get");
            TypeSet.Add("set");
            //--------
            /// <summary>
            /// Ganeral Data Type
            /// </summary>
            TypeSet.Add("void");
        }

        private void InitModifierSet()
        {
            ModifierSet.Add("abstract");
            ModifierSet.Add("internal");
            ModifierSet.Add("override");
            ModifierSet.Add("readonly");
            ModifierSet.Add("private");
            ModifierSet.Add("public");
            ModifierSet.Add("static");
            ModifierSet.Add("const");

        }

        private void InitStatementSet()
        {
            StatementSet.Add("finally");
            StatementSet.Add("using");
            StatementSet.Add("switch");
            StatementSet.Add("catch");
            StatementSet.Add("while");
            StatementSet.Add("try");
            StatementSet.Add("for");
            StatementSet.Add("if");
            StatementSet.Add("do");
        }
    }
}
