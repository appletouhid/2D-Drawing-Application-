using System;

namespace GrafPack
{
    class InvalidDimensionException : Exception
    {
        public string Error { get; protected set; }

        public InvalidDimensionException( string error )
        {
            this.Error = error;
        }
    }

    [Serializable]
    class MatrixMxN
    {
        double[,] Values { get; set; }

        public int Rows     { get { return this.Values.Length / this.Columns;   } }
        public int Columns  { get { return this.Values.GetLength(0);            } }
        public bool Square  { get { return this.Rows == this.Columns;           } }



        public MatrixMxN( int rows , int columns )
        {
            this.Values = new double[rows,columns];
            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                this.Values[m,n] = ( n == m ) ? 1 : 0;
        }



        public MatrixMxN( double[,] values )
        {
            if( !IsValidMatrix( values ) )
            {
                throw new InvalidDimensionException( "Cannot create matrix from non-uniform row dimensions" );
            }
            else
            {
                this.Values = values;
            }
        }



        protected static bool IsValidMatrix( double[,] values )
        {
            int dimension = values.GetLength( 0 );

            for( int i = 1 ; i < values.Length / values.GetLength(0) ; i++ )
            {
                if( values.GetLength( i ) != dimension )
                {
                    return false;
                }
            }

            return true;
        }



        protected static bool IsDifferentDimensions( MatrixMxN matrix_a , MatrixMxN matrix_b )
        {
            return matrix_a.Rows != matrix_b.Rows || matrix_a.Columns != matrix_b.Columns;
        }



        protected static bool IsIncompatibleDimensions( MatrixMxN matrix_a , MatrixMxN matrix_b )
        {
            return matrix_a.Columns != matrix_b.Rows;
        }



        protected static void ThrowIfDifferentDimensions( MatrixMxN matrix_a , MatrixMxN matrix_b , string operation )
        {
            if( IsDifferentDimensions( matrix_a , matrix_b ) )
            {
                throw new InvalidDimensionException( "Cannot perform " + operation + " on 2 matrices of different dimensions." );
            }
        }



        protected static void ThrowIfIncompatibleDimensions( MatrixMxN matrix_a , MatrixMxN matrix_b , string operation )
        {
            if( IsIncompatibleDimensions( matrix_a , matrix_b ) )
            {
                throw new InvalidDimensionException( "Cannot perform " + operation + " on 2 matrices of incompatible dimensions." );
            }
        }



        public double Get( int m , int n )
        {
            return this.Values[m,n];
        }



        public double[] GetRow( int m )
        {
            double[] result = new double[this.Columns];
            for( int i = 0 ; i < this.Columns ; result[i] = this.Values[m,i++] );
            return result;
        }



        public double[] GetColumn( int n )
        {
            double[] result = new double[this.Rows];
            for( int i = 0 ; i < this.Rows; result[i] = this.Values[i++,n] );
            return result;
        }



        public void Set( int m , int n , double value )
        {
            this.Values[m,n] = value;
        }



        public void SetRow( int m , double[] values )
        {
            for( int i = 0 ; i < values.Length ; this.Values[m,i] = values[i++] );
        }



        public void SetColumn( int n , double[] values )
        {
            for( int i = 0 ; i < values.Length ; this.Values[i,n] = values[i++] );
        }



        public virtual MatrixMxN Add( MatrixMxN other )
        {
            ThrowIfDifferentDimensions( this , other , "addition" );

            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                this.Values[m,n] += other.Values[m,n];

            return this;
        }



        public virtual MatrixMxN Subtract( MatrixMxN other )
        {
            ThrowIfDifferentDimensions( this , other , "subtractiton" );

            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                this.Values[m,n] -= other.Values[m,n];

            return this;
        }



        public virtual MatrixMxN EntrywiseMultiply( MatrixMxN other )
        {
            ThrowIfDifferentDimensions( this , other , "entrywise multiplication" );

            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                this.Values[m,n] *= other.Values[m,n];

            return this;
        }



        public virtual MatrixMxN Multiply( MatrixMxN other )
        {
            ThrowIfIncompatibleDimensions( this , other , "multiplication" );

            double[,] result = new double[this.Rows,other.Columns];

            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < other.Columns  ; n++ )
            for( int i = 0 ; i < this.Columns   ; i++ )
                result[m,n] += this.Values[m,i] * other.Values[i,n];

            this.Values = result;
            return this;
        }



        public virtual MatrixMxN Transpose()
        {
            double[,] result = new double[this.Columns,this.Rows];
            
            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                result[n,m] = this.Values[m,n];

            this.Values = result;
            return this;
        }



        public virtual MatrixMxN Identity()
        {
            for( int m = 0 ; m < this.Rows      ; m++ )
            for( int n = 0 ; n < this.Columns   ; n++ )
                this.Values[m,n] = ( n == m ) ? 1 : 0;

            return this;
        }
    }
}
