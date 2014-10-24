using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace SnakeGame
{
    public class Snake : Form
    {
        private Random r;
        private bool gameStarted, discoSnake;
        private Timer timer;
        private Queue<Piece> snake;
        private Piece food;
        private int canvasSize, lastKey, score;
        Brush[] randBrush = { Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Purple, Brushes.Violet };

        public Snake()
        {
            //Form Setup
            canvasSize = 400;
            this.Size = new Size(canvasSize + 16, canvasSize + 39);
            this.Text = "Snake";
            this.DoubleBuffered = true;
            this.MaximizeBox = false;

            timer = new Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            r = new Random();

            //Form Actions
            this.FormClosed += new FormClosedEventHandler(Snake_FormClosed);
            this.KeyDown += new KeyEventHandler(Snake_KeyDown);
            this.Paint += new PaintEventHandler(Snake_Paint);

            //Game Stuff
            score = 0;
            gameStarted = true;
            discoSnake = false;
            snake = new Queue<Piece>();
            for(int i = 0; i < 4; i++)
            {
                snake.Enqueue(new Piece(200, 200 + (i*10), true, 0));
            }
            food = new Piece(r.Next(1, 10) * 10, r.Next(1,10)* 10, false, 0);
        }

        void Snake_KeyDown(object sender, KeyEventArgs e)
        {
            //Give the command to change the direction
            lastKey = -1;
            if (e.KeyCode == Keys.P)
            {
                if (gameStarted)
                {
                    gameStarted = false;
                }
                else
                {
                    gameStarted = true;
                }
            }
            if(e.KeyCode == Keys.Q && !gameStarted)
            {
                Application.Exit();
            }
            if(e.KeyCode == Keys.M && gameStarted)
            {
                if(discoSnake)
                {
                    discoSnake = false;
                }
                else
                {
                    discoSnake = true;
                }
            }
            if(e.KeyCode == Keys.R && !gameStarted)
            {
                restartGame();
            }
            if(e.KeyCode == Keys.W)
            {
                lastKey = 0;
            }
            if(e.KeyCode == Keys.D)
            {
                lastKey = 1;
            }
            if(e.KeyCode == Keys.S)
            {
                lastKey = 2;
            }
            if(e.KeyCode == Keys.A)
            {
                lastKey = 3;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //Move the pieces
            if (gameStarted)
            {
                for (int i = 0; i < snake.Count; i++)
                {
                    snake.ElementAt(i).move();
                }



                //Change the directions of the pieces
                int newDir = -1;
                if (lastKey == 0 && snake.ElementAt(0).direction != 2)
                {
                    newDir = 0;
                }
                else if (lastKey == 1 && snake.ElementAt(0).direction != 3)
                {
                    newDir = 1;
                }
                else if(lastKey == 2 && snake.ElementAt(0).direction != 0)
                {
                    newDir = 2;
                }
                else if(lastKey == 3 && snake.ElementAt(0).direction != 1)
                {
                    newDir = 3;
                }

                if (newDir != -1)
                {
                    for (int i = 0; i < snake.Count; i++)
                    {
                        Piece curPiece = snake.ElementAt(i);
                        curPiece.addInstruction(new Instruction(curPiece, newDir, i));
                    }
                }

                Invalidate();
            }
        }

        void Snake_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (checkCollision(g))
            {
                return;
            }

            //Draw the Playing Grid
            Brush b = Brushes.Gray;
            if(discoSnake)
            {
                b = randBrush[r.Next(0, randBrush.Length)];
            }
            g.DrawRectangle(new Pen(b), 5, 5, canvasSize-10, canvasSize-10);

            //Draw the Pieces
            for (int i = 0; i < snake.Count; i++)
            {
                DrawPiece(snake.ElementAt(i), g);
            }

            DrawPiece(food, g);
        }

        void DrawPiece(Piece p, Graphics g)
        {
            if (p.isSnakePart)
            {
                Brush b = Brushes.Black;
                if(discoSnake)
                {
                    b = randBrush[r.Next(0,randBrush.Length)];
                }
                g.FillRectangle(b, p.cornerX, p.cornerY, p.size, p.size); 
                g.DrawRectangle(new Pen(Brushes.White), p.cornerX, p.cornerY, p.size, p.size);
            }
            else
            {
                Brush b = Brushes.White;
                if(discoSnake)
                {
                    b = randBrush[r.Next(0, randBrush.Length)];
                }
                g.FillEllipse(b, p.cornerX, p.cornerY, p.size, p.size);
                g.DrawEllipse(new Pen(Brushes.Black), p.cornerX, p.cornerY, p.size, p.size);
            }
        }

        void restartGame()
        {
            score = 0;
            discoSnake = false;
            gameStarted = true;
            snake = new Queue<Piece>();
            for(int i = 0; i < 4; i++)
            {
                snake.Enqueue(new Piece(200, 200 + (i * 10), true, 0));
            }
            food = new Piece(r.Next(1, 10) * 10, r.Next(1, 10) * 10, false, 0);
            timer.Start();
        }

        bool checkCollision(Graphics g)
        {
            int x = snake.ElementAt(0).x;
            int y = snake.ElementAt(0).y;

            if (food.x == x && food.y == y)
            {
                eatFood();
                return false;
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (snake.ElementAt(i).x == x && snake.ElementAt(i).y == y)
                {
                    gameOver(g);
                    return true;
                }
            }

            if (x < 10 || x > canvasSize - 10 || y < 10 || y > canvasSize - 10)
            {
                gameOver(g);
                return true;
            }
            return false;
        }

        void eatFood()
        {
            score++;
            int newx = r.Next(1, canvasSize / 10);
            int newy = r.Next(1, canvasSize / 10);
            food.setPos(newx * 10, newy * 10);

            Piece last = snake.ElementAt(snake.Count-1);
            switch(last.direction)
            {
                //UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3
                case 0:
                    newx = last.x;
                    newy = last.y + 10;
                    break;
                case 1:
                    newx = last.x - 10;
                    newy = last.y;
                    break;
                case 2:
                    newx = last.x;
                    newy = last.y - 10;
                    break;
                case 3:
                    newx = last.x + 10;
                    newy = last.y;
                    break;
            }

            Piece p = new Piece(newx, newy, true, last.direction);
            copyAndDelayInstr(last, p);
            snake.Enqueue(p);
            
        }

        public void gameOver(Graphics g)
        {
            gameStarted = false;
            score = score * 10;

            string str = String.Format("      Game over.\n      Score : {0:000}\nPress 'R' to Restart.", score);
            g.DrawString(str, new Font("Microsoft Sans Serif", 24f), Brushes.Black, 60, 140);
            timer.Stop();
        }

        public void copyAndDelayInstr(Piece from, Piece to)
        {
            List<Instruction> fromList = from.instructions;
            Instruction instr;
            for(int i = 0; i < fromList.Count; i++)
            { 
                instr = new Instruction(to, fromList.ElementAt(i).direction, fromList.ElementAt(i).count + 1);
                to.addInstruction(instr);
            }
        }

        static void Main()
        {
            Application.Run(new Snake());
        }

        void Snake_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }

    public class Piece
    {
        public bool isSnakePart, isFood;
        public int cornerX, cornerY, x, y, direction, size; //DIR: UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3
        public List<Instruction> instructions;

        public Piece(int x, int y, bool snakePart, int direction)
        {
            size = 10;
            this.x = x;
            this.y = y;
            this.direction = direction;
            instructions = new List<Instruction>();

            if (snakePart)
            {
                this.isSnakePart = true;
                this.isFood = false;
            }
            else
            {
                this.isSnakePart = false;
                this.isFood = true;
            }

            cornerX = x - (size/2);
            cornerY = y - (size/2);
        }

        public void addInstruction(Instruction i)
        {
            instructions.Add(i);
        }

        public void move()
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                instructions.ElementAt(i).tick();
                if (instructions.ElementAt(i).finished)
                {
                    instructions.RemoveAt(i);
                    i--;
                }
            }
            if (isSnakePart)
            {
                if (direction == 0) { y -= size; }
                if (direction == 2) { y += size; }
                if (direction == 1) { x += size; }
                if (direction == 3) { x -= size; }
            }

            cornerX = x - (size / 2);
            cornerY = y - (size / 2);
        }

        public void setPos(int x, int y)
        {
            this.x = x;
            this.y = y;

            cornerX = x - (size / 2);
            cornerY = y - (size / 2);
        }
    }

    public class Instruction
    {
        public int direction;
        public int count;
        public Piece p;
        public bool finished;

        public Instruction(Piece p, int direction, int count)
        {
            finished = false;
            this.direction = direction;
            this.count = count;
            this.p = p;
        }

        public void tick()
        {
            if (count == 0)
            {
                p.direction = direction;
                finished = true;
            }
            count--;
        }
    }
}