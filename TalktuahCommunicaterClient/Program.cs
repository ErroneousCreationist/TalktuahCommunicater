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
    static Font font1, font2, font3;

    private enum ScreenEnum { Menu, InChat }
    private static ScreenEnum screen;

    private static void DrawText(string text, Font font, Vector2 pos, float size, Color col, float spacing = 1)
    {
        Raylib.DrawTextEx(font, text, pos - Raylib.MeasureTextEx(font, text, size, spacing)/2, size, spacing, col);
    }

    private static Vector2 RectCentre(Rectangle rect)
    {
        return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
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
        font3 = Raylib.LoadFont("font3.ttf");

        var polchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 120);
        var genchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 190);
        var ranchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 260);
        var usernamepos = new Vector2(SCREENWIDTH / 2 - 250, 330);
        var ippos = new Vector2(SCREENWIDTH / 2 - 150, 400);
        var joinpos = new Vector2(SCREENWIDTH / 2 - 150, 470);

        var rand = new Random();
        List<Vector2> randDirs = new()
        {
            new Vector2(rand.NextSingle(), rand.NextSingle()),
             new Vector2(rand.NextSingle(), rand.NextSingle()),
              new Vector2(rand.NextSingle(), rand.NextSingle()),
               new Vector2(rand.NextSingle(), rand.NextSingle()),
                new Vector2(rand.NextSingle(), rand.NextSingle()),
                 new Vector2(rand.NextSingle(), rand.NextSingle())
        };

        while (!Raylib.WindowShouldClose())
        {
            time += 1f / 60f;
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Magenta);

            switch (screen)
            {
                case ScreenEnum.Menu:
                    {
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
                            trianglespinspeed *= -1;
                            centre = new Vector2(SCREENWIDTH / 2 - 600, SCREENHEIGHT / 2 + 284);
                            Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed * 2.1f) * trianglesize, (float)Math.Sin(time * trianglespinspeed * 2.3f) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                            trianglespinspeed *= -1;
                            centre = new Vector2(SCREENWIDTH / 2 - 493, SCREENHEIGHT / 2 - 195);
                            Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed * 1.1f) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
                            trianglespinspeed *= -0.9f;
                            trianglesize *= 2f;
                            centre = new Vector2(SCREENWIDTH / 2 + 546, SCREENHEIGHT / 2 + 40);
                            Raylib.DrawTriangle(centre + new Vector2((float)Math.Cos(time * trianglespinspeed * 1.3f) * trianglesize, (float)Math.Sin(time * trianglespinspeed) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi / 2) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi / 2) * trianglesize), centre + new Vector2((float)Math.Cos(time * trianglespinspeed - float.Pi) * trianglesize, (float)Math.Sin(time * trianglespinspeed - float.Pi) * trianglesize), Color.Red);
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
                            centre = new Vector2(SCREENWIDTH / 2 + 271, SCREENHEIGHT / 2 - 304);
                            Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);

                            pulsespeed *= -1;
                            centre = new Vector2(SCREENWIDTH / 2 + 424, SCREENHEIGHT / 2 + 85);
                            Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);

                            pulsespeed *= -3;
                            maxradius *= 1.5f;
                            centre = new Vector2(SCREENWIDTH / 2 - 615, SCREENHEIGHT / 2 - 283);
                            Raylib.DrawCircleV(centre, (float)((Math.Sin(time * pulsespeed) + 1) * 0.5f * maxradius), Color.SkyBlue);
                        }

                        //title
                        DrawText("TALKTUAH COMMUNICATER CLIENT 1925", font3, new Vector2(SCREENWIDTH / 2, 50), 70, Color.Black);

                        //political chat
                        {
                            polchatbuttonpos += randDirs[0] / 60;
                            var rect = new Rectangle(polchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Politicool Chat", font1, RectCentre(rect), 35, Color.Black);

                            //todo join!!!
                        }

                        //genreal chat
                        {
                            genchatbuttonpos += randDirs[1] / 60;
                            var rect = new Rectangle(genchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Genreal Chat", font1, RectCentre(rect), 40, Color.Black);

                            //todo JOIN!!!
                        }

                        //randoom chat
                        {
                            ranchatbuttonpos += randDirs[2] / 60;
                            var rect = new Rectangle(ranchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Randoom Chat", font1, RectCentre(rect), 40, Color.Black);

                            //todo JOIN!!!
                        }

                        break;
                    }
            }
            
            Raylib.EndDrawing();
        }

        Raylib.UnloadFont(font1);
        Raylib.UnloadFont(font2);
        Raylib.UnloadFont(font3);
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

