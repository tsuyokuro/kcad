using System;
using System.Collections.Generic;

namespace Plotter
{
    using Plotter;

    public class HistoryManager
    {
        private CadObjectDB mDB;

        public CadObjectDB DB
        {
            set
            {
                Clear();
                mDB = value;
            }
        }

        public Stack<CadOpe> mUndoStack = new Stack<CadOpe>();
        public Stack<CadOpe> mRedoStack = new Stack<CadOpe>();

        public HistoryManager(CadObjectDB db)
        {
            mDB = db;
        }

        public void Clear()
        {
            mUndoStack.Clear();
            mRedoStack.Clear();
        }

        public void foward(CadOpe ope)
        {
            mUndoStack.Push(ope);

            cleanObjectsInStack(mRedoStack);
            mRedoStack.Clear();
        }

        private void cleanObjectsInStack(Stack<CadOpe> stack)
        {
            foreach (CadOpe ope in stack)
            {
                ope.ReleaseResource(mDB);
            }
        }

        public bool canUndo()
        {
            return mUndoStack.Count > 0;
        }

        public bool canRedo()
        {
            return mRedoStack.Count > 0;
        }

        public void undo()
        {
            try
            {
                if (mUndoStack.Count == 0) return;

                CadOpe ope = mUndoStack.Pop();

                if (ope == null)
                {
                    return;
                }

                ope.Undo(mDB);

                mRedoStack.Push(ope);
            }
            catch (InvalidOperationException e)
            {

            }
        }

        public void redo()
        {
            try
            {
                if (mRedoStack.Count == 0) return;

                CadOpe ope = mRedoStack.Pop();

                if (ope == null)
                {
                    return;
                }

                ope.Redo(mDB);
                mUndoStack.Push(ope);
            }
            catch (InvalidOperationException e)
            {
            }
        }

        public void dump(DebugOut dout)
        {
            dout.println(this.GetType().Name);
            dout.println("{");
            dout.Indent++;
            dout.println("UndoStack [");
            dout.Indent++;



            dout.Indent--;
            dout.Indent--;
            dout.println("}");
        }
    }
}
