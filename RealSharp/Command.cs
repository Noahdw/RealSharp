using System;
using System.Collections.Generic;
using System.Text;

namespace RealSharp
{
    interface Command
    {
        void Execute();
        void undo();
    }

     class AddTextCommand : Command
    {
        public AddTextCommand(string newChar, EditorModel context)
        {
            mNewChar = newChar;
            mContext = context;
        }
       public void Execute()
        {
            mContext.AddCharText(mNewChar);
            mLastCursorX = mContext.CursorX;
            mLastCursorY = mContext.CursorY;
        }
       public void undo()
        {
            mContext.CursorX = mLastCursorX;
            mContext.CursorY = mLastCursorY;
            mContext.RemoveCharText();
        }

        string mNewChar;
        int mLastCursorX;
        int mLastCursorY;
        EditorModel mContext;
    }

    class RemoveTextCommand : Command
    {
        public RemoveTextCommand(EditorModel context)
        {
            mContext = context;
        }
         public void Execute()
        {
            mLastCursorX = mContext.CursorX;
            mLastCursorY = mContext.CursorY;
            mContext.RemoveCharText();
        }
        public void undo()
        {
            mContext.CursorX = mLastCursorX;
            mContext.CursorY = mLastCursorY;
            mContext.AddCharText(mOldChar);
        }
        string mOldChar;
        int mLastCursorX;
        int mLastCursorY;
        EditorModel mContext;
    }

}
