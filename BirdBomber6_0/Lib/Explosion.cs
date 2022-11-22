using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace BirdBomber.Lib
{
    public class Explosion:Sprite
    {
        //public float Scale { get; set; }
        private int Timer = 30;

        public Explosion(Game game) : base(game)
        {
            Texture = game.Content.Load<Texture2D>("explosion");
        }
        public override Color Color
        {
            get { return new Color(Timer * 8, Timer * 8, Timer * 8, Timer * 8); }
        }
        public override void Update(GameTime gameTime)
        {
            if (Timer > 0)
                Timer--;
            else
                IsActive = false;

            base.Update(gameTime);
        }
    }
}
