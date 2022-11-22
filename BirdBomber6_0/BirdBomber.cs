using BirdBomber.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BirdBomber
{
    public enum GameState
    {
        Paus,
        InGame,
        Error
    }
    public class BirdBomber : Game
    {
        private string test = "";
        private GameState ActiveState = GameState.Paus;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        Random r = new Random();
        SpriteFont Font;
        int Life = 3;
        int Points = 0;

        Fighter fighter { get; set; }
        List<Shot> shots { get; set; } = new List<Shot>();
        List<Bomb> bombs { get; set; } = new List<Bomb>();
        List<Explosion> explosions { get; set; } = new();
        Ufo ufo { get; set; }

        Texture2D Background;
        Texture2D BackgroundEnd;

        //Hur ofta man får skjuta - tiden mellan skotten
        int Shot_delay = 300;
        int Shot_time;

        //Hur ofta bomberna ska falla
        int Bomb_delay = 350;
        int Bomb_time;

        //Hur ofta Ufo ska komma
        int Ufo_delay = 10000;
        int Ufo_time;

        //Ljud
        SoundEffect laserSound;
        SoundEffect explosionSound;

        public BirdBomber()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            fighter = new Fighter(this)
            {
                Position = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2 - 20, graphics.GraphicsDevice.Viewport.Height - 60),
                Speed = 5,
            };
            graphics.PreferredBackBufferWidth = 800;// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = 550; //GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();
            base.Initialize();

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Background = Content.Load<Texture2D>("bgspace");
            BackgroundEnd = Content.Load<Texture2D>("bgdeath");

            laserSound = Content.Load<SoundEffect>("laserSound");
            explosionSound = Content.Load<SoundEffect>("explosionSound");
            Font = Content.Load<SpriteFont>("Text");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //Här uppdaterar vi all logic, skapar objekt, kontrollerar kollissioner mm - ingenting ritas upp
            //utan vi sätter bara förutsättningarna här

            //Avslutar spelet om vi klickar Esc
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            //Lyssnar av tangentbordet
            KeyboardState ks = Keyboard.GetState();

            //Om spelet är igång
            if (ActiveState == GameState.InGame)
            {
                fighter.Update(gameTime);

                // Skapa Ufo
                Ufo_time -= gameTime.ElapsedGameTime.Milliseconds;
                if (Ufo_time < 0)
                {
                    Ufo_time = Ufo_delay;
                    ufo = new Ufo(this);
                    Shot_delay = 800; // Vi gör skjut-tiden långsammare när det kommer ett nytt ufo!
                }
                if (ufo != null)
                {
                    ufo.Update(gameTime);
                }
                //Skapa skotten och uppdatera samt radera gamla
                Shot_time -= gameTime.ElapsedGameTime.Milliseconds; // Tiden för en loop
                if (Shot_time < 0)
                {
                    Shot_time = 0;
                }
                if (ks.IsKeyDown(Keys.Space) && Shot_time == 0)
                {
                    laserSound.Play(0.7f, 0f, 0f);
                    Shot_time = Shot_delay;
                    shots.Add(new Shot(this)
                    {
                        Position = new Vector2(fighter.Position.X + 20, fighter.Position.Y)
                    });
                }
                shots.ForEach(e => e.Update(gameTime));
                shots.RemoveAll(e => !e.IsActive);
                if (ks.GetPressedKeys().Length > 0)
                {
                    Debug.WriteLine(ks.IsKeyUp(ks.GetPressedKeys()[0]).ToString());
                }

                //Debug.WriteLine("hej+" + gameTime);
                //Bomber, skapa, uppdatera positioner samt radera gamla
                Bomb_time -= gameTime.ElapsedGameTime.Milliseconds;
                if (Bomb_time < 0)
                {
                    Bomb_time = 0;
                }
                if (Bomb_time == 0 && Life > 0)
                {
                    Bomb_time = Bomb_delay;
                    //Slumpa x position.
                    int x = r.Next(0, graphics.GraphicsDevice.Viewport.Width - 40);
                    bombs.Add(new Bomb(this)
                    {
                        Position = new Vector2(x, -50)
                    });
                }
                bombs.ForEach(e => e.Update(gameTime));
                bombs.RemoveAll(e => e.IsActive == false);

                //Uppdatera gamla explisioner och radera gamla
                explosions.ForEach(e => e.Update(gameTime));
                explosions.RemoveAll(e => e.IsActive == false);

                //För varje bomb i bomlistan
                foreach (Bomb b in bombs)
                {
                    //Kolla om den kolliderar med fightern
                    if (b.Rectangle.Intersects(fighter.Rectangle))
                    {
                        //Om en bomb krockar med rymdskeppet - skapa ny explosion på detta stället
                        explosions.Add(new Explosion(this)
                        {
                            Position = fighter.Position
                        });
                        explosionSound.Play(0.05f, 0f, 0f); //Spela upp ljud av explossion
                        if (Life > 0) Life -= 1; //Minska livet
                        b.IsActive = false; //Inaktivera bomben
                        Shot_delay = 300; //Återställer skjuthastigheten om man har fått snabbare tidigare
                    }
                    foreach (Shot s in shots)
                    {
                        //För varje bomb kollar vi även om något skott kolliderar med en bomb
                        //Om bomben b - kolliderar med skottet s
                        if (b.Rectangle.Intersects(s.Rectangle))
                        {
                            explosions.Add(new Explosion(this)
                            {
                                Position = b.Position
                            });
                            Points += 1 * b.Speed; //Här får vi ju poäng :) 
                            explosionSound.Play(0.05f, 0f, 0f);
                            b.IsActive = false; //Bomben ska inaktiveras
                            s.IsActive = false; //Skottet ska inaktiveras - kommer att raderas sen
                        }
                    }
                }
                //Koll om träff ufo
                if (ufo != null)
                {
                    foreach (Shot s in shots)
                    {
                        if (ufo.Rectangle.Intersects(s.Rectangle))
                        {
                            explosions.Add(new Explosion(this)
                            {
                                Position = ufo.Position
                            });
                            Points += 100; //Poäng!!!!
                            explosionSound.Play(0.05f, 0f, 0f);
                            ufo.IsActive = false;
                            s.IsActive = false;
                            Shot_delay = 150; //Lite bonus vid träff - skjuta oftare
                        }
                    }
                }
                if (Life == 0)
                {
                    //Om liven tagit slut
                    ActiveState = GameState.Paus;
                }
            }
            else if (ActiveState == GameState.Paus)
            {
                //Om spelet inte är aktiverat
                if (ks.IsKeyDown(Keys.Enter))
                {
                    Life = 3; Points = 0;
                    ActiveState = GameState.InGame;
                    //Vi behöver ju även göra en reset på alla bomber och var fightern är när vi startar om
                }
            }
            else { }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if (ActiveState == GameState.Paus)
            {
                spriteBatch.Draw(BackgroundEnd, new Vector2(0, 0), Color.White);
                //Om spelet inte är aktivt
                if (Life > 0)
                {
                    spriteBatch.DrawString(Font, "PRESS ENTER TO START", new Vector2(250, 250), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(Font, "GAME OVER - PRESS ENTER TO RESTART", new Vector2(250, 250), Color.White);
                }

            }
            else if (ActiveState == GameState.InGame)
            {
                spriteBatch.Draw(Background, new Vector2(0, 0), Color.White);

                //Uppdatera fightern
                fighter.Draw(spriteBatch);

                //Uppdatera alla skott
                shots.ForEach(e => e.Draw(spriteBatch));

                //Uppdatera alla bomber
                bombs.ForEach(e => e.Draw(spriteBatch));
                explosions.ForEach(e => e.Draw(spriteBatch));

                if (ufo != null)
                {
                    if (ufo.IsActive == false)
                    {
                        ufo = null;
                    }
                    else
                    {
                        ufo.Draw(spriteBatch);
                    }
                }
                spriteBatch.DrawString(Font, "Skott: " + shots.Count, new Vector2(30, 20), Color.White);
                spriteBatch.DrawString(Font, "Bomber: " + bombs.Count, new Vector2(120, 20), Color.White);

            }
            //Skriver ut lite info
            spriteBatch.DrawString(Font, "Liv: " + Life, new Vector2(600, 20), Color.White);
            spriteBatch.DrawString(Font, "Points: " + Points, new Vector2(680, 20), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}