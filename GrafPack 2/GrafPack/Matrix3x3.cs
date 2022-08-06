using System;
using System.Drawing;


namespace GrafPack
{
    [Serializable]
    class Matrix3x3 : MatrixMxN
    {
        private const double DTOR = Math.PI / 180;

        public Point    Point   { get { return new Point    ( ( int     ) this.Get(0,0) , ( int      ) this.Get(0,1) ); } }
        public PointF   PointF  { get { return new PointF   ( ( float   ) this.Get(0,0) , ( float    ) this.Get(0,1) ); } }

        public Matrix3x3(                       ) : base( 3 , 3             ) { }
        public Matrix3x3( Point point           ) : this( point.X , point.Y ) { }
        public Matrix3x3( PointF point          ) : this( point.X , point.Y ) { }
        public Matrix3x3( double x , double y   ) : this()
        {
            this.Identity();
            this.Set( 0 , 0 , x );
            this.Set( 0 , 1 , y );
            this.Set( 0 , 2 , 1 );
        }



        public Matrix3x3( double[,] values ) : base( values )
        {
            if( IsDifferentDimensions( this , new Matrix3x3() ) )
            {
                throw new InvalidDimensionException( "Cannot create a 3x3 matrix with " + values.Rank + " rows and " + values.GetLength( 0 ) + " columns. Use MatrixMxN instead." );
            }
        }



        public new Matrix3x3 Multiply( MatrixMxN other )
        {
            if( IsDifferentDimensions( this , other ) )
            {
                throw new InvalidDimensionException( "Cannot multiply a 3x3 matrix with " + other.Rows + " rows and " + other.Columns + " columns. Use MatrixMxN instead." );
            }

            base.Multiply( other );
            return this;
        }


        
        public static Matrix3x3 GetTranslationMatrix( Point point           ) { return GetTranslationMatrix( point.X , point.Y ); }
        public static Matrix3x3 GetTranslationMatrix( PointF point          ) { return GetTranslationMatrix( point.X , point.Y ); }
        public static Matrix3x3 GetTranslationMatrix( double x , double y   )
        {
            Matrix3x3 matrix = new Matrix3x3();

            matrix.Identity();
            matrix.Set( 2 , 0 , x );
            matrix.Set( 2 , 1 , y );

            return matrix;
        }


        
        public static Matrix3x3 GetRotationMatrix( double angle )
        {
            Matrix3x3 matrix = new Matrix3x3();

            matrix.Identity();
            matrix.Set( 0 , 0 ,  Math.Cos( angle * DTOR ) );
            matrix.Set( 0 , 1 , -Math.Sin( angle * DTOR ) );
            matrix.Set( 1 , 0 ,  Math.Sin( angle * DTOR ) );
            matrix.Set( 1 , 1 ,  Math.Cos( angle * DTOR ) );

            return matrix;
        }


        
        public static Matrix3x3 GetScaleMatrix( double xScale , double yScale )
        {
            Matrix3x3 matrix = new Matrix3x3();

            matrix.Identity();
            matrix.Set( 0 , 0 , xScale );
            matrix.Set( 1 , 1 , yScale );

            return matrix;
        }
    }
}
