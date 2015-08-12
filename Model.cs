using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SMEditor
{
    [Serializable()]
    abstract class Figure
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    [Serializable()]
    class State : Figure
    {
        public int Name { get; set; }

        public bool isStartState;
        public bool isFinalState;

        public State(int X_Coord, int Y_Coord, int StateName)
        {
            X = X_Coord;
            Y = Y_Coord;
            Name = StateName;
            isStartState = false;
            isFinalState = false;
        }

        public Rectangle getBounds()
        {
            return new Rectangle(new Point(X - 15, Y - 15), new Size(30, 30));
        }
    }

    [Serializable()]
    class InfPoint : Figure
    {
        public InfPoint(int X_Coord, int Y_Coord)
        {
            X = X_Coord;
            Y = Y_Coord;
        }
        public Rectangle getBounds()
        {
            return new Rectangle(new Point(X - 5, Y - 5), new Size(10, 10));
        }
    }

    [Serializable()]
    class Transition
    {
        public State Start { get; set; }
        public State End { get; set; }
        public string Name { get; set; }

        public List<InfPoint> InfList;

        public Transition(State StartState, State EndState, string TransitionName)
        {
            Start = StartState;
            End = EndState;
            Name = TransitionName;
            InfList = new List<InfPoint>();
        }
    }
}
