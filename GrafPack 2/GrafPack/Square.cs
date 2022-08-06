using System;
using System.Drawing;

namespace GrafPack
{
    [Serializable]
    class Square : NGon
    {
        public const int sides = 4;

        public Square( Color color ) : base( color ) { this.GenerateNGon( sides ); }
    }
}
