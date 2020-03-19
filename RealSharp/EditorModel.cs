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
            for (var i = 0; i < 10; i++)
            {
                _lineList.Add("");
                _stringTokenList.Add(new List<string>());
            }
        }
        private List<string> _lineList = new List<string>()
        {
            ""
        };

        private List<List<string>> _stringTokenList = new List<List<string>>()
        {
        new List<string>()
        };
        private SyntaxHighlighter _syntaxHighlighter = new SyntaxHighlighter();
        public const int MinimumLines = 1;
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int PrefferedCursorX { get; set; }
        public int SpacesPerTab = 4;



        public bool KeyEvent(string text, int keyCode)
        {
            int CursorXAdjust = 1;
            if (CursorY < 0 || CursorY > LineCount())
            {
                return false;
            }
            if (text == "\t") //Tab
            {
                text = "";
                text = text.PadRight(SpacesPerTab);
                CursorXAdjust = SpacesPerTab;
            }
            if (text == null) // Likely a Fn key or arrow keys
            {
                HandleUnprintableCharacter(keyCode);
            }

            else if (text == "\r") //Enter key
            {
                InsertNewLine();
            }
            else if (text == "\b") //Backspace
            {
                RemoveCharacter();
            }


            else if (char.IsLetterOrDigit(text[0]) || char.IsWhiteSpace(text[0]) || char.IsPunctuation(text[0]) || char.IsSymbol(text[0]))
            {
                if (!SpecialCase(text))
                {
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    CursorX += CursorXAdjust;
                }
                else
                {
                    ParseLine(CursorY);
                    return true;
                }
            }

            ParseLine(CursorY);
            return true;
        }

        public void MouseEventX(int xPos)
        {
            if (xPos <= _lineList[CursorY].Length && !(xPos < 0))
            {
                CursorX = xPos;

            }
            else
            {
                CursorX = _lineList[CursorY].Length;
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
        }


        public int LineCount()
        {
            return _lineList.Count;
        }

        public int TokensInLine(int line)
        {
            return _stringTokenList[line].Count;
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
                ParseLine(i);
            }

        }

        private void RemoveCharacter()
        {
            var str = _lineList[CursorY];
            //If empty line remove the line
            if (str == null || str == "\r" || str == "")
            {
                if (LineCount() == MinimumLines)
                {
                    return;
                }
                _lineList.RemoveAt(CursorY);
                if (CursorY <= 0) return;
                CursorY--;
                EnsureValidCursorX();

            }
            else //remove single chracter
            {
                if (CursorX == 0)
                {
                    if (LineCount() == MinimumLines) return;
                    var text = _lineList[CursorY];
                    var newCursorX = _lineList[CursorY - 1].Length;
                    _lineList[CursorY - 1] += text;
                    _lineList.RemoveAt(CursorY);
                    if (CursorY <= 0) return;
                    CursorY--;
                    CursorX = newCursorX;

                }
                else if (CursorInMiddleOfBrace()) // handle cursor in middle of () et al
                {


                }
                else
                {
                    _lineList[CursorY] = str.Remove(CursorX - 1, 1);
                    CursorX--;
                }

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
        //private int State = States.NoState;
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
                case '\"':
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX + 1, "\"");
                    CursorX++;
                    break;
                case '\'':
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX, text);
                    _lineList[CursorY] = _lineList[CursorY].Insert(CursorX + 1, "\'");
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
                case '\'' when _lineList[CursorY][CursorX] == '\'':
                    break;
                case '\"' when _lineList[CursorY][CursorX] == '\"':
                    break;
                default:
                    return false;
            }

            _lineList[CursorY] = _lineList[CursorY].Remove(CursorX - 1, 2);
            CursorX--;
            return true;
        }
        private void EnsureValidCursorX()
        {

            CursorX = _lineList[CursorY].Length > 0 ? _lineList[CursorY].Length : 0;
        }

    }

}
