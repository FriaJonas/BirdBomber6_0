using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BirdBomber.Lib
{
    public class Ufo: Sprite
    {
        public Ufo(Game _game) : base(_game)
        {
            Random rnd = new Random();
            int t = rnd.Next(0, 2);
            Speed = rnd.Next(5, 9);
            if (t == 0)
            {
                
                Position = new Vector2(-100, 50);
            }
            else
            {
                Speed = -Speed;
                
                Position = new Vector2(game.GraphicsDevice.Viewport.Width + 100, 50);
            }
            Texture = game.Content.Load<Texture2D>("ufo");
            Sound = game.Content.Load<SoundEffect>("laserSound");

            //PlaySound();
            
            
        }
        public override void Update(GameTime gameTime)
        {
            Position.X += Speed;
            if ((Speed>0 && Position.X > game.GraphicsDevice.Viewport.Width + 50)|(Speed > 0 && Position.X < game.GraphicsDevice.Viewport.Width -1000))
            {
                this.IsActive = false;
            }
            base.Update(gameTime);
        }
    }
}
