using CadDataTypes;

namespace Plotter.Controller
{
    public class ViewController
    {
        public void SetOrigin(DrawContext dc, int pixX, int pixY)
        {
            CadVertex op = default(CadVertex);
            op.Set(pixX, pixY, 0);

            dc.SetViewOrg(op);
        }

        public void MoveOrigin(DrawContext dc, double pixDx, double pixDy)
        {
            CadVertex d = CadVertex.Create(pixDx, pixDy, 0);

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
            CadVertex op = dc.ViewOrg;

            CadVertex center = default(CadVertex); 
                
            center.Set(dc.ViewWidth / 2, dc.ViewHeight / 2, 0);

            CadVertex d = center - op;

            d *= f;

            op = center - d;


            dc.SetViewOrg(op);

            dc.UnitPerMilli *= f;
        }
    }
}
