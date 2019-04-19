using CadDataTypes;

namespace Plotter.Controller
{
    public class ViewController
    {
        public void SetOrigin(DrawContext dc, int pixX, int pixY)
        {
            CadVector op = default(CadVector);
            op.Set(pixX, pixY, 0);

            dc.SetViewOrg(op);
        }

        public void MoveOrigin(DrawContext dc, double pixDx, double pixDy)
        {
            CadVector d = CadVector.Create(pixDx, pixDy, 0);

            dc.SetViewOrg(dc.ViewOrg + d);
        }

        public void AdjustOrigin(DrawContext dc, double pixX, double pixY, int vw, int vh)
        {
            double dx = vw / 2 - pixX;
            double dy = vh / 2 - pixY;

            MoveOrigin(dc, dx, dy);
        }

        public void DpiUpDown(DrawContext dc, double f)
        {
            CadVector op = dc.ViewOrg;

            CadVector center = default(CadVector); 
                
            center.Set(dc.ViewWidth / 2, dc.ViewHeight / 2, 0);

            CadVector d = center - op;

            d *= f;

            op = center - d;


            dc.SetViewOrg(op);

            dc.UnitPerMilli *= f;
        }
    }
}
