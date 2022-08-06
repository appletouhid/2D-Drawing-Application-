using System;
using System.Collections.Generic;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    class NGon : Shape
    {
        private const double DTOR = Math.PI / 180;

        [Serializable]
        protected class Edge
        {
            public int VertexIdA { get; set; }
            public int VertexIdB { get; set; }
            public bool IsValid
            {
                get
                {
                    return this.VertexIdA != this.VertexIdB;
                }
            }

            public Edge( int vertexIdA , int vertexIdB )
            {
                this.VertexIdA = vertexIdA;
                this.VertexIdB = vertexIdB;
            }

            public void Swap( int vertexIdFrom , int vertexIdTo )
            {
                this.VertexIdA  = this.VertexIdA != vertexIdFrom ? this.VertexIdA : vertexIdTo;
                this.VertexIdB  = this.VertexIdB != vertexIdFrom ? this.VertexIdB : vertexIdTo;
            }

            public bool Has( int vertexId )
            {
                return this.VertexIdA == vertexId || this.VertexIdB == vertexId;
            }

            public int GetOther( int vertexId )
            {
                return this.VertexIdA == vertexId ? this.VertexIdA : this.VertexIdB;
            }
        }

        public List<PointF> Vertices; // Vertices of this shape
        protected List<Edge> Edges;



        public NGon( Color color ) : base( color )
        {
            this.Vertices   = new List<PointF>();
            this.Edges      = new List<Edge>();
        }



        public override bool IsValid
        {
            get
            {
                return base.IsValid && this.Vertices.Count > 0 && this.Edges.Count > 0;
            }
        }



        // Transform our vertices and return a copy
        protected List<PointF> GetTransformedVertices( Matrix3x3 transformationMatrix )
        {
            List<PointF> newVertices = new List<PointF>( this.Vertices.Capacity );

            for( int i = 0 ; i < this.Vertices.Count ; i++ )
            {
                Matrix3x3 vertexMatrix = new Matrix3x3( this.Vertices[i] );
                newVertices.Add( vertexMatrix.Multiply( transformationMatrix ).PointF );
            }

            return newVertices;
        }



        // Changes the position of a vertex
        public virtual Point MoveVertex( int vertexId , Point moveDelta )
        {
            Matrix3x3 translationMatrix = Matrix3x3.GetTranslationMatrix( moveDelta.X , moveDelta.Y );
            Matrix3x3 vertexMatrix      = new Matrix3x3( this.Vertices[vertexId] );

            vertexMatrix.Multiply( this.TransformMatrix         );
            vertexMatrix.Multiply( translationMatrix            );
            vertexMatrix.Multiply( this.InverseTransformMatrix  );

            this.Vertices[vertexId] = vertexMatrix.PointF;

            return vertexMatrix.Multiply( this.TransformMatrix ).Point;
        }



        // Validate all vertices and make sure that all edges have at least one connection.
        protected virtual void Validate()
        {
            int[] connections = new int[this.Vertices.Count];

            for( int i = 0 ; i < this.Edges.Count ; i++ )
            {
                Edge edge = this.Edges[i];
                if( edge.IsValid )
                {
                    connections[edge.VertexIdA]++;
                    connections[edge.VertexIdB]++;
                }
                else
                {
                    this.Edges.RemoveAt( i-- );
                }
            }

            for( int i = this.Vertices.Count - 1 ; i >= 0 ; i-- )
            {
                if( connections[i] == 0 )
                {
                    this.Vertices.RemoveAt( i );
                    this.ReduceEdgeVertexIds( i );
                }
            }
        }



        // Reduce edges' vertices ID
        protected virtual void ReduceEdgeVertexIds( int floorId )
        {
            foreach( Edge edge in this.Edges )
            {
                if( edge.VertexIdA > floorId ) edge.VertexIdA--;
                if( edge.VertexIdB > floorId ) edge.VertexIdB--;
            }
        }



        // Deletes a vertex
        public virtual void DeleteVertex( int vertexId )
        {
            // Remove the vertex and it's reference in our adjacency list
            this.Vertices.RemoveAt( vertexId );
            this.Edges.RemoveAll( edge => edge.Has( vertexId ) );
            this.ReduceEdgeVertexIds( vertexId );
            this.Validate();
        }



        // Adds a vertex at a point on the edge closest to the area clicked
        public virtual Point RefineEdge( Point point , out int newVertexId )
        {
            int edgeId;
            PointF newVertex        = this.GetClosestPoint( point , out edgeId );
            Edge refinedEdge        = this.Edges[edgeId];
            Matrix3x3 vertexMatrix  = new Matrix3x3( newVertex ).Multiply( this.InverseTransformMatrix );
            newVertexId             = this.Vertices.Count;

            this.Vertices.Add( vertexMatrix.Point );
            this.Edges.RemoveAt( edgeId );
            this.Edges.Add( new Edge( refinedEdge.VertexIdA , newVertexId ) );
            this.Edges.Add( new Edge( refinedEdge.VertexIdB , newVertexId ) );

            return new Matrix3x3( newVertex ).Point;
        }



        // Merge 2 NGons together
        public virtual int Merge( NGon shape )
        {
            if( this != shape )
            {
                int vertexIdOffset          = this.Vertices.Count;
                int newEdgeIndex            = this.Edges.Count;
                List<PointF> newVertices    = shape.GetTransformedVertices( shape.TransformMatrix.Multiply( this.InverseTransformMatrix ) );

                this.Vertices.AddRange( newVertices );
                this.Edges.AddRange( shape.Edges );

                for( int i = newEdgeIndex ; i < this.Edges.Count ; i++ )
                {
                    this.Edges[i].VertexIdA += vertexIdOffset;
                    this.Edges[i].VertexIdB += vertexIdOffset;
                }

                return vertexIdOffset;
            }

            return 0;
        }



        // Returns true if there exist an edge between vertex A and vertex B
        public virtual bool HaveEdge( int vertexIdA , int vertexIdB )
        {
            foreach( Edge edge in this.Edges )
                if( edge.Has( vertexIdA ) && edge.Has( vertexIdB ) )
                    return true;

            return false;
        }



        // Extend a vertex to another point and/or shape, adding an edge in between
        public virtual Point LinkVertex( int vertexIdA , int vertexIdB , NGon shape , out int newVertexId )
        {
            newVertexId = vertexIdB + this.Merge( shape );

            // Do not allow duplicated edges.
            // Multiple edges per vertices pair is technically possible to occur during welding.
            // This is just a safety measure to prevent even more of those duplications.
            if( !this.HaveEdge( vertexIdA , newVertexId ) )
            {
                // Make sure we're not linking a vertex to itself as that would be invalid.
                if( vertexIdA != newVertexId )
                {
                    this.Edges.Add( new Edge( vertexIdA , newVertexId ) );
                }
            }
            else
            {
                newVertexId = vertexIdA;
            }

            return new Matrix3x3( this.Vertices[newVertexId] ).Multiply( this.TransformMatrix ).Point;
        }



        // Adds a vertex connected to an existing vertex
        public virtual int AddVertex( Point point , int connectedVertexId )
        {
            int newVertexId     = this.Vertices.Count;
            PointF newVertex    = new Matrix3x3( point ).Multiply( this.InverseTransformMatrix ).PointF;
            this.Vertices.Add( newVertex );
            this.Edges.Add( new Edge( connectedVertexId , newVertexId ) );

            return newVertexId;
        }



        // Averages two vertex's position and remove the vertex positioned later in the array
        public virtual Point WeldVertex( int vertexIdA , int vertexIdB , NGon shape )
        {
            vertexIdB          += this.Merge( shape );
            PointF vertexA      = this.Vertices[vertexIdA];
            PointF vertexB      = this.Vertices[vertexIdB];
            PointF weldedVertex = new PointF( ( vertexA.X + vertexB.X ) / 2 , ( vertexA.Y + vertexB.Y ) / 2 );

            this.Vertices[vertexIdA] = weldedVertex;

            foreach( Edge edge in this.Edges )
                edge.Swap( vertexIdB , vertexIdA );

            this.Validate();
            return new Matrix3x3( weldedVertex ).Multiply( this.TransformMatrix ).Point;
        }



        public virtual void GenerateNGon( int sides )
        {
            // Generate NGon Vertices centered around (0,0)
            this.Vertices.Clear();
            
            // Special case for 2 points, our shape degenerates into a single line
            if( sides == 2 )
            {
                Vertices.Add( new Point(  Size.X / 2 ,  Size.Y / 2 ) );
                Vertices.Add( new Point( -Size.X / 2 , -Size.Y / 2 ) );
            }
            
            // Special case for triangle, we can't generate in a circular pattern
            else if( sides == 3 )
            {
                Vertices.Add( new Point(  0          , -Size.Y / 2 ) ); // Apex
                Vertices.Add( new Point(  Size.X / 2 ,  Size.Y / 2 ) );
                Vertices.Add( new Point( -Size.X / 2 ,  Size.Y / 2 ) );
            }

            // Special case for rectangle, we'll put the 4 points at a 45 degrees offset
            else if( sides == 4 )
            {
                Vertices.Add( new Point(  Size.X / 2 ,  Size.Y / 2 ) );
                Vertices.Add( new Point( -Size.X / 2 ,  Size.Y / 2 ) );
                Vertices.Add( new Point( -Size.X / 2 , -Size.Y / 2 ) );
                Vertices.Add( new Point(  Size.X / 2 , -Size.Y / 2 ) );
            }

            // Any polygon above 4 sides are generated here
            else
            {
                double deltaTheta   = 360.0 / sides;
                double angle        = 180.0;

                for ( int i = 0 ; i < sides ; i++, angle += deltaTheta )
                {
                    double vertexX  = Math.Sin( angle * DTOR ) * this.Size.X * 0.5;
                    double vertexY  = Math.Cos( angle * DTOR ) * this.Size.Y * 0.5;

                    Vertices.Add( new Point( ( int ) vertexX , ( int ) vertexY ) );
                }
            }

            // Generate an adjacency list that links all vertices to the next, and the last vertex to the first
            for( int i = 0 ; i < this.Vertices.Count - 1 ; i++ )
                this.Edges.Add( new Edge( i , i + 1                     ) );
                this.Edges.Add( new Edge( 0 ,  this.Vertices.Count - 1  ) );
        }



        public override Matrix3x3 CommitScale()
        {
            Matrix3x3 scaleMatrix   = base.CommitScale();
            this.Vertices           = this.GetTransformedVertices( scaleMatrix );
            return scaleMatrix;
        }



        public override void Rasterize()
        {
            // Call the Superclass Function
            base.Rasterize();

            // Merge our transformation matrices into a single transform matrix
            List<PointF> transformedVertices = this.GetTransformedVertices( this.TransformMatrix );

            // Rasterize Lines
            for( int i = 0 ; i < this.Edges.Count ; i++ )
            {
                Edge edge       = this.Edges[i];
                PointF vertexA  = transformedVertices[edge.VertexIdA];
                PointF vertexB  = transformedVertices[edge.VertexIdB];

                RasterTools.RasterizeLine( RasterPixels , vertexB.X , vertexB.Y , vertexA.X , vertexA.Y );
            }
        }



        public override double GetClosestVertex( Point point , double range , out int closestVertexId , out Point result )
        {
            List<PointF> transformedVertices    = this.GetTransformedVertices( this.TransformMatrix );
            double closest                      = range;
            result                              = point;
            closestVertexId                     = 0;

            for( int i = 0 ; i < transformedVertices.Count ; i++ )
            {
                PointF vertex   = transformedVertices[i];
                double distance = Math.Sqrt( Math.Pow( point.X - vertex.X , 2 ) + Math.Pow( point.Y - vertex.Y , 2 ) );

                if( distance < closest )
                {
                    closestVertexId = i;
                    closest         = distance;
                    result          = new Point( ( int ) vertex.X , ( int ) vertex.Y );
                }
            }

            return closest;
        }



        public PointF GetClosestPoint( Point point , out int edgeId )
        {
            List<PointF> transformedVertices    = this.GetTransformedVertices( this.TransformMatrix );
            double closestDistance              = double.PositiveInfinity;
            PointF closestPoint                 = this.Origin;
            edgeId                              = 0;

            for( int i = 0 ; i < this.Edges.Count ; i++ )
            {
                // Use projection to find the closest point. It's shorter, simpler, and faster than calculating the intersection of a perpendicular line.
                Edge edge           = this.Edges[i];
                PointF vertexA      = transformedVertices[edge.VertexIdA];
                PointF vertexB      = transformedVertices[edge.VertexIdB];

                PointF heading      = new PointF( vertexB.X - vertexA.X , vertexB.Y - vertexA.Y );
                PointF projection   = new PointF( point.X - vertexA.X   , point.Y - vertexA.Y   );

                double dotHeading       = ( heading.X * heading.X       ) + ( heading.Y * heading.Y     );
                double dotProjection    = ( heading.X * projection.X    ) + ( heading.Y * projection.Y  );
                double pointRatio       = dotProjection / dotHeading;

                PointF newClosestPoint      = new PointF( ( float )( vertexA.X + heading.X * pointRatio ) , ( float )( vertexA.Y + heading.Y * pointRatio ) );
                double newClosestDistance   = Math.Sqrt( Math.Pow( newClosestPoint.X - point.X , 2 ) + Math.Pow( newClosestPoint.Y - point.Y , 2 ) );

                // Make sure our point is bound by the edge
                if( newClosestDistance < closestDistance && pointRatio >= 0 && pointRatio <= 1 )
                {
                    closestDistance = newClosestDistance;
                    closestPoint    = newClosestPoint;
                    edgeId          = i;
                }
            }

            return closestPoint;
        }
    }
}
