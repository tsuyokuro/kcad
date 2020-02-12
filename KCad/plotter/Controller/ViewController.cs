using OpenTK;

namespace Plotter.Controller
{
    public class ViewController
    {
        public void SetOrigin(DrawContext dc, int pixX, int pixY)
        {
            Vector3d op = new Vector3d(pixX, pixY, 0);

            dc.SetViewOrg(op);
        }

        public void MoveOrigin(DrawContext dc, double pixDx, double pixDy)
        {
            Vector3d d = new Vector3d(pixDx, pixDy, 0);

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
            Vector3d op = dc.ViewOrg;

            Vector3d center = new Vector3d(dc.ViewWidth / 2, dc.ViewHeight / 2, 0);

            Vector3d d = center - op;

            d *= f;

            op = center - d;


            dc.SetViewOrg(op);

            dc.UnitPerMilli *= f;
        }
    }
}
