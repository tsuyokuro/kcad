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
        public delegate void DragHandler(CadMouse pointer, DrawContext dc, int x, int y);
        public delegate void WheelHandler(CadMouse pointer, DrawContext dc, int x, int y, int delta);

        public enum Buttons : int
        {
            NONE = 0,
            L_BUTTON = 1,
            R_BUTTON = 2,
            M_BUTTON = 3,
        }

        public enum ButtonMasks : uint
        {
            L_BUTTON = 1 << Buttons.L_BUTTON,
            R_BUTTON = 1 << Buttons.R_BUTTON,
            M_BUTTON = 1 << Buttons.M_BUTTON,
        }

        public enum CombiKeys : int
        {
            NONE = 0,
            CTRL = 1,
            SHIFT = 2,
            ALT = 3,
        }

        public enum CombiKeyMasks : uint
        {
            CTRL = 1 << CombiKeys.CTRL,
            SHIFT = 1 << CombiKeys.SHIFT,
            ALT = 1 << CombiKeys.ALT,
        }

        public CadPoint DownPoint = default(CadPoint);
        public CadPoint Point = default(CadPoint);

        private ButtonHandler[] ButtonDownProcs = { null, null, null, null };
        private ButtonHandler[] ButtonUpProcs = { null, null, null, null };
        private DragHandler[] DragProcs = { null, null, null, null };
        private MoveHandler MoveProc = null;
        private WheelHandler WheelProc = null;

        #region "Store mouse button"
        private uint DownedButton = 0;

        private void addDownedButton(Buttons btn)
        {
            uint mask = buttonMask(btn);
            DownedButton |= mask;
        }

        private void removeDownedButton(Buttons btn)
        {
            uint mask = buttonMask(btn);
            DownedButton &= ~mask;
        }
        #endregion

        #region "Get bit mask"
        private static uint combiKeyMask(CombiKeys key)
        {
            return (uint)0x1 << (int)key;
        }

        private static uint buttonMask(Buttons key)
        {
            return (uint)0x1 << (int)key;
        }
        #endregion

        #region "Covert codes"
        private static Buttons ConvMouseButton(MouseButtons mb)
        {
            switch (mb)
            {
                case MouseButtons.Left:
                    return Buttons.L_BUTTON;

                case MouseButtons.Right:
                    return Buttons.R_BUTTON;

                case MouseButtons.Middle:
                    return Buttons.M_BUTTON;
            }

            return Buttons.NONE;
        }

        private static uint getCombiKeyBits()
        {
            uint ret = 0;

            KeyStates ks;

            ks = Keyboard.GetKeyStates(Key.LeftCtrl);
            ks |= Keyboard.GetKeyStates(Key.LeftCtrl);

            if ((ks & KeyStates.Down)!=0)
            {
                ret |= (uint)CombiKeyMasks.CTRL;
            }

            ks = Keyboard.GetKeyStates(Key.LeftShift);
            ks |= Keyboard.GetKeyStates(Key.LeftShift);

            if ((ks & KeyStates.Down) != 0)
            {
                ret |= (uint)CombiKeyMasks.SHIFT;
            }

            ks = Keyboard.GetKeyStates(Key.LeftAlt);
            ks |= Keyboard.GetKeyStates(Key.LeftAlt);

            if ((ks & KeyStates.Down) != 0)
            {
                ret |= (uint)CombiKeyMasks.ALT;
            }

            return ret;
        }
        #endregion

        #region API
        public bool isDownCombiKey(CombiKeys key)
        {
            uint mask = combiKeyMask(key);
            uint bits = getCombiKeyBits();
            return (bits & mask) != 0;
        }

        public bool isDown(Buttons bt)
        {
            uint mask = buttonMask(bt);
            return (DownedButton & mask) != 0;
        }

        public void down(DrawContext dc, MouseButtons button, int x, int y)
        {
            Buttons bc = ConvMouseButton(button);
            down(dc, bc, x, y);
        }

        public void down(DrawContext dc, Buttons button, int x, int y)
        {
            addDownedButton(button);
            DownPoint.set(x, y, 0);
            Point.set(x, y, 0);

            ButtonHandler bh = ButtonDownProcs[(uint)button];
            if (bh != null)
            {
                bh(this, dc, x, y);
            }
        }

        public void up(DrawContext dc, MouseButtons button, int x, int y)
        {
            Buttons bc = ConvMouseButton(button);
            up(dc, bc, x, y);
        }

        public void up(DrawContext dc, Buttons button, int x, int y)
        {
            Point.set(x, y, 0);

            ButtonHandler bh = ButtonUpProcs[(uint)button];
            if (bh != null)
            {
                bh(this, dc, x, y);
            }

            removeDownedButton(button);
        }

        public void pointerMoved(DrawContext dc, int x, int y)
        {
            if (DownedButton != 0)
            {
                Buttons btn;

                btn = Buttons.M_BUTTON;

                if (isDown(btn))
                {
                    if (DragProcs[(uint)btn] != null)
                    {
                        DragProcs[(uint)btn](this, dc, x, y);
                    }
                }

                btn = Buttons.R_BUTTON;

                if (isDown(btn))
                {
                    if (DragProcs[(uint)btn] != null)
                    {
                        DragProcs[(uint)btn](this, dc, x, y);
                    }
                }

                btn = Buttons.L_BUTTON;

                if (isDown(btn))
                {
                    if (DragProcs[(uint)btn] != null)
                    {
                        DragProcs[(uint)btn](this, dc, x, y);
                    }
                }
            }
            else    
            {
                if (MoveProc != null)
                {
                    MoveProc(this, dc, x, y);
                }
            }

            Point.set(x, y, 0);
        }

        public void wheel(DrawContext dc, int x, int y, int delta)
        {
            if (WheelProc != null)
            {
                WheelProc(this, dc, x, y, delta);
            }
        }
        #endregion

        #region "set event handlers"
        public void setButtonDownProc(Buttons btn, ButtonHandler f)
        {
            ButtonDownProcs[(uint)btn] += f;
        }

        public void setButtonUpProc(Buttons btn, ButtonHandler f)
        {
            ButtonUpProcs[(uint)btn] += f;
        }

        public void setDragProc(Buttons btn, DragHandler f)
        {
            DragProcs[(uint)btn] += f;
        }

        public void setMoveProc(MoveHandler f)
        {
            MoveProc += f;
        }

        public void setWheelProc(WheelHandler f)
        {
            WheelProc += f;
        }
        #endregion
    }
}
