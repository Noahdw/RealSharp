using System;
using System.Collections.Generic;
using System.Text;

namespace RealSharp
{
    class SyntaxHighlighter
    {
        public Dictionary<string,string> SyntaxMap = new Dictionary<string, string>();
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
            SyntaxMap.Add("default", "#F2F2F2");
            SyntaxMap.Add("type", "#33D4FF");
            SyntaxMap.Add("modifier", "#AFBBBF");
            SyntaxMap.Add("statement", "#CC58C3");
            SyntaxMap.Add("comment", "#88E57B");
        }

        private void InitTypeSet()
        {
            TypeSet.Add("int");
            TypeSet.Add("void");
            TypeSet.Add("bool");
            TypeSet.Add("double");
            TypeSet.Add("float");
            TypeSet.Add("var");
            TypeSet.Add("string");
            TypeSet.Add("class");
            TypeSet.Add("namespace");
            TypeSet.Add("static");
            TypeSet.Add("true");
            TypeSet.Add("false");
        }

        private void InitModifierSet()
        {
            ModifierSet.Add("public");
            ModifierSet.Add("private");
            ModifierSet.Add("internal");
        }

        private void InitStatementSet()
        {
            StatementSet.Add("for");
            StatementSet.Add("if");
            StatementSet.Add("while");
            StatementSet.Add("switch");
        }
    }
}
