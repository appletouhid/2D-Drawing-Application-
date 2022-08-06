using System;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    class Circle : Shape
    {
        public Circle( Color color ) : base( color ) { }

        public override double GetClosestVertex( Point point , double range , out int closestVertexId , out Point result )
        {
            double distance = Math.Sqrt( Math.Pow( point.X - this.Origin.X , 2 ) + Math.Pow( point.Y - this.Origin.Y , 2 ) );
            result          = ( distance < range ) ? this.Origin : point;
            closestVertexId = 0;

            return distance;
        }

        public override void Rasterize()
        {
            // Call the Superclass Function
            base.Rasterize();

            double xScale           = this.Size.X * this.Scale.X / 2;
            double yScale           = this.Size.Y * this.Scale.Y / 2;
            double roundedRotation  = Math.Round( this.Rotation / 90.0 );

            if( roundedRotation == 1 || roundedRotation == -1 )
            {
                double tScale   = xScale;
                xScale          = yScale;
                yScale          = tScale;
            }

            // Rasterize Lines
            RasterTools.RasterizeEllipse( RasterPixels , this.Origin.X , this.Origin.Y , xScale , yScale );
        }
    }
}
