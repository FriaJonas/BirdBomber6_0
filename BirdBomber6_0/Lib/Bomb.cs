using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BirdBomber.Lib
{
    public class Bomb:Sprite
    {
        public Bomb(Game game) : base(game)
        {
            Random rnd = new Random();

            Speed = rnd.Next(3, 10);
            if (Speed < 6)
            {
                Texture = game.Content.Load<Texture2D>("bomb");
            }
            else
            {
                Texture = game.Content.Load<Texture2D>("rocket2");
            }
            
        }
        public override void Update(GameTime gameTime)
        {
            Position.Y += Speed;
            //Om bomberna faller förbi
            if (Position.Y > game.GraphicsDevice.Viewport.Height+20) this.IsActive = false;
            base.Update(gameTime);
        }

    }
}
