using Raylib_cs;
using System.Numerics;

namespace SnakeRaylib
{
    public class Snake
    {
        private static int _cellSize = 20;
        private static int _gridSize = 20;
        private static Random _rand = new Random();
        private static double _snakeSpeed = 0.9f;
        private static int _borderWidth = 1;
        private static bool _fadeSnake = true;
        private static int _defaultLength = 1;

        public static List<Vector2> SnakeBody = new List<Vector2>();
        public static int SnakeLength = 1;
        public static Direction SnakeDirection = Direction.Paused;
        public static Vector2 ApplePos = new Vector2();
        public static double MoveTimer = 0.0;
        public static bool MoveDebounce = false;
        public static bool Dead = false;
        public static bool Won = false;

        static void Main(string[] args)
        {
            ReadParameters(args);
            Raylib.SetTraceLogLevel(TraceLogLevel.Fatal);
            Raylib.InitWindow(_cellSize * _gridSize, _cellSize * _gridSize, "Snake Raylib");
            SnakeBody.Add(new Vector2(_rand.Next(_gridSize), _rand.Next(_gridSize)));
            ResetApple();

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                CheckInput();
                MoveSnake();
                UpdateSnake();
                DrawApple();
                DrawSnake();

                if (Dead)
                    DrawDead();
                if (Won)
                    DrawWon();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static void CheckInput()
        {
            if (Dead || Won)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    SnakeBody.Clear();
                    SnakeBody.Add(new Vector2(_rand.Next(_gridSize), _rand.Next(_gridSize)));
                    ResetApple();
                    SnakeLength = _defaultLength;
                    MoveTimer = 0.0;
                    MoveDebounce = false;
                    Dead = false;
                    SnakeDirection = Direction.Paused;
                }
                return;
            }

            if (MoveDebounce)
                return;

            Direction orgDir = SnakeDirection;
            if (Raylib.IsKeyPressed(KeyboardKey.Up) && SnakeDirection != Direction.Down)
                SnakeDirection = Direction.Up;
            if (Raylib.IsKeyPressed(KeyboardKey.Down) && SnakeDirection != Direction.Up)
                SnakeDirection = Direction.Down;
            if (Raylib.IsKeyPressed(KeyboardKey.Left) && SnakeDirection != Direction.Right)
                SnakeDirection = Direction.Left;
            if (Raylib.IsKeyPressed(KeyboardKey.Right) && SnakeDirection != Direction.Left)
                SnakeDirection = Direction.Right;

            if (orgDir != SnakeDirection)
                MoveDebounce = true;
        }

        static void MoveSnake()
        {
            MoveTimer += Raylib.GetFrameTime();
            if (MoveTimer < 1.0 - _snakeSpeed)
                return;
            MoveTimer -= 1.0 - _snakeSpeed;
            Vector2 snakeHead = SnakeBody.Last();
            switch (SnakeDirection)
            {
                case Direction.Up:
                    SnakeBody.Add(new Vector2(snakeHead.X, snakeHead.Y - 1));
                    break;
                case Direction.Down:
                    SnakeBody.Add(new Vector2(snakeHead.X, snakeHead.Y + 1));
                    break;
                case Direction.Left:
                    SnakeBody.Add(new Vector2(snakeHead.X - 1, snakeHead.Y));
                    break;
                case Direction.Right:
                    SnakeBody.Add(new Vector2(snakeHead.X + 1, snakeHead.Y));
                    break;
            }
            MoveDebounce = false;
        }

        static void UpdateSnake()
        {
            bool collidedApple = false;
            bool collidedDeath = false;

            for (int i = 0; i < SnakeBody.Count; i++)
            {
                Vector2 snakeBody = SnakeBody[i];
                if (snakeBody.Equals(ApplePos))
                    collidedApple = true;
                if (snakeBody.X < 0 || snakeBody.X >= _gridSize ||
                    snakeBody.Y < 0 || snakeBody.Y >= _gridSize)
                    collidedDeath = true;
            }

            if (collidedApple)
            {
                SnakeLength++;
                ResetApple();
            }

            if (SnakeLength == _gridSize * _gridSize)
            {
                SnakeDirection = Direction.Paused;
                Won = true;
            }

            for (int i = 0; i < SnakeBody.Count - SnakeLength; i++)
                SnakeBody.RemoveAt(0);

            for (int i = 0; i < SnakeBody.Count; i++)
            {
                Vector2 snakeBody = SnakeBody[i];
                for (int ii = 0; ii < SnakeBody.Count; ii++)
                    if (i != ii && snakeBody.Equals(SnakeBody[ii]))
                        collidedDeath = true;
            }

            if (collidedDeath)
            {
                SnakeDirection = Direction.Paused;
                Dead = true;
            }
        }

