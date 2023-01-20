using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Treat
    {
        Position position;
        char c;

        public Treat(Position position, char c)
        {
            this.position = position;
            this.c = c;
        }

        public Position GetPosition()
        { return position; }
    }
}
