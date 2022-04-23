namespace PowerOfThor
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            Thor thor = new Thor(int.Parse(inputs[0]), int.Parse(inputs[1]));
            List<Giant> giants = new List<Giant>();

            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int H = int.Parse(inputs[0]);
                int N = int.Parse(inputs[1]);
                for (int i = 0; i < N; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int X = int.Parse(inputs[0]);
                    int Y = int.Parse(inputs[1]);
                    giants.Add(new Giant(X, Y));
                }

                if (giants.Any((giant) => giant.IsStrikeable(thor.X, thor.Y)))
                {
                    Console.WriteLine("STRIKE");
                }
                else
                {
                    Console.WriteLine("WAIT");
                }
                giants = new List<Giant>();
            }
        }
        
    }
    public abstract class Entity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Entity(int x, int y)
        {
            X = x;
            Y = y;
        }
        public string Move(int toX, int toY)
        {
            string dir = "";
            if (Y > toY)
            {
                dir += "N";
                Y--;
            }
            else if (Y < toY)
            {
                dir += "S";
                Y++;
            }
            if (X > toX)
            {
                dir += "W";
                X--;
            }
            else if (X < toX)
            {
                dir += "E";
                X++;
            }
            return dir;
        }
    }
    public class Thor : Entity
    {
        public Thor(int x, int y): base(x, y) { }
    }
    public class Giant: Entity
    {
        public Giant(int x, int y) : base(x, y) { }
        public bool IsStrikeable(int x, int y)
        {
            return X.Between(x - Constants.StrikeSize, x + Constants.StrikeSize) &&
            Y.Between(y - Constants.StrikeSize, y + Constants.StrikeSize);
        }
    }
    public static class Extensions
    {
        public static bool Between(this int value, int lowerBound, int upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }
    }
    public static class Constants
    {
        public const int StrikeSize = 5;

    }
}
