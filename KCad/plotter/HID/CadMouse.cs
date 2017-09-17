using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;

namespace Plotter
{
    public class CadMouse
    {
        public delegate void ButtonHandler(CadMouse pointer, DrawContext dc, int x, int y);
        public delegate void MoveHandler(CadMouse pointer, DrawContext dc, int x, int y);
        public delegate void WheelHandler(CadMouse pointer, DrawContext dc, int x, int y, int delta);


        public ButtonHandler LButtonDown;
        public ButtonHandler LButtonUp;
        public ButtonHandler RButtonDown;
        public ButtonHandler RButtonUp;
        public ButtonHandler MButtonDown;
        public ButtonHandler MButtonUp;

        public WheelHandler Wheel;
        public MoveHandler PointerMoved;

        public CadVector LDownPoint = default(CadVector);
        public CadVector RDownPoint = default(CadVector);
        public CadVector MDownPoint = default(CadVector);

        public void MouseMove(DrawContext dc, int x, int y)
        {
            if (PointerMoved != null)
            {
                PointerMoved?.Invoke(this, dc, x, y);
            }
        }

        public void MouseDown(DrawContext dc, MouseButtons btn, int x, int y)
        {
            if (btn == MouseButtons.Left)
            {
                LDownPoint.x = x;
                LDownPoint.y = y;

                if (LButtonDown != null) LButtonDown(this, dc, x, y);
            }
            else if (btn == MouseButtons.Right)
            {
                RDownPoint.x = x;
                RDownPoint.y = y;

                if (LButtonDown != null) RButtonDown(this, dc, x, y);
            }
            else if (btn == MouseButtons.Middle)
            {
                MDownPoint.x = x;
                MDownPoint.y = y;

                if (MButtonDown != null) MButtonDown(this, dc, x, y);
            }
        }

        public void MouseUp(DrawContext dc, MouseButtons btn, int x, int y)
        {
            if (btn == MouseButtons.Left)
            {
                if (LButtonUp != null) LButtonUp(this, dc, x, y);
            }
            else if (btn == MouseButtons.Right)
            {
                if (LButtonUp != null) RButtonUp(this, dc, x, y);
            }
            else if (btn == MouseButtons.Middle)
            {
                if (MButtonUp != null) MButtonUp(this, dc, x, y);
            }
        }

        public void MouseWheel(DrawContext dc, int x, int y, int delta)
        {
            if (Wheel != null)
            {
                Wheel(this, dc, x, y, delta);
            }
        }
    }
}
