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
                    SnakeLength = 1;
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
            while (true)
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
                    for (int ii = 0; ii < SnakeBody.Count; ii++)
                        if (i != ii && snakeBody.Equals(SnakeBody[ii]))
                            collidedDeath = true;
                }

                if (collidedDeath)
                {
                    SnakeDirection = Direction.Paused;
                    Dead = true;
                }

                if (collidedApple)
                {
                    SnakeLength++;
                    ResetApple();
                }
                else
                    break;
            }

            if (SnakeLength == _gridSize * _gridSize)
            {
                SnakeDirection = Direction.Paused;
                Won = true;
            }

            for (int i = 0; i < SnakeBody.Count - SnakeLength; i++)
                SnakeBody.RemoveAt(0);
        }

        static void DrawSnake()
        {
            foreach (Vector2 snakeBody in SnakeBody)
            {
                Rectangle snakeBox = new Rectangle(snakeBody.X * _cellSize + _borderWidth,
                                                   snakeBody.Y * _cellSize + _borderWidth,
                                                   _cellSize - _borderWidth,
                                                   _cellSize - _borderWidth);
                Raylib.DrawRectangleRec(snakeBox, Color.Green);
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
            ApplePos = new Vector2(_rand.Next(_gridSize), _rand.Next(_gridSize));
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

        public enum Direction
        {
            Paused = -1,
            Up,
            Down,
            Left,
            Right
        }
    }
}
