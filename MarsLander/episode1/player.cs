using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static readonly double gravity = 3.711;
    static readonly int maxFinalV = 40;
    static readonly int maxPower = 4;
    static readonly int minPower = 0;
    static void Main(string[] args)
    {
        string[] inputs;
        int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
        int prevLandY = -999;
        int flatHeight = -999;
        double finalV = -999;
        int desiredPower = 0;
        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
            if(prevLandY == landY){
                flatHeight = landY;
            }
            prevLandY = landY;
        }

        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int hSpeed = int.Parse(inputs[2]);
            int vSpeed = int.Parse(inputs[3]); 
            int fuel = int.Parse(inputs[4]); 
            int rotate = int.Parse(inputs[5]); 
            int power = int.Parse(inputs[6]); 
            if(vSpeed < -35) {
                power++;
            }
                Console.WriteLine($"0 {Math.Max(minPower,Math.Min(power,maxPower))}");
            // finalV = Math.Pow(vSpeed, 2) -2 * (Y - flatHeight) * (power - gravity); // based on current power
            // flatHeight - Y
            // if (finalV < -maxFinalV*maxFinalV){
            //     desiredPower = power + 1;
            // }
            // else {
            //     desiredPower = power - 1;
            // }
            // Console.Error.WriteLine(finalV);
            // Console.WriteLine($"0 {Math.Max(minPower,Math.Min(desiredPower,maxPower))}");
        }
    }
}