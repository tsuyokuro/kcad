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

        private CadVector Delta = default;

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
            Delta = CadVector.Zero;
            EditFigList = null;
        }

        public void MoveKeyDown()
        {
            if (!IsStarted)
            {
                EditFigList = Controller.StartEdit();
                Delta = CadVector.Zero;
                IsStarted = true;
            }

            CadVector wx = Controller.CurrentDC.DevVectorToWorldVector(CadVector.UnitX);
            CadVector wy = Controller.CurrentDC.DevVectorToWorldVector(CadVector.UnitY);

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

            Controller.MovePointsFromStored(EditFigList, Delta);

            Controller.Redraw();
        }
    }
}
