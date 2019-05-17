using System.Collections.Generic;
using CadDataTypes;
using Plotter.Controller;

namespace Plotter
{
    public class MoveKeyHandler
    {
        PlotterController Controller;

        public bool IsStarted;

        private List<CadFigure> EditFigList;

        private CadVertex Delta = default;

        public MoveKeyHandler(PlotterController controller)
        {
            Controller = controller;
        }

        public void MoveKeyUp()
        {
            if (IsStarted)
            {
                Controller.EndEdit();
            }

            IsStarted = false;
            Delta = CadVertex.Zero;
            EditFigList = null;
        }

        public void MoveKeyDown()
        {
            if (!IsStarted)
            {
                EditFigList = Controller.StartEdit();
                Delta = CadVertex.Zero;
                IsStarted = true;
            }

            CadVertex wx = Controller.CurrentDC.DevVectorToWorldVector(CadVertex.UnitX);
            CadVertex wy = Controller.CurrentDC.DevVectorToWorldVector(CadVertex.UnitY);

            wx = wx.UnitVector() / Controller.CurrentDC.WorldScale;
            wy = wy.UnitVector() / Controller.CurrentDC.WorldScale;

            wx *= SettingsHolder.Settings.KeyMoveUnit;
            wy *= SettingsHolder.Settings.KeyMoveUnit;

            if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Left))
            {
                Delta -= wx;
            }

            if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Right))
            {
                Delta += wx;
            }

            if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Up))
            {
                Delta -= wy;
            }

            if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Down))
            {
                Delta += wy;
            }

            if (Controller.State == PlotterController.States.SELECT)
            {
                if (EditFigList != null && EditFigList.Count > 0)
                {
                    Controller.MovePointsFromStored(EditFigList, Delta);
                    Controller.Redraw();
                }
                else
                {
                    CadVertex p = Controller.GetCursorPos();
                    Controller.SetCursorWoldPos(p + Delta);
                    Controller.Redraw();
                }
            }
        }
    }
}
