using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
#if DEBUG
using Newtonsoft.Json;
#endif

namespace GeneticAlgorithm
{

    class Program
    {
        static void Main(string[] args)
        {
            Game.ReadMap();
            Action action;
            State state = null;
            while (true)
            {
                Game.ReadTurn();
                if(Game.State.Turn > 0)
                {
                    Console.Error.WriteLine(state);
                }
                else
                {
                    state = Game.State;
                }

                if (Game.State.Vy < -35)
                {
                    Game.State.Power++;
                }
                action = new Action(0,Game.State.Power);
                state = State.Update(state, ActionType.Simulate, action);
                Game.Action = action;
                Console.Error.WriteLine(Game.HasLanded(state));
                State.Update(Game.State, ActionType.Send);
            }
        }
    }


    public static class Game
    {
        public static readonly double gravity = -3.711;
        public static readonly int maxFinalVerticalV = 40;
        public static readonly int maxFinalHorizontalV = 20;
        public static readonly int finalRotation = 0;
        public static readonly int maxPower = 4;
        public static readonly int minPower = 0;
        public static readonly int maxRotation = 90;
        public static readonly int minRotation = -90;
        public static int surfaceN;
        public static Line[] Land;
        public static Line LandingZone;
        public static State State = new State();
        public static Action Action;
        public const bool DUMP = true;
        
        public static void ReadMap()
        {
#if DEBUG
            Land = JsonConvert.DeserializeObject<Line[]>(File.ReadAllText("map.json"));
            surfaceN = Land.Length + 1;
            LandingZone = Land.Single(l => l.IsLandingZone);
#else
            string[] inputs;
            surfaceN = int.Parse(Console.ReadLine());
            Land = new Line[surfaceN - 1];
            int previousX = default;
            int previousY = default;
            for (int i = 0; i < surfaceN; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int currentX = int.Parse(inputs[0]);
                int currentY = int.Parse(inputs[1]);
                if(i > 0)
                {
                    Land[i - 1] = new Line(previousX, previousY, currentX, currentY);
                    if (Land[i - 1].IsLandingZone) LandingZone = Land[i - 1];
                }
                previousX = currentX;
                previousY = currentY;
            }
            if (DUMP)
            {
                Console.Error.WriteLine(Land.ToString<Line>());
            }
#endif

        }
        public static void ReadTurn()
        {
#if DEBUG
            State = JsonConvert.DeserializeObject<State>(File.ReadAllText("state.json"));
#else
            State = State.Update(State, ActionType.ReadInput);
            Console.Error.WriteLine(State.ToString());
#endif
        }

        public static Status HasLanded(State state)
        {
            if(state.X < 0) return Status.Crashed;
            if(state.X >= 7000) return Status.Crashed;
            if(state.Y < 0) return Status.Crashed;
            if(state.Y >= 3000) return Status.Crashed;
            if (LandingZone.IsColliding(state.prevX, state.prevY, state.X, state.Y))
            {
                if (Math.Abs(state.Vx) > Game.maxFinalHorizontalV) return Status.LandBadSpeed;
                if (Math.Abs(state.Vy) > Game.maxFinalVerticalV) return Status.LandBadSpeed;
                if (Math.Abs(state.Angle) > Game.finalRotation) return Status.LandBadAngle;
                return Status.Landed;
            }
            for(int i = 0; i < surfaceN - 1; i++)
            {
                if(Land[i].IsColliding(state.prevX, state.prevY, state.X, state.Y))
                {
                    return Status.Crashed;
                }
            }
            return Status.InProgress;
        }

    }

    public class State
    {
        public int Turn;
        public double prevX, prevY, X, Y, Vx, Vy;
        public int Angle, Fuel, Power;
        public double Ax => Helpers.SinDeg(Angle) * Power;
        public double Ay => (Helpers.CosDeg(Angle) * Power) + Game.gravity;

