using System;
using System.Collections.Generic;
using System.Drawing;

namespace GrafPack
{
    class RasterTools
    {
        public static void FillPoints( Graphics graphics , Color color , List<Point> points )
        {
            Brush brush = new SolidBrush( color );
            foreach( Point point in points )
            {
                graphics.FillRectangle( brush , point.X , point.Y , 1 , 1 );
            }
        }

        public static void FillPoints( Graphics graphics , Color color , List<Point> points , MatrixMxN mask ) // Used for dilation and whatnot
        {
            Brush brush = new SolidBrush( color );
            foreach( Point point in points )
            {
                int xCenter = mask.Rows     / 2;
                int yCenter = mask.Columns  / 2;

                for( int m = 0 ; m < mask.Rows      ; m++ )
                for( int n = 0 ; n < mask.Columns   ; n++ )
                if( mask.Get( m , n ) != 0 )
                    graphics.FillRectangle( brush , point.X + m - xCenter , point.Y + n - yCenter , 1 , 1 );
            }
        }

        public static void RasterizeLine( List<Point> points , double xStart , double yStart , double xEnd , double yEnd )
        {
            int x       = ( int )( xStart );
            int y       = ( int )( yStart );
            int width   = ( int )( xEnd - xStart );
            int height  = ( int )( yEnd - yStart );

            int xDelta1 = 0;
            int yDelta1 = 0;
            int xDelta2 = 0;
            int yDelta2 = 0;

            if( width   < 0 ) xDelta1 = -1; else if( width  > 0 ) xDelta1 = 1;
            if( height  < 0 ) yDelta1 = -1; else if( height > 0 ) yDelta1 = 1;
            if( width   < 0 ) xDelta2 = -1; else if( width  > 0 ) xDelta2 = 1;

            int longest     = Math.Abs( width   );
            int shortest    = Math.Abs( height  );

            if( longest <= shortest )
            {
                longest     = Math.Abs( height  );
                shortest    = Math.Abs( width   );

                if( height < 0 ) yDelta2 = -1; else
                if( height > 0 ) yDelta2 = 1;

                xDelta2 = 0;            
            }

            int numerator = longest >> 1;

            for( int i = 0 ; i <= longest ; i++ )
            {
                points.Add( new Point( x , y ) );
                numerator += shortest;

                if( numerator >= longest )
                {
                    numerator -= longest;
                    x += xDelta1;
                    y += yDelta1;
                }
                else
                {
                    x += xDelta2;
                    y += yDelta2;
                }
            }
        }

        public static void RasterizeEllipse( List<Point> points , double xCenter , double yCenter , double xRadius , double yRadius )
        {
            double x = 0;
            double y = yRadius;

            // Initial decision parameter of region 1 
            double xRadiusSquared       = Math.Pow( xRadius , 2 );
            double yRadiusSquared       = Math.Pow( yRadius , 2 );
            double twoXRadiusSquared    = xRadiusSquared * 2;
            double twoYRadiusSquared    = yRadiusSquared * 2;

            double d1                   = ( yRadiusSquared ) - ( xRadiusSquared * yRadius ) + ( 0.25f * xRadiusSquared );
            double xDelta               = twoYRadiusSquared * x;
            double yDelta               = twoXRadiusSquared * y;

            // Rasterize points of region 1 
            while ( xDelta < yDelta )
            {
                // Rasterize points based on 4-way symmetry 
                points.Add( new Point( ( int )(  x + xCenter ) , ( int )(  y + yCenter ) ) );
                points.Add( new Point( ( int )( -x + xCenter ) , ( int )(  y + yCenter ) ) );
                points.Add( new Point( ( int )( -x + xCenter ) , ( int )( -y + yCenter ) ) );
                points.Add( new Point( ( int )(  x + xCenter ) , ( int )( -y + yCenter ) ) );
                x++;

                // Check and update value of decision parameter based on algorithm 
                if ( d1 < 0 )
                {
                    xDelta += twoYRadiusSquared; 
                    d1     += xDelta + yRadiusSquared; 
                } 
                else
                {
                    y--;
                    xDelta += twoYRadiusSquared; 
                    yDelta -= twoXRadiusSquared; 
                    d1     += xDelta - yDelta + yRadiusSquared; 
                } 
            } 
  
            // Decision parameter of region 2 
            double d2 = ( yRadiusSquared * Math.Pow( x + 0.5f , 2 ) ) + ( xRadiusSquared * Math.Pow( y - 1 , 2 ) ) - ( xRadiusSquared * yRadiusSquared ); 
  
            // Rasterize points of region 2 
            while( y >= 0 )
            {
                // Rasterize points based on 4-way symmetry
                points.Add( new Point( ( int )(  x + xCenter ) , ( int )(  y + yCenter ) ) );
                points.Add( new Point( ( int )( -x + xCenter ) , ( int )(  y + yCenter ) ) );
                points.Add( new Point( ( int )( -x + xCenter ) , ( int )( -y + yCenter ) ) );
                points.Add( new Point( ( int )(  x + xCenter ) , ( int )( -y + yCenter ) ) );
                y--;

                // Check and update value of decision parameter based on algorithm 
                if ( d2 > 0 ) 
                {
                    yDelta -= twoXRadiusSquared;
                    d2     += xRadiusSquared - yDelta; 
                } 
                else
                {
                    x++;
                    xDelta += twoYRadiusSquared; 
                    yDelta -= twoXRadiusSquared; 
                    d2     += xDelta - yDelta + xRadiusSquared; 
                } 
            } 
        }
    }
}
