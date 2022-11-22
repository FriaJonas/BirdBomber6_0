using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BirdBomber.Lib
{
    public class Shot: Sprite
    {

        public Shot(Game game):base(game)
        {
            Speed = -5;
            Texture = game.Content.Load<Texture2D>("shot");

        }

        public override void Update(GameTime gameTime)
        {
            Position.Y += Speed;
            if (Position.Y < 0) this.IsActive = false;
            base.Update(gameTime);
        }

    }
}
