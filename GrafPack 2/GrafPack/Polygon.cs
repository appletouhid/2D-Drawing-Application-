using System;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    class Polygon : NGon
    {
        public Polygon( Color color                                     ) : base( color ) { this.GenerateNGon( 2 ); }
        public Polygon( Color color , Point lineStart , Point lineEnd   ) : this( color )
        {
            this.Vertices[0] = new PointF( lineStart.X  , lineStart.Y   );
            this.Vertices[1] = new PointF( lineEnd.X    , lineEnd.Y     );
        }
    }
}
