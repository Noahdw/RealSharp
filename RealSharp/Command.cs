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
            mContext.RequestRender();
        }
        public void undo()
        {
            mContext.CursorX = mLastCursorX;
            mContext.CursorY = mLastCursorY;
            mContext.RemoveCharText();
            mContext.RequestRender();
        }

        string mNewChar;
        int mLastCursorX;
        int mLastCursorY;
        EditorModel mContext;
    }

    class RemoveCharCommand : Command
    {
        public RemoveCharCommand(EditorModel context)
        {
            mContext = context;
        }
        public void Execute()
        {
            mLastCursorX = mContext.CursorX;
            mLastCursorY = mContext.CursorY;
            mContext.RemoveCharText();
            mContext.RequestRender();
        }
        public void undo()
        {
            mContext.CursorX = mLastCursorX;
            mContext.CursorY = mLastCursorY;
            mContext.AddCharText(mOldChar);
            mContext.RequestRender();
        }
        string mOldChar;
        int mLastCursorX;
        int mLastCursorY;
        EditorModel mContext;
    }
}
