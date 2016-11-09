using System;
using System.Collections.Generic;

namespace Plotter
{
    using Plotter;

    class HistoryManager
    {
        CadObjectDB mDB;

        public Stack<CadOpe> mUndoStack = new Stack<CadOpe>();
        public Stack<CadOpe> mRedoStack = new Stack<CadOpe>();

        public HistoryManager(CadObjectDB db)
        {
            mDB = db;
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
                ope.releaseResource(mDB);
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

                ope.undo(mDB);

                mRedoStack.Push(ope);
            }
            catch (InvalidOperationException e)
            {
                Log.e("HistoryManager undo {0:s}", e.ToString());
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

                ope.redo(mDB);
                mUndoStack.Push(ope);
            }
            catch (InvalidOperationException e)
            {
                Log.e("HistoryManager redo {0:s}", e.ToString());
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