        static void DrawSnake()
        {
            for (int i = 0; i < SnakeBody.Count; i++)
            {
                Vector2 snakeBody = SnakeBody[i];
                Rectangle snakeBox = new Rectangle(snakeBody.X * _cellSize + _borderWidth,
                                                   snakeBody.Y * _cellSize + _borderWidth,
                                                   _cellSize - _borderWidth,
                                                   _cellSize - _borderWidth);

                if (_fadeSnake)
                    Raylib.DrawRectangleRec(snakeBox, new Color(0, (int)(255 - ((float)(SnakeBody.Count - 1 - i) / SnakeBody.Count * 200.0f)), 0, 255));
                else
                    Raylib.DrawRectangleRec(snakeBox, new Color(0, 255, 0, 255));
            }
        }

        static void DrawApple()
        {
            Rectangle appleBox = new Rectangle(ApplePos.X * _cellSize + _borderWidth,
                                               ApplePos.Y * _cellSize + _borderWidth,
                                               _cellSize - _borderWidth,
                                               _cellSize - _borderWidth);
            Raylib.DrawRectangleRec(appleBox, Color.Red);
        }

        static void ResetApple()
        {
            while (true)
            {
                ApplePos = new Vector2(_rand.Next(_gridSize), _rand.Next(_gridSize));

                bool collidedApple = false;

                for (int i = 0; i < SnakeBody.Count; i++)
                {
                    Vector2 snakeBody = SnakeBody[i];
                    if (snakeBody.Equals(ApplePos))
                        collidedApple = true;
                }

                if (!collidedApple)
                    break;
            }
        }

        static void DrawDead()
        {
            Raylib.DrawText("You are dead!", Raylib.GetScreenWidth() / 2 - 70, Raylib.GetScreenHeight() / 2 - 20, 20, Color.White);
            Raylib.DrawText("Score: " + SnakeLength, Raylib.GetScreenWidth() / 2 - 40, Raylib.GetScreenHeight() / 2, 20, Color.White);
        }

        static void DrawWon()
        {
            Raylib.DrawText("You won!", Raylib.GetScreenWidth() / 2 - 40, Raylib.GetScreenHeight() / 2 - 20, 20, Color.White);
        }
        static void ReadParameters(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-cellsize":
                        if (int.TryParse(args[++i], out int cellSize))
                            _cellSize = Math.Max(1, cellSize);
                        break;
                    case "-gridsize":
                        if (int.TryParse(args[++i], out int gridSize))
                            _gridSize = Math.Max(1, gridSize);
                        break;
                    case "-bordersize":
                        if (int.TryParse(args[++i], out int borderWidth))
                            _borderWidth = Math.Max(0, borderWidth);
                        break;
                    case "-speed":
                        if (double.TryParse(args[++i], out double snakeSpeed))
                            _snakeSpeed = Math.Clamp(snakeSpeed, 0.0, 1.0);
                        break;
                    case "-dontfadetail":
                        _fadeSnake = false;
                        break;
                    case "-defaultscore":
                        if (int.TryParse(args[++i], out int defaultLength))
                            _defaultLength = Math.Max(1, defaultLength);
                        SnakeLength = _defaultLength;
                        break;
                    case "-fps":
                        if (int.TryParse(args[++i], out int fps))
                            Raylib.SetTargetFPS(Math.Max(1, fps));
                        break;
                    case "-h":
                    case "-help":
                        Console.WriteLine(Help);
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public enum Direction
        {
            Paused = -1,
            Up,
            Down,
            Left,
            Right
        }

        private static string Help =
@"Commands:
`-Help` or `-h` | Shows this menu
`-CellSize`     | Sets the width and height in pixels of each grid space/cell (Default: 20)
`-GridSize`     | Sets the width and height in cells of the grid (Default: 20)
`-BorderSize`   | Sets the thickness in pixels of he black border around each cell (Default: 1)
`-Speed`        | Sets the speed of the snake, 1.0 being every frame, 0.0 being every second (Default: 0.9)
`-DontFadeTail` | Disables the effect that makes the tail cells darker based on length
`-DefaultScore` | Sets the default length of the Snake (Default: 1)
`-FPS`          | Sets the games frame rate (Default: Unlimited)";
    }
}
