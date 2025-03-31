using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
namespace TalktuahCommunicaterClient;

class Program
{
    const int SCREENWIDTH = 1400;
    const int SCREENHEIGHT = 800;
    const string VERSION = "v1.0";

    private static float time;
    static Font font1, font2;

    private static void DrawText(string text, Font font, Vector2 pos, float size, Color col, float spacing = 1)
    {
        Raylib.DrawTextEx(font, text, pos - Raylib.MeasureTextEx(font, text, size, spacing)/2, size, spacing, col);
    }

    static void Main(string[] args)
    {
        //init stuff
        Raylib.InitWindow(SCREENWIDTH, SCREENHEIGHT, $"Talktuah Communicater Client {VERSION}");
        Raylib.SetTargetFPS(60);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { ChangeWorkingDirectory(); } //fix the issue on mac weird
        Raylib.SetExitKey(KeyboardKey.Null);
        font1 = Raylib.LoadFont("font1.otf");
        font2 = Raylib.LoadFont("font2.otf");

        while (!Raylib.WindowShouldClose())
        {
            time += 1f / 60f;
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Magenta);

            //triangles
            {
                float trianglespinspeed = 2f;
                float trianglesize = 100;
                var centre = new Vector2(SCREENWIDTH / 2, SCREENHEIGHT / 2 + 50);
                Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                centre = new Vector2(SCREENWIDTH / 2 + 150, SCREENHEIGHT / 2 - 300);
                trianglespinspeed *= -1;
                Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                trianglespinspeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 - 320, SCREENHEIGHT / 2 + 100);
                Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed * 6) * trianglesize, (float)Math.Sin(time * trianglespinspeed * 7) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                trianglespinspeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 + 100, SCREENHEIGHT / 2 + 200);
                Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                trianglespinspeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 - 200, SCREENHEIGHT / 2 - 200);
                Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
            }

            //circles
            {
                float maxradius = 100;
                float pulsespeed = 2f;
                var centre = new Vector2(SCREENWIDTH / 2, SCREENHEIGHT / 2 - 200);
                Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);

                pulsespeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 + 100, SCREENHEIGHT / 2 - 87);
                Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);

                pulsespeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 - 150, SCREENHEIGHT / 2 + 120);
                Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);

                pulsespeed *= -1;
                centre = new Vector2(SCREENWIDTH / 2 +271, SCREENHEIGHT / 2 - 304);
                Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);
            }

            DrawText("TALKTUAH COMMUNICATER CLIENT 1925", font2, new Vector2(SCREENWIDTH / 2, 50), 80, Color.Black);

            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    private static unsafe void ChangeWorkingDirectory()
    {
        // Get the AppContext.BaseDirectory as a string
        string baseDirectory = AppContext.BaseDirectory;

        // Allocate unmanaged memory for the sbyte pointer
        int length = baseDirectory.Length;
        sbyte* pointer = (sbyte*)Marshal.AllocHGlobal(length + 1); // +1 for null-terminator

        // Copy the string content to the allocated memory as sbytes
        for (int i = 0; i < length; i++)
        {
            pointer[i] = (sbyte)baseDirectory[i];
        }
        pointer[length] = 0;

        bool success = Raylib.ChangeDirectory(pointer);
        if (!success) { Console.WriteLine("Uh oh! Failed to change the working directory!"); }

        Marshal.FreeHGlobal((IntPtr)pointer);
    }


}

