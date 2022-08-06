using System;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    class Triangle : NGon
    {
        public const int sides = 3;

        public Triangle( Color color ) : base( color ) { this.GenerateNGon( sides ); }
    }
}
