using System;
using System.Collections.Generic;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    abstract class Shape
    {
        public int Uuid                 { get; protected set;   }
        public double Rotation          { get; protected set;   }
        public List<Point> RasterPixels { get; protected set;   }
        public Point Origin             { get; protected set;   }
        
        public Color Color              { get; set;             }
        public Point DrawSize           { get; protected set;   }
        public Point DrawnSize          { get; protected set;   }
        public Point Size               { get; protected set;   }
        public PointF Scale             { get; protected set;   }
        public Bitmap RasterImage       { get; protected set;   }
        public Bitmap EdgeSelectionMask { get; protected set;   }
        public MatrixMxN DilationMask   { get; protected set;   }


        public virtual bool IsValid
        {
            get
            {
                return this.Scale.X != 0 && this.Scale.Y != 0;
            }
        }



        public bool IsDrawn
        {
            get
            {
                return this.DrawSize.X == this.DrawnSize.X && this.DrawSize.Y == this.DrawnSize.Y;
            }
        }



        protected Matrix3x3 TranslationMatrix
        {
            get
            {
                return Matrix3x3.GetTranslationMatrix( this.Origin );
            }
        }



        protected Matrix3x3 InverseTranslationMatrix
        {
            get
            {
                return Matrix3x3.GetTranslationMatrix( new Point( -this.Origin.X , -this.Origin.Y ) );
            }
        }



        public Matrix3x3 RotationMatrix
        {
            get
            {
                return Matrix3x3.GetRotationMatrix( this.Rotation );
            }
        }



        public Matrix3x3 InverseRotationMatrix
        {
            get
            {
                return Matrix3x3.GetRotationMatrix( -this.Rotation );
            }
        }



        protected Matrix3x3 ScaleMatrix
        {
            get
            {
                return Matrix3x3.GetScaleMatrix( this.Scale.X , this.Scale.Y );
            }
        }



        protected Matrix3x3 InverseScaleMatrix
        {
            get
            {
                return Matrix3x3.GetScaleMatrix( 1.0 / this.Scale.X , 1.0 / this.Scale.Y );
            }
        }



        protected Matrix3x3 TransformMatrix
        {
            get
            {
                return this.ScaleMatrix.Multiply( this.RotationMatrix ).Multiply( this.TranslationMatrix );
            }
        }



        protected Matrix3x3 InverseTransformMatrix
        {
            get
            {
                return this.InverseTranslationMatrix.Multiply( this.InverseRotationMatrix ).Multiply( this.InverseScaleMatrix );
            }
        }


        // This is the base class for Shapes in the application. It should allow an array or LL to be created containing different kinds of shapes.
        public Shape( Color color )
        {
            this.RasterPixels       = new List<Point>();
            this.RasterImage        = new Bitmap( 1 , 1 );
            this.EdgeSelectionMask  = new Bitmap( 1 , 1 );
            this.DrawSize           = new Point( 0 , 0 );
            this.DrawnSize          = new Point( 0 , 0 );
            this.Origin             = new Point( 0 , 0 );
            this.Size               = new Point( 1000 , 1000 ); // Provide a large enough size. We'll scale it down later.
            this.Scale              = new PointF( 1 , 1 );
            this.DilationMask       = new MatrixMxN( 5 , 5 );
            this.Color              = color;
            
            this.DilationMask.Set( 0 , 1 , 1 );
            this.DilationMask.Set( 0 , 2 , 1 );
            this.DilationMask.Set( 0 , 3 , 1 );
            this.DilationMask.Set( 1 , 0 , 1 );
            this.DilationMask.Set( 1 , 1 , 1 );
            this.DilationMask.Set( 1 , 2 , 1 );
            this.DilationMask.Set( 1 , 3 , 1 );
            this.DilationMask.Set( 1 , 4 , 1 );
            this.DilationMask.Set( 2 , 0 , 1 );
            this.DilationMask.Set( 2 , 1 , 1 );
            this.DilationMask.Set( 2 , 2 , 1 );
            this.DilationMask.Set( 2 , 3 , 1 );
            this.DilationMask.Set( 2 , 4 , 1 );
            this.DilationMask.Set( 3 , 0 , 1 );
            this.DilationMask.Set( 3 , 1 , 1 );
            this.DilationMask.Set( 3 , 2 , 1 );
            this.DilationMask.Set( 3 , 3 , 1 );
            this.DilationMask.Set( 3 , 4 , 1 );
            this.DilationMask.Set( 4 , 1 , 1 );
            this.DilationMask.Set( 4 , 2 , 1 );
            this.DilationMask.Set( 4 , 3 , 1 );
        }



        public void SetRegion( Point TopLeft , Point BottomRight )
        {
            // Ensure that our top-left point is the minimum and bottom-right point is the maximum.
            int xCenter = ( TopLeft.X + BottomRight.X ) / 2;
            int yCenter = ( TopLeft.Y + BottomRight.Y ) / 2;
            float xSize = Math.Abs( TopLeft.X - BottomRight.X );
            float ySize = Math.Abs( TopLeft.Y - BottomRight.Y );
            this.Origin = new Point( xCenter , yCenter );
            this.Scale  = new PointF( xSize / this.Size.X , ySize / this.Size.Y );
        }



        public virtual Matrix3x3 CommitScale()
        {
            // Ensure that our top-left point is the minimum and bottom-right point is the maximum.
            PointF lastScale    = this.Scale;
            int xSize           = ( int )( this.Size.X * this.Scale.X );
            int ySize           = ( int )( this.Size.Y * this.Scale.Y );
            this.Size           = new Point( xSize , ySize );
            this.Scale          = new PointF( 1 , 1 );

            return Matrix3x3.GetScaleMatrix( lastScale.X , lastScale.Y );
        }
        


        public virtual void Rotate( double angle )
        {
            // Normalize our angle to be between -180 to 180 degrees
            this.Rotation = ( this.Rotation + angle ) % 360;                                    // Reduce the angle
            this.Rotation = ( this.Rotation + 360   ) % 360;                                    // Convert the angle into a positive value
            this.Rotation = ( this.Rotation > 180   ) ? this.Rotation - 360 : this.Rotation;    // Normalize to -180 to 180
        }

        public virtual void Move( Point translation )
        {
            Matrix3x3 translationMatrix = Matrix3x3.GetTranslationMatrix( translation );
            Matrix3x3 originMatrix      = new Matrix3x3( this.Origin ).Multiply( translationMatrix );
            this.Origin                 = originMatrix.Point;
        }



        public virtual void Resize( double xScale , double yScale )
        {
            PointF newScale = new PointF( this.Scale.X * ( float ) xScale , this.Scale.Y * ( float ) yScale );
            if( !float.IsInfinity( newScale.X ) && !float.IsInfinity( newScale.Y ) && !float.IsNaN( newScale.X ) && !float.IsNaN( newScale.Y ) && newScale.X != 0 && newScale.Y != 0 )
            {
                this.Scale = newScale;
            }
        }



        public virtual void SetScreenSize( int width , int height )
        {
            this.DrawSize = new Point( width , height );
        }



        public virtual void Invalidate()
        {
            this.DrawnSize = new Point( 0 ,  0 );
        }



        public virtual void Rasterize   (                   ) { this.RasterPixels.Clear(); }
        public virtual void DrawImage   ( Color maskColor   )
        {
            if( !this.IsDrawn )
            {
                this.Rasterize();
                this.RasterImage.Dispose();
                this.EdgeSelectionMask.Dispose();

                this.RasterImage            = new Bitmap( this.DrawSize.X , this.DrawSize.Y );
                this.EdgeSelectionMask      = new Bitmap( this.DrawSize.X , this.DrawSize.Y );

                Graphics ImageGraphics      = Graphics.FromImage( this.RasterImage          );
                Graphics EdgeMaskGraphics   = Graphics.FromImage( this.EdgeSelectionMask    );

                RasterTools.FillPoints( ImageGraphics       , this.Color    , this.RasterPixels                     );
                RasterTools.FillPoints( EdgeMaskGraphics    , maskColor     , this.RasterPixels , this.DilationMask );

                this.DrawnSize = this.DrawSize;
            }
        }



        public virtual bool IsEdge( Point point )
        {
            return this.IsDrawn && this.EdgeSelectionMask.GetPixel( point.X , point.Y ).A > 0;
        }



        public abstract double GetClosestVertex( Point point , double range , out int closestVertexId , out Point result );
    }
}
