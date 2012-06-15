/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /RenderIdle.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * When the terminal is idle, the following class is responsible for rendering additional
 * things for entertainment; currently this entertainment is simple ball collisions on
 * the screen with random colours, numbers and velocities.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UberMediaServer
{
    public class RenderIdle
    {
        #region "Variables"
        private Main main;
        private const int BALLS_MIN = 20;
        private const int BALLS_MAX = 30;
        private const int BALLS_VEL_MIN = 1;
        private const int BALLS_VEL_MAX = 8;
        public int canvasWidth;
        public int canvasHeight;
        private List<Ball> balls;
        #endregion

        #region "Methods - Constructors"
        public RenderIdle(Main main)
        {
            this.main = main;
            // Set the canvas size and hook the form for updates
            canvasWidth = main.Width;
            canvasHeight = main.Height;
            main.SizeChanged += new EventHandler(main_SizeChanged);
            // Calculate the ball diameter based on the size of the canvas
            int diameter = (int)Math.Min(canvasWidth * 0.05, canvasHeight * 0.05);
            // Create new array
            balls = new List<Ball>();
            // Generate random new balls
            Random r = new Random();
            Ball b;
            for (int i = 1; i <= r.Next(BALLS_MIN, BALLS_MAX); i++)
            {
                // Create ball
                b = new Ball((i * 2) + i * diameter,
                        i * diameter,
                        r.Next(BALLS_VEL_MIN, BALLS_VEL_MAX), r.Next(BALLS_VEL_MIN, BALLS_VEL_MAX), diameter, new SolidBrush(Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255))))
                        ;
                // Add variance to x if within bounds of canvas
                if(canvasWidth - (b.x + b.diameter) > 0) b.x += r.Next(0, canvasWidth - (b.x + b.diameter));
                // Add to collection
                balls.Add(b);
            }
        }
        #endregion

        #region "Methods - Events"
        void main_SizeChanged(object sender, EventArgs e)
        {
            canvasWidth = main.Width;
            canvasHeight = main.Height;
        }
        #endregion

        #region "Methods"
        public void logic()
        {
            lock (balls)
            {
                // Calculate canvas mid-points
                int midX = canvasWidth / 2;
                int midY = canvasHeight / 2;
                // Update the position of all the balls
                bool inverse;
                bool collision;
                int newX;
                int newY;
                double pythag;
                foreach (Ball b in balls)
                {
                    // Calculate the new position
                    newX = b.x + b.velX;
                    newY = b.y + b.velY;
                    // Check the position is within bounds, else inverse the velocity
                    inverse = false;
                    if (newX < 0)
                    {
                        newX = 0;
                        inverse = true;
                    }
                    if (newX + b.diameter > canvasWidth)
                    {
                        newX = canvasWidth - b.diameter;
                        inverse = true;
                    }
                    if (newY < 0)
                    {
                        newY = 0;
                        inverse = true;
                    }
                    if (newY + b.diameter > canvasHeight)
                    {
                        newY = canvasHeight - b.diameter;
                        inverse = true;
                    }
                    // Perform collision detection
                    collision = false;
                    foreach (Ball b2 in balls)
                        if (b2 != b)
                        {
                            pythag = Math.Sqrt(Math.Pow(newX - b2.x, 2) + Math.Pow(newY - b2.y, 2));
                            if (pythag < b.diameter) collision = true;
                        }
                    // Only move to the new position if no collision occurred
                    if (!collision)
                    {
                        b.x = newX;
                        b.y = newY;
                    }
                    // Only inverse the velocity if we hit something
                    if (collision || inverse)
                    {
                        b.velX = -b.velX;
                        b.velY = -b.velY;
                    }
                }
            }
        }
        public void draw(Graphics g)
        {
            // Render the balls
            lock (balls)
            {
                foreach (Ball b in balls)
                    g.FillEllipse(b.colour, b.x, b.y, b.diameter, b.diameter);
            }
        }
        #endregion
    }
    public class Ball
    {
        public int x, y, velX, velY, diameter;
        public SolidBrush colour;
        public Ball(int x, int y, int velX, int velY, int diameter, SolidBrush colour)
        {
            this.x = x;
            this.y = y;
            this.diameter = diameter;
            this.velX = velX;
            this.velY = velY;
            this.colour = colour;
        }
    }
}