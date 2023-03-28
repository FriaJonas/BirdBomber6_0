using BirdBomber.Lib;
using BirdBomber6_0.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BirdBomber
{
    public enum GameState
    {
        Start,
        InGame,
        GameOver
    }
    public class BirdBomber : Game
    {
        private List<HighScore> HighScores { get; set; } = new List<HighScore>();
        private bool GotHighScore { get;set; }=false;

        private string test = "";
        private GameState ActiveState = GameState.Start;

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
        int Shot_delay = 150;
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
            HighScores= LoadHighScore();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            fighter = new Fighter(this);
            //graphics.IsFullScreen = true;
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferWidth =  GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
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
                    Shot_delay = 600; // Vi gör skjut-tiden långsammare när det kommer ett nytt ufo!
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
                            Shot_delay = 50; //Lite bonus vid träff - skjuta oftare
                        }
                    }
                }
                if (Life == 0)
                {
                    //Om liven tagit slut
                    ActiveState = GameState.GameOver;
                    HighScore  newHs = new HighScore()
                    {
                        Score = Points,
                        Nickname = "test",
                        TimePlayed = DateTime.Now
                    };
                    SaveScore(newHs);
                }
            }
            else if (ActiveState == GameState.GameOver | ActiveState == GameState.Start)
            {
                //Om spelet inte är aktiverat
                if (ks.IsKeyDown(Keys.Enter))
                {
                    GotHighScore = false;
                    Life = 3; Points = 0;
                    bombs.Clear();
                    shots.Clear();
                    fighter.Restore();
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
            if (ActiveState == GameState.Start)
            {
                spriteBatch.Draw(BackgroundEnd, GraphicsDevice.Viewport.Bounds, Color.White);
                //Om spelet inte är aktivt
                spriteBatch.DrawString(Font, "PRESS ENTER TO START", new Vector2(250, 250), Color.White);
                DrawHigScore();
            }
            else if(ActiveState== GameState.GameOver)
            {
                spriteBatch.Draw(BackgroundEnd, GraphicsDevice.Viewport.Bounds, Color.White);
                //Kolla Highscore
                
                if (GotHighScore)
                {
                    spriteBatch.DrawString(Font, "GRATULATION TO HIGHSCORE", new Vector2(GraphicsDevice.Viewport.Bounds.Width/3, 150), Color.White);
                }

                spriteBatch.DrawString(Font, "GAME OVER - PRESS ENTER TO RESTART", new Vector2(GraphicsDevice.Viewport.Bounds.Width / 3, 200), Color.White);
                DrawHigScore();
            }
            else if (ActiveState == GameState.InGame)
            {
                spriteBatch.Draw(Background, GraphicsDevice.Viewport.Bounds, Color.White);
                
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

        private void SaveScore(HighScore newScore)
        {
            //Vi lägger till scoren
            int minScore = 0;
            int maxScore = 0;
            if (HighScores.Count > 0)
            {
                minScore = HighScores.Min(x => x.Score);
                maxScore = HighScores.Max(x => x.Score);
            }
            if (newScore.Score > maxScore)
            {
                GotHighScore = true;
            }
            //om scoren är högre än lägsta i listan eller att det inte finns 5st.
            if(newScore.Score> minScore | HighScores.Count<5)
            {
                HighScores.Add(newScore);
                HighScores.Sort(delegate (HighScore x, HighScore y)
                {
                    return y.Score.CompareTo(x.Score);
                });
                //Plocka ut de fem högsta
                HighScores = HighScores.Take(5).ToList();

                string serializedText = JsonSerializer.Serialize<List<HighScore>>(HighScores);

                File.WriteAllText("HighScore.json", serializedText);
            }
        }
        private List<HighScore> LoadHighScore()
        {
            try
            {
                var content = File.ReadAllText("HighScore.json");
                var lista =  JsonSerializer.Deserialize<List<HighScore>>(content);
                return lista;
            }
            catch
            {
               return new List<HighScore>();
                        
            }
        }
        private void DrawHigScore()
        {
            //Skriva ut Hiscore TOP 10
            if(HighScores.Count > 0)
            {
                float y = 300;
                spriteBatch.DrawString(Font, "High scores Top 5: ", new Vector2(250, y), Color.White);
                
                foreach(var hs in HighScores)
                {
                    y += 30;
                    spriteBatch.DrawString(Font,hs.TimePlayed.ToShortDateString()+" "+ hs.Nickname+" " +hs.Score, new Vector2(250, y), Color.White);
                    
                }
                
            }

        }
    }
}