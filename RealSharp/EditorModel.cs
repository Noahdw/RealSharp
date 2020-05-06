using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace RealSharp
{
    internal class EditorModel
    {
        //Qml won't call constructor??
        public void Init()
        {
            _stringTokenList = new List<List<string>>();
            for (var i = 0; i < MinimumLines; i++)
            {
                _lineList.Add("");
                _stringTokenList.Add(new List<string>());
                LinesToDraw.Add(i);
                TextRedrawNeeded = true;
            }
        }
        private List<string> _lineList = new List<string>();
        
        private List<List<string>> _stringTokenList = new List<List<string>>()
        {
        new List<string>()
        };

        private Stack<Command> mCommandHistory = new Stack<Command>();

        public const int MinimumLines = 30;
        private SyntaxHighlighter _syntaxHighlighter = new SyntaxHighlighter();
        public List<string> CopyLines = new List<string>();
        public List<int> LinesToDraw { get; }= new List<int>();
        public List<int> TestTest { get; } = new List<int>(){1,2,3,4,5};

        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int PrefferedCursorX { get; set; }
        public int SpacesPerTab = 4;
        public bool TextRedrawNeeded { get; set; }


        public void IssueCommand(Command c)
        {
            mCommandHistory.Push(c);
            c.Execute();
        }

        public bool KeyEvent(string text, int keyCode)
        {
          // TestTest.
            TextRedrawNeeded = true;
            int cursorXAdjust = 1;
            if (CursorY < 0 || CursorY > LineCount())
            {
                return false;
            }
            if (text == "\t") //Tab
            {
                text = "";
                text = text.PadRight(SpacesPerTab);
                cursorXAdjust = SpacesPerTab;
            }
            if (text == null) // Likely a Fn key or arrow keys
            {
                HandleUnprintableCharacter(keyCode);
            }

            else if (text == "\r") //Enter key
            {
                InsertNewLine();
                TextRedrawNeeded = true;
            }
            else if (text == "\b") //Backspace
            {
                // RemoveCharacter();
                // TextRedrawNeeded = true;
                RemoveCharText();
                TextRedrawNeeded = true;
            }


            else if (char.IsLetterOrDigit(text[0]) || char.IsWhiteSpace(text[0]) || char.IsPunctuation(text[0]) || char.IsSymbol(text[0]))
            {
                TextRedrawNeeded = true;
                //TextRedrawNeeded = true;
                //LinesToDraw.Add(CursorY);
                //if (!SpecialCase(text))
                //{
                //    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                //    CursorX += cursorXAdjust;
                //}
                //else
                //{
                //    ParseLine(CursorY);
                //    return true;
                //}
               // AddCharText(text);
                IssueCommand(new AddTextCommand(text,this));
            }

            ParseLine(CursorY);
            return true;
        }

        public void MouseEventX(int xPos)
        {
            if (xPos <= _lineList[CursorY].Length)
            {
                CursorX = xPos;

            }
            else
            {
                CursorX = _lineList[CursorY].Length;
            }

            if (CursorX < 0)
            {
                CursorX = 0;
            }
            PrefferedCursorX = CursorX;

        }
        public void MouseEventY(int yPos)
        {
            if (yPos >= _lineList.Count)
            {
                CursorY = _lineList.Count - 1;

            }
            else
            {
                CursorY = yPos;
            }
            if (CursorY < 0)
            {
                CursorY= 0;
            }
        }

        public int LineCount()
        {
            return _lineList.Count;
        }

        public int TokensInLine(int line)
        {
            if (_stringTokenList.Count < line)
            {
                return 0;
            }
            return _stringTokenList[line].Count;
        }

        public int CharactersInLine(int line)
        {
            if (_stringTokenList.Count < line ||  line > _stringTokenList.Count)
            {
                return 0;
            }

            var tokens = TokensInLine(line);
            var chars = 0;
            for (int i = 0; i < tokens; i++)
            {
                chars += Text(line, i).Length;
            }

            return chars;
        }

        public string Text(int line, int token)
        {
            return _stringTokenList[line][token];
        }



        private void InsertNewLine()
        {
            string newLineText = "";

            newLineText = _lineList[CursorY].Substring(CursorX);


            if (CursorY + 1 == LineCount())
            {
                _lineList.Add(newLineText);
            }
            else
            {
                _lineList.Insert(CursorY + 1, newLineText);
            }

            if (CursorX != _lineList[CursorY].Length)
            {
                _lineList[CursorY] = _lineList[CursorY].Remove(CursorX);
            }

            CursorY++;
            //EnsureValidCursorX();
            CursorX = 0;
            _stringTokenList.Add(new List<string>());
            for (int i = 0; i < LineCount(); i++)
            {
                LinesToDraw.Add(i);
                ParseLine(i);
            }

        }

        private void RemoveCharacter()
        {
            var str = _lineList[CursorY];
            //If empty line remove the whole line
            if (str == null || str == "\r" || str == "")
            {
                _lineList.RemoveAt(CursorY);
                
                if (LineCount() < MinimumLines)
                {
                    _lineList.Add("");
                }

                if (CursorY != 0)
                    CursorY--;

                for (int i = 0; i < LineCount(); i++)
                {
                    LinesToDraw.Add(i);
                    ParseLine(i);
                }
                EnsureValidCursorX();
            }
            else //remove single chracter
            {
                if (CursorX == 0) // remove line and put remaining characters on line above it
                {
                    if (LineCount() == MinimumLines) return;
                    if (CursorY <= 0) return;
                    var text = _lineList[CursorY];
                    var newCursorX = _lineList[CursorY - 1].Length;
                    _lineList[CursorY - 1] += text;
                    _lineList.RemoveAt(CursorY);
                    if (CursorY <= 0) return;
                    CursorY--;
                    CursorX = newCursorX;
                    for (int i = 0; i < LineCount(); i++)
                    {
                        LinesToDraw.Add(i);
                        ParseLine(i);
                    }
                }
                // handle cursor in middle of () et al
                else if (CursorInMiddleOfBrace()) 
                {
                    LinesToDraw.Add(CursorY);
                }
                //Remove single character
                else
                {
                    LinesToDraw.Add(CursorY);
                    _lineList[CursorY] = str.Remove(CursorX - 1, 1);
                    CursorX--;
                }

            }
        }

        public void UndoCommand()
        {
            if (mCommandHistory.TryPop(out var c) )
            {
                c.undo();
            }
        }

        private void HandleUnprintableCharacter(int keycode)
        {
            switch (keycode)
            {
                case 0x01000012: // LEFT ARROW
                    if (CursorX > 0)
                    {
                        CursorX--;
                        PrefferedCursorX = CursorX;
                    }
                    else if (CursorY > 0)
                    {
                        CursorY--;
                        CursorX = PrefferedCursorX = _lineList[CursorY].Length;
                    }
                    break;
                case 0x01000013: // UP ARROW
                    if (CursorY > 0)
                    {
                        CursorY--;
                        if (_lineList[CursorY].Length < PrefferedCursorX)
                        {
                            EnsureValidCursorX();
                        }
                        else
                        {
                            CursorX = PrefferedCursorX;
                        }

                    }
                    break;
                case 0x01000014: // Right ARROW
                    if (CursorX < _lineList[CursorY].Length)
                    {
                        CursorX++;
                        PrefferedCursorX = CursorX;
                    }
                    else if (CursorY + 1 < LineCount())
                    {
                        CursorY++;
                        CursorX = PrefferedCursorX = 0;
                    }
                    break;
                case 0x01000015: // DOWN ARROW
                    if (CursorY + 1 < LineCount())
                    {
                        CursorY++;
                        if (_lineList[CursorY].Length < PrefferedCursorX)
                        {
                            EnsureValidCursorX();
                        }
                        else
                        {
                            CursorX = PrefferedCursorX;
                        }
                    }
                    break;
                default:
                    break;
            }
        }


        private enum States
        {
            NoState,
            UnknownWordState,
            VariableTypeState,
            FunctionDefinitionState,

        }
        //This is not maintainable..
        private void ParseLine(int line)
        {
            _stringTokenList[line].Clear();
            char ch;
            string word = "";
            for (int i = 0; i < _lineList[line].Length; i++)
            {
                ch = _lineList[line][i];
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    if (word != "" && !(char.IsLetterOrDigit(word[0]) || word[0] == '_'))
                    {
                        _stringTokenList[line].Add(word);
                        word = "";
                    }
                    word += ch;
                }
                else if (char.IsPunctuation(ch))
                {
                    if (word != "")
                    {
                        _stringTokenList[line].Add(word);
                        word = "";
                    }

                    if (ch == '/' && i + 1 < _lineList[line].Length && _lineList[line][i + 1] == '/')
                    {
                        _stringTokenList[line].Add("//");
                        i++;
                        continue;
                    }
                    word += ch;
                    _stringTokenList[line].Add(word);
                    word = "";
                    continue;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (word != "")
                    {
                        _stringTokenList[line].Add(word);
                        word = "";
                    }

                    word += " ";
                }
                else if (char.IsSymbol(ch))
                {
                    if (word != "")
                    {
                        _stringTokenList[line].Add(word);
                        word = "";
                    }
                    word += ch;
                    _stringTokenList[line].Add(word);
                    word = "";
                    continue;
                }

                if (i + 1 == _lineList[line].Length && word != "")
                {
                    _stringTokenList[line].Add(word);
                }
            }
        }

        public string GetColor(string token)
        {
            if (_syntaxHighlighter.TypeSet.Contains(token))
            {
                return _syntaxHighlighter.SyntaxMap["type"];
            }
            if (_syntaxHighlighter.StatementSet.Contains(token))
            {
                return _syntaxHighlighter.SyntaxMap["statement"];
            }
            if (_syntaxHighlighter.ModifierSet.Contains(token))
            {
                return _syntaxHighlighter.SyntaxMap["modifier"];
            }

            if (_syntaxHighlighter.SyntaxMap.ContainsKey(token))
            {
                return _syntaxHighlighter.SyntaxMap[token];
            }

            return _syntaxHighlighter.SyntaxMap["default"];
        }

        bool SpecialCase(string text)
        {
            switch (text[0])
            {
                case '(':
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX + 1, ")");
                    CursorX++;
                    break;
                case '{':
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX + 1, "}");
                    CursorX++;
                    break;
                case '[':
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX + 1, "]");
                    CursorX++;
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool CursorInMiddleOfBrace()
        {
            if (CursorX <= 0 || _lineList[CursorY].Length < 2 || CursorX == _lineList[CursorY].Length) return false;
            switch (_lineList[CursorY][CursorX - 1])
            {
                case '(' when _lineList[CursorY][CursorX] == ')':
                    break;
                case '{' when _lineList[CursorY][CursorX] == '}':
                    break;
                case '[' when _lineList[CursorY][CursorX] == ']':
                    break;
                default:
                    return false;
            }

            _lineList[CursorY] = _lineList[CursorY].Remove(CursorX - 1, 2);
            CursorX--;
            return true;
        }

        public void CopyText(int initX, int initY)
        {
            CopyLines.Clear();
            var lines = Math.Abs(initY - CursorY);
            var direction = initY - CursorY < 0 ? 1 : -1;
            for (int i = 0; i <= lines; i++)
            {
               
                var charactersInLine = CharactersInLine(initY + i * direction);
                var lineWidth = charactersInLine;
                var selectStart = 0;
                if (i == 0)
                {
                    lineWidth = CursorX > charactersInLine ? charactersInLine : initX;
                    if (lines == 0) 
                    {
                        selectStart = CursorX;
                        lineWidth = (initX - CursorX);
                    }
                    else if (direction == 1)
                    {
                        selectStart = initX;
                        lineWidth = charactersInLine - initX;
                    }
                }
                else if (initY + i * direction == CursorY)
                {
                    if (direction == 1)
                    {
                        lineWidth = CursorX;
                    }
                    else
                    {
                        selectStart = CursorX;
                        lineWidth = charactersInLine - CursorX;
                    }
                }

                var str = _lineList[initY + i * direction];
                CopyLines.Add(str.Substring(selectStart,lineWidth));
            }
            if(direction == 1)
                CopyLines.Reverse();
        }

        public void PasteText()
        {
            TextRedrawNeeded = true;
            var tempCursor = CursorY;
            for (int i = 0; i < CopyLines.Count; i++)
            {
                if (CursorY + i >= LineCount())
                {
                    _lineList.Add("");
                    _stringTokenList.Add(new List<string>());
                }
                var insertPos = i == 0 ? CursorX : 0;

                string newString = _lineList[CursorY + i].Insert(insertPos, CopyLines[i]);
                _lineList[CursorY + i] = newString;
                ParseLine(CursorY + i);
                LinesToDraw.Add(CursorY + i);
            }
        }

        private void EnsureValidCursorX()
        {

            CursorX = _lineList[CursorY].Length > 0 ? _lineList[CursorY].Length : 0;
        }

        public void ClearLinesToDraw()
        {
            LinesToDraw.Clear();
        }

        public int NumLinesToDraw()
        {
            return LinesToDraw.Count;
        }

        public int GetLineToDraw(int index)
        {
            return LinesToDraw[index];
        }

        public void AddCharText(string c)
        {
            TextRedrawNeeded = true;
            LinesToDraw.Add(CursorY);
            if (!SpecialCase(c))
            {
                _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, c);
                CursorX++;
            }
            else
            {
                ParseLine(CursorY);
            }
        }

        public void RemoveCharText()
        {
            RemoveCharacter();
            TextRedrawNeeded = true;
        }
    }

}
