using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BirdBomber.Lib
{
    public abstract class Sprite
    {
        public Game game { get; set; }
        protected Texture2D Texture { get; set; }
        public SoundEffect Sound { get; set; }
        public Vector2 Position;
        public virtual Color Color { get; set; } = Color.White;
        public int Speed { get; set; }
        public bool IsActive { get; set; } = true;

        public Sprite(Game _game) { game = _game; }
        
        //För kollisions koll
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }
        public virtual void Update(GameTime gameTime) 
        {

        }

        public  void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(Texture, Position, Color);        
        }

        public void PlaySound(bool on=true)
        {
            if (Sound != null && on)
            {
                Sound.Play(0.05f, 0f, 0f);
            }
            else
            {
                Sound.Dispose();
            }
        }
    }
}