        public override string ToString()
        {
            return $"{{Turn:{Turn},prevX:{prevX},prevY:{prevY},X:{X},Y:{Y},Vx:{Vx},Vy:{Vy},Angle:{Angle},Fuel:{Fuel},Power:{Angle}}}";
        }
        public static State Update(State state, ActionType type, Action? action = null)
        {
            switch (type)
            {
                case ActionType.ReadInput:
                    return ReadInput(state);
                case ActionType.Send:
                    return Send(state);
                case ActionType.Simulate:
                    return Simulate(state, action.Value);
                default:
                    throw new Exception("Action not defined: " + type);
            }
        }
        public static State ReadInput(State state)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            State newState = new State()
            {
                X = int.Parse(inputs[0]),
                Y = int.Parse(inputs[1]),
                Vx = int.Parse(inputs[2]),
                Vy = int.Parse(inputs[3]),
                Fuel = int.Parse(inputs[4]),
                Angle = int.Parse(inputs[5]),
                Power = int.Parse(inputs[6])
            };
            newState.Turn = state.Turn;
            if(state.Turn == 0)
            {
                newState.prevX = newState.X;
                newState.prevY = newState.Y;
            }
            else
            {
                newState.prevX = state.X;
                newState.prevY = state.Y;
            }
            return newState;
        }
        public static State Send(State state)
        {
            Console.WriteLine(Game.Action);
            state.Turn++;
            return state;
        }
        public static State Simulate(State state, Action action)
        {
            if (action.Power > state.Power) state.Power = Helpers.ClampPower(state.Power + 1);
            if (action.Power < state.Power) state.Power = Helpers.ClampPower(state.Power - 1);
            if (action.Angle > state.Angle) state.Angle = Helpers.ClampAngle(state.Angle + 15);
            if (action.Angle < state.Angle) state.Angle = Helpers.ClampAngle(state.Angle - 15);
            state.Fuel -= state.Power;
            state.prevX = state.X;
            state.prevY = state.Y;
            state.X += state.Vx + state.Ax * 0.5;
            state.Y += state.Vy + state.Ay * 0.5;
            state.Vx += state.Ax;
            state.Vy += state.Ay;
            state.Turn++;
            return state;
        }
    }

    public struct Action
    {
        public int Angle;
        public int Power;
        public Action(int angle, int power)
        {
            Angle = Helpers.ClampAngle(angle);
            Power = Helpers.ClampPower(power);
        }
        public override string ToString()
        {
            return $"{Angle} {Power}";
        }
    }

    public enum ActionType
    {
        ReadInput,
        Send,
        Simulate
    }

    public enum Status
    {
        InProgress,
        Crashed,
        Landed,
        LandBadSpeed,
        LandBadAngle
    }

    public class Line
    {
        public int LeftX;
        public int LeftY;
        public int RightX;
        public int RightY;
        public int SlopeX;
        public int SlopeY;
        public bool IsLandingZone;
        public Line(int x0, int y0, int x1, int y1)
        {
            LeftX = x0;
            LeftY = y0;
            RightX = x1;
            RightY = y1;
            SlopeY = y1 - y0;
            SlopeX = x1 - x0;
            IsLandingZone = SlopeY == 0;
        }
        public bool IsColliding(double prevX, double prevY, double currentX, double currentY)
        {
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#cite_note-GGIII-2
            double denominator = -SlopeY * (currentX - prevX) + SlopeX * (currentY - prevY);
            double alpha = -(currentY - prevY) * (LeftX - prevX) + (currentX - prevX) * (LeftY - prevY);
            double beta = SlopeX * (LeftY - prevY) - SlopeY * (LeftX - prevX);
            if (denominator == 0) return false;
            if(denominator > 0)
            {
                if (alpha < 0 || alpha > denominator) return false;
                if (beta < 0 || beta > denominator) return false;
            }else
            {
                if (alpha > 0 || alpha < denominator) return false;
                if (beta > 0 || beta < denominator) return false;
            }
            return true;
        }
        public override string ToString()
        {
            return $"{{LeftX:{LeftX},LeftY:{LeftY},RightX:{RightX},RightY:{RightY},SlopeX:{SlopeX},SlopeY:{SlopeY},IsLandingZone:{IsLandingZone}}}";
        }

    }

    public static class Extensions
    {
        public static string ToString<T>(this T[] element)
        {
            return $"[{string.Join(",", element)}]";
        }
        public static string ToString<T>(this List<T> element)
        {
            return $"[{string.Join(",", element)}]";
        }
        public static string ToString<T>(this Dictionary<int, T> element)
        {
            return $"{{{string.Join(",", element.Select(kv => $"'{kv.Key}':{kv.Value}"))}}}";
        }
        public static int[] DeepCopy(this int[] original)
        {
            return original.Select(elem => elem).ToArray();
        }
        public static T[] Clone<T>(this T[] original) where T : class, ICloneable<T>
        {
            return original.Select(elem => elem?.Clone()).ToArray();
        }
        public static Dictionary<int, T> Clone<T>(this Dictionary<int, T> original) where T : class, ICloneable<T>
        {
            return new Dictionary<int, T>(original.Select(kv => new KeyValuePair<int, T>(kv.Key, kv.Value.Clone())));
        }
        public static List<T> Clone<T>(this List<T> original) where T : class, ICloneable<T>
        {
            return original.Select(elem => elem?.Clone()).ToList();
        }
        public static bool In<T>(this T elem, params T[] args)
        {
            return args.Contains(elem);
        }
    }

    public static class Helpers
    {
        public static double CosDeg(double angle)
        {
            return Math.Cos(angle * Math.PI / 180);
        }
        public static double SinDeg(double angle)
        {
            return Math.Sin(angle * Math.PI / 180);
        }
        public static int ClampAngle(int angle)
        {
            return Math.Max(Math.Min(angle, Game.maxRotation), Game.minRotation);
        }
        public static int ClampPower(int power)
        {
            return Math.Max(Math.Min(power, Game.maxPower), Game.minPower);
        }
    }

    public interface ICloneable<T>
    {
        T Clone();
    }
}
