﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BirdBomber.Lib
{
    public class Fighter:Sprite
    {
        public Fighter(Game game):base(game) {
            Texture = game.Content.Load<Texture2D>("ship");
            Restore();
        }
        public void Restore()
        {
            Position = new Vector2(game.GraphicsDevice.Viewport.Width / 2 - 20, game.GraphicsDevice.Viewport.Height - 90);
            Speed = 5;
        }
        public override void Update(GameTime gameTime)
        {
            CheckKeyPress();
            //base.Update(gameTime);
        }
        private void CheckKeyPress()
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Left))
            {
                Position.X -= Speed;
            }
            if (ks.IsKeyDown(Keys.Right))
            {
                Position.X += Speed;
            }
            if (ks.IsKeyDown(Keys.Down) && Position.Y< game.GraphicsDevice.Viewport.Height-90)
            {
                Position.Y += Speed;
            }
            if (ks.IsKeyDown(Keys.Up) && Position.Y>0)
            {
                Position.Y -= Speed;
            }
            if (Position.X < -40)
            {
                Position.X = game.GraphicsDevice.Viewport.Width + 40;
            }
            if (Position.X > game.GraphicsDevice.Viewport.Width + 40)
            {
                Position.X = -40;
            }
        }
    }
}
