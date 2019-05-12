using OpenTK.Graphics;

namespace CadDataTypes
{
    public interface ICadVertexAttr
    {
        Color4 Color4
        {
            get; set;
        }

        CadVector Normal
        {
            get; set;
        }

        bool HasColor
        {
            get; set;
        }

        bool HasNormal
        {
            get; set;
        }
    }
}
