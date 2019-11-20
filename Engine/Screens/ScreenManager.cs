
//#define CATCH_EXCEPTIONS

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Screens
{
    public class ScreenManager : IDisposable
    {
        private readonly List<GameScreen> allScreens = new List<GameScreen>();
        private readonly Dictionary<Type, GameScreen> mappedScreens = new Dictionary<Type, GameScreen>();

        public GameScreen GetScreen(ushort id)
        {
            if (id == 0)
                return null;

            if (id > allScreens.Count)
                return null;

            return allScreens[id - 1];
        }

        public T GetScreen<T>() where T : GameScreen
        {
            if (mappedScreens.ContainsKey(typeof(T)))
            {
                return mappedScreens[typeof(T)] as T;
            }
            else
            {
                return null;
            }
        }

        public GameScreen RegisterNew(GameScreen scr)
        {
            if(scr == null)
            {
                Debug.Warn("Null screen passed into registering.");
                return null;
            }

            if(scr.ID != 0)
            {
                Debug.Warn($"Screen {scr.Name} is already registered to this or another screen manager under the ID {scr.ID}.");
                return scr;
            }

            allScreens.Add(scr);
            scr.ID = (ushort)allScreens.Count;
            Debug.Log($"Registered new screen {scr.Name} under ID {scr.ID}.");

            if (!mappedScreens.ContainsKey(scr.GetType()))
            {
                mappedScreens.Add(scr.GetType(), scr);
            }

            return scr;
        }

        private void UpdateActiveState(GameScreen screen)
        {
            if (screen.targetActive != screen.realActive)
            {
                bool oldActive = screen.realActive;
                screen.realActive = screen.targetActive;
                if (oldActive)
                {
                    // Used to be active, is no longer active.
                    screen.OnDisabled();
                }
                else
                {
                    // Used to not be active, is now active.
                    screen.OnEnabled();
                }
            }
        }

        public void Initialize()
        {
            foreach (var scr in allScreens)
            {
#if CATCH_EXCEPTIONS
                try
                {
#endif
                    scr.Init();
#if CATCH_EXCEPTIONS
                }
                catch (Exception e)
                {
                    Debug.Error($"Exception initializing screen {scr.Name} ({scr.ID}).", e);
                }
#endif
            }
        }

        public void LoadContent(JContent contentManager)
        {
            foreach (var scr in allScreens)
            {
#if CATCH_EXCEPTIONS
                try
                {
#endif
                    scr.LoadContent(contentManager);
#if CATCH_EXCEPTIONS
                }
                catch (Exception e)
                {
                    Debug.Error($"Exception loading content of screen {scr.Name} ({scr.ID}).", e);
                }
#endif
            }
        }

        public void Update()
        {
            foreach (var scr in allScreens)
            {
#if CATCH_EXCEPTIONS
                try
                {
#endif
                    UpdateActiveState(scr);
                    scr.InternalUpdate();
                    UpdateActiveState(scr);
#if CATCH_EXCEPTIONS
                }
                catch (Exception e)
                {
                    Debug.Error($"Exception updating screen {scr.Name} ({scr.ID}).", e);
                }
#endif
            }
        }

        public void Draw(SpriteBatch spr)
        {
            foreach (var scr in allScreens)
            {
#if CATCH_EXCEPTIONS
                try
                {
#endif
                    scr.InternalDraw(spr);
#if CATCH_EXCEPTIONS
                }
                catch (Exception e)
                {
                    Debug.Error($"Exception drawing screen {scr.Name} ({scr.ID}).", e);
                }
#endif
            }
        }

        public void DrawUI(SpriteBatch spr)
        {
            foreach (var scr in allScreens)
            {
#if CATCH_EXCEPTIONS
                try
                {
#endif
                    scr.InternalDrawUI(spr);
#if CATCH_EXCEPTIONS
                }
                catch (Exception e)
                {
                    Debug.Error($"Exception drawing UI for screen {scr.Name} ({scr.ID}).", e);
                }
#endif
            }
        }

        public void OnClose()
        {
            foreach (var screen in allScreens)
            {
                screen?.OnClosing();
            }
        }

        public void Dispose()
        {

        }
    }
}
