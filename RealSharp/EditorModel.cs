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
            for (var i = 0; i < MinimumLines; i++)
            {
                mLineList.Add("");
                mStringTokenList.Add(new List<string>());
                LinesToDraw.Add(i);
            }
            RequestRender();
        }
        private List<string> mLineList = new List<string>() { };

        private List<List<string>> mStringTokenList = new List<List<string>>()
        {
            new List<string>()
        };

        private Stack<Command> mCommandHistory = new Stack<Command>();

        public const int MinimumLines = 30;
        private SyntaxHighlighter mSyntaxHighlighter = new SyntaxHighlighter();
        public List<string> CopyLines = new List<string>();
        public List<int> LinesToDraw { get; } = new List<int>();
        public List<int> TestTest { get; } = new List<int>() { 1, 2, 3, 4, 5 };

        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int PrefferedCursorX { get; set; }
        public int SpacesPerTab = 4;
        public bool TextRedrawNeeded { get; set; }

        private readonly object _lineLock = new object();
        public void IssueCommand(Command c)
        {
            mCommandHistory.Push(c);
            c.Execute();
        }

        public bool KeyEvent(string text, int keyCode)
        {
            RequestRender();
            int cursorXAdjust = 1;
            if (CursorY < 0 || CursorY > LineCount())
            {
                return false;
            }
            if (text == "\t") // Tab
            {
                text = "";
                text = text.PadRight(SpacesPerTab);
                cursorXAdjust = SpacesPerTab;
            }
            if (text == null) // Likely a Fn key or arrow keys
            {
                HandleUnprintableCharacter(keyCode);
            }

            else if (text == "\r") // Enter key
            {
                InsertNewLine();
                RequestRender();
            }
            else if (text == "\b") // Backspace
            {
                IssueCommand(new RemoveCharCommand(this));
            }

            else if (CharIsText(text[0]))
            {
                IssueCommand(new AddTextCommand(text, this));
            }

            ParseLine(CursorY);
            return true;
        }

        public void RequestRender()
        {
            TextRedrawNeeded = true;
        }

        public void HandleMouseEventX(int xPos)
        {
            if (xPos <= mLineList[CursorY].Length && !(xPos < 0))
            {
                CursorX = xPos;
            }
            else
            {
                CursorX = mLineList[CursorY].Length;
            }
            if (CursorX < 0)
            {
                CursorX = 0;
            }

            PrefferedCursorX = CursorX;
        }
        public void HandleMouseEventY(int yPos)
        {
            if (yPos >= mLineList.Count)
            {
                CursorY = mLineList.Count - 1;
            }
            else
            {
                CursorY = yPos;
            }
            if (CursorY < 0)
            {
                CursorY = 0;
            }
        }


        public int LineCount()
        {
            return mLineList.Count;
        }

        public int TokensInLine(int line)
        {
            Debug.Assert(line < mStringTokenList.Count);
            return mStringTokenList[line].Count;
        }

        public int CharactersInLine(int line)
        {
            if (mStringTokenList.Count < line || line > mStringTokenList.Count)
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
            Debug.Assert(line < mStringTokenList.Count);
            Debug.Assert(token < mStringTokenList[line].Count);

            return mStringTokenList[line][token];
        }

        private void InsertNewLine()
        {
            string newLineText = "";
            newLineText = mLineList[CursorY].Substring(CursorX);

            if (CursorY + 1 == LineCount())
            {
                mLineList.Add(newLineText);
            }
            else
            {
                mLineList.Insert(CursorY + 1, newLineText);
            }

            if (CursorX != mLineList[CursorY].Length)
            {
                mLineList[CursorY] = mLineList[CursorY].Remove(CursorX);
            }

            CursorY++;
            CursorX = 0;
            mStringTokenList.Add(new List<string>());
            MarkAllLinesDirty();
        }
        /// Remove the selected character and return what was removed :TODO: NOT FULLY IMPLEMENTED (retChar)
        private char BackspaceRemoveCharacter()
        {
            var str = mLineList[CursorY];
            char retChar = '\n'; // This is just to signify that it's not text.
            if (str == null || str == "\r" || str == "") // If empty line remove the whole line
            {
                BackspaceLineEmptyText();
            }
            else if (CursorX == 0)  // Remove line and put remaining characters on line above it
            {
                BackspaceLineWithText();
            }
            else if (CursorInMiddleOfBrace())  // Handle cursor in middle of (), [], et al
            {
                mLineList[CursorY] = mLineList[CursorY].Remove(CursorX - 1, 2);
                CursorX--;
                LinesToDraw.Add(CursorY);
                ParseLine(CursorY);
            }
            else  // Remove a single character
            {
                LinesToDraw.Add(CursorY);
                mLineList[CursorY] = str.Remove(CursorX - 1, 1);
                CursorX--;
                ParseLine(CursorY);
            }
            return retChar;
        }

        private void BackspaceLineEmptyText()
        {
            mLineList.RemoveAt(CursorY);

            if (LineCount() < MinimumLines)
            {
                mLineList.Add("");
            }

            if (CursorY != 0)
            {
                CursorY--;
            }

            MarkAllLinesDirty();
            EnsureValidCursorX();
        }

        private void BackspaceLineWithText()
        {
            if (LineCount() == MinimumLines) return;
            if (CursorY <= 0) return;

            var text = mLineList[CursorY];
            var newCursorX = mLineList[CursorY - 1].Length;
            mLineList[CursorY - 1] += text;
            mLineList.RemoveAt(CursorY);
            if (CursorY <= 0)
            {
                return;
            }
            CursorY--;
            CursorX = newCursorX;
            MarkAllLinesDirty();
        }

        private void MarkAllLinesDirty()
        {
            for (int i = 0; i < LineCount(); i++)
            {
                LinesToDraw.Add(i);
                ParseLine(i);
            }
        }

        public void UndoCommand()
        {
            if (mCommandHistory.TryPop(out var c))
            {
                c.undo();
            }
        }

        public void PrintDebugInfo()
        {
            Console.WriteLine("CursorX: " + CursorX + ", CursorY: " + CursorY);
            Console.WriteLine("LinesToDraw, size = " + LinesToDraw.Count);
            Console.WriteLine("lineList, size = " + mLineList.Count);
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
                        CursorX = PrefferedCursorX = mLineList[CursorY].Length;
                    }
                    break;
                case 0x01000013: // UP ARROW
                    if (CursorY > 0)
                    {
                        CursorY--;
                        if (mLineList[CursorY].Length < PrefferedCursorX)
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
                    if (CursorX < mLineList[CursorY].Length)
                    {
                        CursorX++;
                        PrefferedCursorX = CursorX;
                    }
                    else if (CursorY + 1 < LineCount())
                    {
                        CursorY++;
                        CursorX = 0;
                        PrefferedCursorX = 0;
                    }
                    break;
                case 0x01000015: // DOWN ARROW
                    if (CursorY + 1 < LineCount())
                    {
                        CursorY++;
                        if (mLineList[CursorY].Length < PrefferedCursorX)
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

        //This is not maintainable.. TODO: Create a new class and implement a state machine or something..
        private void ParseLine(int line)
        {
            if (mStringTokenList.Count <= line)
            {
                Console.WriteLine("ParseLine on nonexistent line");
                return;
            }
            mStringTokenList[line].Clear();
            char ch;
            string word = "";
            for (int i = 0; i < mLineList[line].Length; i++)
            {
                ch = mLineList[line][i];
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    if (word != "" && !(char.IsLetterOrDigit(word[0]) || word[0] == '_'))
                    {
                        mStringTokenList[line].Add(word);
                        word = "";
                    }
                    word += ch;
                }
                else if (char.IsPunctuation(ch))
                {
                    if (word != "")
                    {
                        mStringTokenList[line].Add(word);
                        word = "";
                    }

                    if (ch == '/' && i + 1 < mLineList[line].Length && mLineList[line][i + 1] == '/')
                    {
                        mStringTokenList[line].Add("//");
                        i++;
                        continue;
                    }
                    word += ch;
                    mStringTokenList[line].Add(word);
                    word = "";
                    continue;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (word != "")
                    {
                        mStringTokenList[line].Add(word);
                        word = "";
                    }
                    word += " ";
                }
                else if (char.IsSymbol(ch))
                {
                    if (word != "")
                    {
                        mStringTokenList[line].Add(word);
                        word = "";
                    }
                    word += ch;
                    mStringTokenList[line].Add(word);
                    word = "";
                    continue;
                }

                if (i + 1 == mLineList[line].Length && word != "")
                {
                    mStringTokenList[line].Add(word);
                }
            }
        }

        /// Used by the QML to access syntax highlighter. TODO: Access the actual class in QML if possible
        public string GetColor(string token)
        {
            return mSyntaxHighlighter.GetColor(token);
        }

        /// Returns true if there was a special case and we removed it.E.g, (), [], etc.
        bool HandleSpecialPairCase(string text)
        {
            switch (text[0])
            {
                case '(':
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, text);
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX + 1, ")");
                    CursorX++;
                    break;
                case '{':
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, text);
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX + 1, "}");
                    CursorX++;
                    break;
                case '[':
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, text);
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX + 1, "]");
                    CursorX++;
                    break;
                case '\"':
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, text);
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX + 1, "\"");
                    CursorX++;
                    break;
                case '\'':
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, text);
                    mLineList[CursorY] = mLineList[CursorY].Insert(CursorX + 1, "\'");
                    CursorX++;
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool CursorInMiddleOfBrace()
        {
            if (CursorX <= 0 || mLineList[CursorY].Length < 2 || CursorX == mLineList[CursorY].Length) return false;
            char charLeftOfCursor = mLineList[CursorY][CursorX - 1];
            switch (charLeftOfCursor)
            {
                case '(' when mLineList[CursorY][CursorX] == ')':
                    break;
                case '{' when mLineList[CursorY][CursorX] == '}':
                    break;
                case '[' when mLineList[CursorY][CursorX] == ']':
                    break;
                case '\'' when mLineList[CursorY][CursorX] == '\'':
                    break;
                case '\"' when mLineList[CursorY][CursorX] == '\"':
                    break;
                default:
                    return false;
            }
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
                else if (initY + (i * direction) == CursorY)
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
                var str = mLineList[initY + (i * direction)];
                CopyLines.Add(str.Substring(selectStart, lineWidth));
            }
            if (direction == 1)
            {
                CopyLines.Reverse();
            }
        }

        public void PasteText()
        {
            var tempCursor = CursorY;
            for (int i = 0; i < CopyLines.Count; i++)
            {
                if (CursorY + i >= LineCount())
                {
                    mLineList.Add("");
                    mStringTokenList.Add(new List<string>());
                }
                var insertPos = i == 0 ? CursorX : 0;

                string newString = mLineList[CursorY + i].Insert(insertPos, CopyLines[i]);
                mLineList[CursorY + i] = newString;
                ParseLine(CursorY + i);
                LinesToDraw.Add(CursorY + i);
            }
            RequestRender();
        }

        private void EnsureValidCursorX()
        {
            CursorX = mLineList[CursorY].Length > 0 ? mLineList[CursorY].Length : 0;
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
            if (!HandleSpecialPairCase(c))
            {
                mLineList[CursorY] = mLineList[CursorY].Insert(CursorX, c);
                CursorX++;
            }
            ParseLine(CursorY);
            LinesToDraw.Add(CursorY);
        }

        public char RemoveCharText()
        {
            return BackspaceRemoveCharacter();
        }

        bool CharIsText(char c)
        {
            return char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c);
        }
    }
}
