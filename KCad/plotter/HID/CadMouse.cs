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


        public ButtonHandler LDown;
        public ButtonHandler LUp;
        public ButtonHandler RDown;
        public ButtonHandler RUp;
        public ButtonHandler MDown;
        public ButtonHandler MUp;

        public WheelHandler Wheel;
        public MoveHandler MovePointer;

        public CadPoint LDownPoint = default(CadPoint);
        public CadPoint RDownPoint = default(CadPoint);
        public CadPoint MDownPoint = default(CadPoint);

        public void MouseMove(DrawContext dc, int x, int y)
        {
            if (MovePointer != null)
            {
                MovePointer?.Invoke(this, dc, x, y);
            }
        }

        public void MouseDown(DrawContext dc, MouseButtons btn, int x, int y)
        {
            if (btn == MouseButtons.Left)
            {
                LDownPoint.x = x;
                LDownPoint.y = y;

                if (LDown != null) LDown(this, dc, x, y);
            }
            else if (btn == MouseButtons.Right)
            {
                RDownPoint.x = x;
                RDownPoint.y = y;

                if (LDown != null) RDown(this, dc, x, y);
            }
            else if (btn == MouseButtons.Middle)
            {
                MDownPoint.x = x;
                MDownPoint.y = y;

                if (MDown != null) MDown(this, dc, x, y);
            }
        }

        public void MouseUp(DrawContext dc, MouseButtons btn, int x, int y)
        {
            if (btn == MouseButtons.Left)
            {
                if (LUp != null) LUp(this, dc, x, y);
            }
            else if (btn == MouseButtons.Right)
            {
                if (LUp != null) RUp(this, dc, x, y);
            }
            else if (btn == MouseButtons.Middle)
            {
                if (MUp != null) MUp(this, dc, x, y);
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
