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
    static string currUsername = "", currMessage= "";
    static string currIP = "", currPort = "";

    public static Dictionary<string, byte[]> EMOJIS = new();

    private struct ChatMessage
    {
        public string Sender;
        public string Message;
        public Texture2D Image;
        public bool IsImage { get { return string.IsNullOrEmpty(Message); } }

        public ChatMessage(string sender, string message)
        {
            Sender = sender;
            Message = message;
        }

        public ChatMessage(string sender, byte[] image)
        {
            Sender = sender;
            Message = "";
            var temp = Raylib.LoadImageFromMemory(".png", image); //if its not png then KYS lmaooooo
            Image = Raylib.LoadTextureFromImage(temp);
            Raylib.UnloadImage(temp);
        }
    }

    /// <summary>
    /// This is centred :3
    /// </summary>
    private static void DrawText(string text, Font font, Vector2 pos, float size, Color col, float spacing = 1)
    {
        Raylib.DrawTextEx(font, text, pos - Raylib.MeasureTextEx(font, text, size, spacing)/2, size, spacing, col);
    }

    private static Vector2 RectCentre(Rectangle rect)
    {
        return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    }

    private static Vector2 Rotate(Vector2 v, double radians)
    {
        return new Vector2(
            (float)(v.X * Math.Cos(radians) - v.Y * Math.Sin(radians)),
            (float)(v.X * Math.Sin(radians) + v.Y * Math.Cos(radians))
        );
    }

    private static List<char> GetCharsPressed
    {
        get
        {
            List<char> returned = new();
            while (true)
            {
                int i = Raylib.GetCharPressed();
                if (i == 0) { break; }
                returned.Add((char)i);
            }
            return returned;
        }
    }

    private static List<ChatMessage> MESSAGES = new();

    static void Main(string[] args)
    {
        //init stuff
        Raylib.InitWindow(SCREENWIDTH, SCREENHEIGHT, $"Talktuah Communicater Client {VERSION}");
        Raylib.SetTargetFPS(60);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { ChangeWorkingDirectory(); } //fix the issue on mac weird
        Raylib.SetExitKey(KeyboardKey.Null);

        //load resources
        font1 = Raylib.LoadFont("resources/font1.otf");
        font2 = Raylib.LoadFont("resources/font2.otf");
        font3 = Raylib.LoadFont("resources/font3.ttf");
        EMOJIS = new();
        foreach (var item in Directory.EnumerateFiles(AppContext.BaseDirectory + "/resources/emojis"))
        {
            if (EMOJIS.ContainsKey(Path.GetFileName(item).Split(".")[0].ToLower().Replace(" ", "").Replace("-", ""))) { continue; }
            //Console.WriteLine(item + " \\ "+ )
            EMOJIS.Add(Path.GetFileName(item).Split(".")[0].ToLower().Replace(" ", "").Replace("-", ""), File.ReadAllBytes(item)); //add the emoji name to the list
        }

        int currentScroll = 0;

        //events
        NetworkManager.ImageMessageRecieved += (a, b) => { MESSAGES.Insert(0,new ChatMessage(a, b)); };
        NetworkManager.TextMessageRecieved += (a, b) => { MESSAGES.Insert(0,new ChatMessage(a, b)); };
        NetworkManager.OnConnected += () => { screen = ScreenEnum.InChat; MESSAGES = new(); currentScroll = 0; currMessage = ""; };
        NetworkManager.OnDisconnected += () => { screen = ScreenEnum.Menu; MESSAGES = new(); currentScroll = 0; currMessage = ""; };

        //menu shit
        var polchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 120);
        var genchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 190);
        var ranchatbuttonpos = new Vector2(SCREENWIDTH / 2 - 150, 260);
        var usernamepos = new Vector2(SCREENWIDTH / 2 - 250, 330);
        var ippos = new Vector2(SCREENWIDTH / 2 - 150, 400);
        var portpos = new Vector2(SCREENWIDTH / 2 - 150, 470);
        var joinpos = new Vector2(SCREENWIDTH / 2 - 150, 540);
        int selectedfieldindex = -1;

        var rand = new Random();
        List<Vector2> randDirs = new()
        {
            new Vector2(rand.NextSingle(), rand.NextSingle()),
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
                            polchatbuttonpos += randDirs[0] * 30f / 60;
                            if(polchatbuttonpos.X + 300 > SCREENWIDTH || polchatbuttonpos.X < 0 || polchatbuttonpos.Y + 50 > SCREENHEIGHT || polchatbuttonpos.Y < 0) { randDirs[0] = Rotate(randDirs[0], double.Pi/2); }
                            var rect = new Rectangle(polchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Join Politicool", font1, RectCentre(rect), 35, Color.Black);

                            //todo join!!!
                        }

                        //genreal chat
                        {
                            genchatbuttonpos += randDirs[1] * 30f/ 60;
                            if (genchatbuttonpos.X + 300 > SCREENWIDTH || genchatbuttonpos.X < 0 || genchatbuttonpos.Y + 50 > SCREENHEIGHT || genchatbuttonpos.Y < 0) { randDirs[1] = Rotate(randDirs[1], double.Pi/2); }
                            var rect = new Rectangle(genchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Join Genreal", font1, RectCentre(rect), 40, Color.Black);

                            //todo JOIN!!!
                        }

                        //randoom chat
                        {
                            ranchatbuttonpos += randDirs[2] * 30f / 60;
                            if (ranchatbuttonpos.X + 300 > SCREENWIDTH || ranchatbuttonpos.X < 0 || ranchatbuttonpos.Y + 50 > SCREENHEIGHT || ranchatbuttonpos.Y < 0) { randDirs[2] = Rotate(randDirs[2], double.Pi/2); }
                            var rect = new Rectangle(ranchatbuttonpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Join Randoom", font1, RectCentre(rect), 40, Color.Black);

                            //todo JOIN!!!
                        }

                        //username field
                        {
                            usernamepos += randDirs[3] * 30f / 60;
                            if (usernamepos.X + 500 > SCREENWIDTH || usernamepos.X < 0 || usernamepos.Y + 50 > SCREENHEIGHT || usernamepos.Y < 0) { randDirs[3] = Rotate(randDirs[3], double.Pi/2); }
                            var rect = new Rectangle(usernamepos, 500, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering||selectedfieldindex==0 ? Color.Lime : Color.Black);
                            if(currUsername == "") { DrawText("enter username...", font1, RectCentre(rect), 40, Color.Black); }
                            else { DrawText(currUsername, font3, RectCentre(rect), 40, Color.Black); }
                            if (hovering && Raylib.IsMouseButtonPressed(MouseButton.Left)) { selectedfieldindex = selectedfieldindex == 0 ?  -1 :  0; }

                            if(selectedfieldindex == 0)
                            {
                                var stuff = GetCharsPressed;
                                if (stuff.Count > 0) { currUsername += new string(stuff.ToArray()); }
                                if (Raylib.IsKeyPressed(KeyboardKey.Enter)) { selectedfieldindex = -1; }
                                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && currUsername.Length > 0) { currUsername = currUsername[..^1]; }
                                if ((Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)) && Raylib.IsKeyPressed(KeyboardKey.V)) { currUsername += Raylib.GetClipboardText_(); }
                            }
                        }

                        //IP field
                        {
                            ippos += randDirs[4] * 30f / 60;
                            if (ippos.X + 300 > SCREENWIDTH || ippos.X < 0 || ippos.Y + 50 > SCREENHEIGHT || ippos.Y < 0) { randDirs[4] = Rotate(randDirs[4], double.Pi/2); ; }
                            var rect = new Rectangle(ippos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering || selectedfieldindex == 1 ? Color.Lime : Color.Black);
                            if (currIP == "") { DrawText("enter IP...", font1, RectCentre(rect), 35, Color.Black); }
                            else { DrawText(currIP, font3, RectCentre(rect), 35, Color.Black); }
                            if (hovering && Raylib.IsMouseButtonPressed(MouseButton.Left)) { selectedfieldindex = selectedfieldindex == 1 ? -1 : 1; }

                            if (selectedfieldindex == 1)
                            {
                                var stuff = GetCharsPressed;
                                if (stuff.Count > 0) { currIP += new string(stuff.ToArray()); }
                                if (Raylib.IsKeyPressed(KeyboardKey.Enter)) { selectedfieldindex = -1; }
                                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && currIP.Length > 0) { currIP = currIP[..^1]; }
                                if((Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)) && Raylib.IsKeyPressed(KeyboardKey.V)) { currIP += Raylib.GetClipboardText_(); }
                            }
                        }

                        //Port field
                        {
                            portpos += randDirs[6] * 30f / 60;
                            if (portpos.X + 300 > SCREENWIDTH || portpos.X < 0 || portpos.Y + 50 > SCREENHEIGHT || portpos.Y < 0) { randDirs[6] = Rotate(randDirs[6], double.Pi / 2); ; }
                            var rect = new Rectangle(portpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering || selectedfieldindex == 2 ? Color.Lime : Color.Black);
                            if (currPort == "") { DrawText("enter port...", font1, RectCentre(rect), 35, Color.Black); }
                            else { DrawText(currPort, font3, RectCentre(rect), 35, Color.Black); }
                            if (hovering && Raylib.IsMouseButtonPressed(MouseButton.Left)) { selectedfieldindex = selectedfieldindex == 1 ? -1 : 2; }

                            if (selectedfieldindex == 2)
                            {
                                var stuff = GetCharsPressed;
                                if (stuff.Count > 0) { currPort += new string(stuff.ToArray()); }
                                if (Raylib.IsKeyPressed(KeyboardKey.Enter)) { selectedfieldindex = -1; }
                                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && currPort.Length > 0) { currPort = currPort[..^1]; }
                                if ((Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)) && Raylib.IsKeyPressed(KeyboardKey.V)) { currPort += Raylib.GetClipboardText_(); }
                            }
                        }

                        //join custom
                        {
                            joinpos += randDirs[5] * 30f / 60;
                            if (joinpos.X + 300 > SCREENWIDTH || joinpos.X < 0 || joinpos.Y + 50 > SCREENHEIGHT || joinpos.Y < 0) { randDirs[5] = Rotate(randDirs[5], double.Pi / 2); }
                            var rect = new Rectangle(joinpos, 300, 50);
                            bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                            Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                            Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                            DrawText("Join Custom", font1, RectCentre(rect), 40, Color.Black);

                            if(Raylib.IsMouseButtonPressed(MouseButton.Left) && hovering)
                            {
                                JoinRoom(currIP, currPort);
                            }
                        }

                        break;
                    }
                case ScreenEnum.InChat:
                    {
                        //draw messages
                        int currHeight = 0;
                        int finalheight = 0;
                        for (int i = 0; i < MESSAGES.Count; i++)
                        {
                            var message = MESSAGES[i];
                            var bottomcorner = new Vector2(50, SCREENHEIGHT - 70 - currHeight + currentScroll); //bottom left
                            //if(bottomcorner.Y <= 0) { continue; } //dont draw if we dont need to
                            var usernamesize = Raylib.MeasureTextEx(font3, message.Sender, 30, 1);

                            if (message.IsImage)
                            {
                                Raylib.DrawTriangle(new Vector2(bottomcorner.X - 5, bottomcorner.Y + 5), new Vector2(bottomcorner.X - 5, bottomcorner.Y - 25), new Vector2(20, bottomcorner.Y - 10), Color.Lime);
                                Raylib.DrawRectangle((int)bottomcorner.X - 5, (int)bottomcorner.Y - message.Image.Height - (int)usernamesize.Y - 5, (int)MathF.Max(message.Image.Width, usernamesize.X) + 10, (int)usernamesize.Y + message.Image.Height + 10, Color.Lime);
                                Raylib.DrawTexture(message.Image, 50, (int)bottomcorner.Y - message.Image.Height, Color.White);
                                Raylib.DrawTextEx(font3, message.Sender, new Vector2(50, bottomcorner.Y - message.Image.Height - usernamesize.Y), 30, 1, Color.Black);
                                currHeight += message.Image.Height + (int)usernamesize.Y + 30;
                                if(i == MESSAGES.Count - 1) { finalheight = message.Image.Height + (int)usernamesize.Y + 30; }
                            }
                            else
                            {
                                var messagesize = Raylib.MeasureTextEx(font3, message.Message, 30, 1);

                                Raylib.DrawTriangle(new Vector2(bottomcorner.X - 5, bottomcorner.Y + 5), new Vector2(bottomcorner.X - 5, bottomcorner.Y - 25), new Vector2(20, bottomcorner.Y - 10), Color.Lime);
                                Raylib.DrawRectangle((int)bottomcorner.X - 5, (int)bottomcorner.Y - (int)messagesize.Y - (int)usernamesize.Y - 5, (int)MathF.Max(messagesize.X, usernamesize.X)+10, (int)usernamesize.Y+(int)messagesize.Y+10, Color.Lime);
                                Raylib.DrawTextEx(font3, message.Sender, new Vector2(50, bottomcorner.Y - messagesize.Y - usernamesize.Y), 30, 1, Color.Black);
                                Raylib.DrawTextEx(font3, message.Message, new Vector2(50, bottomcorner.Y - messagesize.Y), 30, 1, Color.Black);
                                currHeight += (int)messagesize.Y + (int)usernamesize.Y + 30;
                                if (i == MESSAGES.Count - 1) { finalheight = (int)messagesize.Y + (int)usernamesize.Y + 30; }
                            }
                        }

                        //scrolling shit
                        float temp = currentScroll;
                        temp += Raylib.GetMouseWheelMoveV().Y * 20;
                        if (SCREENHEIGHT - currHeight + (int)MathF.Round(temp) < finalheight && (int)MathF.Round(temp) > 0) { currentScroll = (int)MathF.Round(temp); }

                        //text box rendering
                        Raylib.DrawRectangle(5, SCREENHEIGHT - 60, SCREENWIDTH - 10, 60, Color.LightGray);
                        if(currMessage == "") { Raylib.DrawTextEx(font3, "enter text...", new Vector2(5, SCREENHEIGHT - 65), 50, 1, Color.Black); }
                        else {
                            var split = currMessage.Split('\n');
                            if (split.Length == 1) { Raylib.DrawTextEx(font3, currMessage, new Vector2(5, SCREENHEIGHT - 65), 50, 1, Color.Black); }
                            else { Raylib.DrawTextEx(font3, split[^1], new Vector2(5, SCREENHEIGHT - 65), 50, 1, Color.Black); }
                        }

                        //text input
                        var stuff = GetCharsPressed;
                        if (stuff.Count > 0) { currMessage += new string(stuff.ToArray()); }
                        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) {
                            if (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift)) { currMessage += "\n"; }
                            //send the actual message
                            else { NetworkManager.SendMessage?.Invoke(currMessage); currMessage = ""; }
                        }
                        if (Raylib.IsKeyDown(KeyboardKey.Backspace) && currMessage.Length > 0) { currMessage = currMessage[..^1]; }
                        if ((Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)) && Raylib.IsKeyPressed(KeyboardKey.V)) { currMessage += Raylib.GetClipboardText_(); }

                        //send image
                        //{
                        //    var rect = new Rectangle(new Vector2(SCREENWIDTH - 110, SCREENHEIGHT - 50), 100, 40);
                        //    bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                        //    Raylib.DrawRectangleRounded(rect, 5, 5, Color.LightGray);
                        //    Raylib.DrawRectangleRoundedLinesEx(rect, 5, 5, 10, hovering ? Color.Lime : Color.Black);
                        //    DrawText("Send\nImage", font1, RectCentre(rect), 22, Color.Black);

                        //    if(hovering && Raylib.IsMouseButtonPressed(MouseButton.Left))
                        //    {

                        //    }
                        //}
                        var files = Raylib.GetDroppedFiles();
                        if (files.Length > 0) {
                            if (Path.GetExtension(files[0]) == "png" || Path.GetExtension(files[0]) == ".png")
                            {
                                NetworkManager.SendImage?.Invoke(File.ReadAllBytes(files[0]));
                            }
                        }
                        break;
                    }
            }
            Raylib.EndDrawing();
        }
        Raylib.UnloadFont(font1);
        Raylib.UnloadFont(font2);
        Raylib.UnloadFont(font3);
        NetworkManager.Close?.Invoke();
        Raylib.CloseWindow();
    }

    private static void JoinRoom(string ip, string port)
    {
        if (!int.TryParse(port, out int finalport)) { return; }

        NetworkManager _ = new(ip, finalport, currUsername);
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

