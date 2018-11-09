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
            if (mUndoStack.Count == 0) return;

            CadOpe ope = mUndoStack.Pop();

            if (ope == null)
            {
                return;
            }

            ope.Undo(mDB);

            mRedoStack.Push(ope);
        }

        public void redo()
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

        public void dump()
        {
            DbgOut.pln(this.GetType().Name);
            DbgOut.pln("{");
            DbgOut.Indent++;
            DbgOut.pln("UndoStack [");
            DbgOut.Indent++;



            DbgOut.Indent--;
            DbgOut.Indent--;
            DbgOut.pln("}");
        }
    }
}
