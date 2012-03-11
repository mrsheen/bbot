using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Contexts;
using System.IO;
using System.Linq;
using System.Threading;

namespace BBot.States
{
    public class BaseGameState
    {
        internal GameEngine game;

        public string AssetName;
        public int MinimumConfidence;

        public Point matchOffset;

        protected bool bStarted = false;
        

        public BaseGameState()
        {
            AssetName = "-none-";
            MinimumConfidence = 1;
            matchOffset = new Point(0, 0);
        }

        public virtual void Init(GameEngine gameRef)
        {
            game = gameRef;
        }

        private Thread FindThread;

        private void TryFindState()
        {

            FindStateFromScreen(true);

            if (!game.GameExtents.HasValue)
                game.StateManager.PopState();

            bStarted = true;
            return;
        }


        public virtual void Cleanup() {
            if (FindThread != null)
            {
                FindThread.Abort();
                FindThread = null;
            }
        }

        public virtual void Pause() { }

        public virtual void Resume() { }

        public virtual bool HandleEvents()
        {
            if (game.EventStack.Count > 0)
            {
                GameEvent myEvent = game.EventStack.Pop();


                if (myEvent.eventType == EngineEventType.ENGINE_INIT)
                {
                    FindThread = new Thread(new ThreadStart(TryFindState));
                    FindThread.Name = String.Format("FindThread-{0}-{1}", this.AssetName, DateTime.Now);

                    FindThread.Start();

                    return true;

                }

              


                if (myEvent.eventType == EngineEventType.ENGINE_SHUTDOWN)
                {
                    this.Cleanup();
                    return true;

                }

                game.EventStack.Push(myEvent);
            }

            

            return false;
        }

        public void Run()
        {
            if (!bStarted && !game.GameExtents.HasValue)
                return;

            this.Update();
            this.Draw();
        }

        public virtual void Update() { }

        public virtual void Draw() { }

        public FindBitmap.MatchingPoint FindStateFromScreen(bool quickCheck = false)
        {
            SearchParams search = new SearchParams();

            
            FindBitmap.MatchingPoint match = new FindBitmap.MatchingPoint();

            search.UsingGameExtents = false;
            search.MinimumConfidence = this.MinimumConfidence;
            search.QuickCheck = quickCheck;

            //STEP1: Get image to find and mask(if available)
            search.ToFind = GetBitmapByType(StateBitmapType.RawImage);
            search.Mask = GetBitmapByType(StateBitmapType.Mask, search.ToFind.Size, search.ToFind.PixelFormat);

            //STEP2: Get area of screen to search
            GetSearchAreaBitmap(ref search);

            //STEP3: Do search
            if (FindExact(search, ref match))
                return match;

            if (FindHazy(search, ref match))
                return match;

            if (FindUsingAlternateMasks(search, ref match))
                return match;

            return match;

        }

        #region FindStateFromScreen


        private struct SearchParams
        {
            public Bitmap SearchArea;
            public Bitmap ToFind;
            public Bitmap Mask;
            public int MinimumConfidence;
            public bool QuickCheck;
            public bool UsingGameExtents;

        }

        private enum StateBitmapType
        {
            RawImage,
            Mask,
            Blue,
            SmartMask
        }

        private bool FindUsingAlternateMasks(SearchParams search, ref FindBitmap.MatchingPoint match)
        {
            Stack<StateBitmapType> typesToCheck = new Stack<StateBitmapType>();
            //typesToCheck.Push(StateBitmapType.Blue);
            typesToCheck.Push(StateBitmapType.SmartMask);

            while (typesToCheck.Count > 0)
            {
                StateBitmapType maskType = typesToCheck.Pop();
                search.Mask = GetBitmapByType(maskType, search.ToFind.Size, search.ToFind.PixelFormat);
                if (FindExact(search, ref match))
                    return true;
                if (FindHazy(search, ref match))
                    return true;
            }

            return false;
        }

        private bool FindExact(SearchParams search, ref FindBitmap.MatchingPoint match)
        {
            if (search.QuickCheck && search.SearchArea.Size == search.ToFind.Size)
            {
                match = FindBitmap.CheckExactMatch(search.SearchArea, search.ToFind, search.Mask, search.MinimumConfidence);

                if (!match.Confident)
                {
                    try
                    {
                        if (game.DebugMode)
                            search.SearchArea.Save(String.Format("{0}-notmatched-{1}-{2}.bmp", Assembly.GetCallingAssembly().Location, this.AssetName, DateTime.Now));
                    }
                    catch (Exception)
                    { }
                }

                return match.Confident;
            }

            return false;
        }

        private bool FindHazy(SearchParams search, ref FindBitmap.MatchingPoint match)
        {
            match = FindBitmap.FindInScreen(search.SearchArea, search.ToFind, search.Mask, search.QuickCheck, search.MinimumConfidence);
            if (search.UsingGameExtents && !search.QuickCheck )
            {
                match.X += (game.GameExtents.Value.X - 20);
                match.Y += (game.GameExtents.Value.Y - 20);
            }

            if (match.Confident)
                game.UpdateGameExtents(match.X - this.matchOffset.X, match.Y - this.matchOffset.Y);

            return match.Confident;
        }


        private Bitmap GetBitmapByType(StateBitmapType bitmapType, Size? size = null, PixelFormat? format = null)
        {
            Bitmap rootBitmap = null;
            Bitmap filterBitmap = null;
            string assetName = String.Empty;
            AForge.Imaging.Filters.Add addFilter;
            try
            {
                // Add given masks
                switch (bitmapType)
                {
                    case StateBitmapType.SmartMask:
                        rootBitmap = GetBitmap(this.AssetName, size, format);
                        assetName = "wholegame.background";
                        filterBitmap = GetBitmap(assetName, size, format);
                        if (filterBitmap != null)
                        {
                            addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                            addFilter.ApplyInPlace(rootBitmap);
                        }
                        goto case StateBitmapType.Mask; // farking c#: http://stackoverflow.com/a/174223
                    case StateBitmapType.Blue:
                    case StateBitmapType.Mask:
                        // Get area to find
                        if (rootBitmap == null)
                            rootBitmap = GetBitmap("wholegame.blankmask", size, format);
                        assetName = String.Format("{0}.mask", this.AssetName);
                        filterBitmap = GetBitmap(assetName, size, format);
                        if (filterBitmap != null)
                        {
                            addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                            addFilter.ApplyInPlace(rootBitmap);
                        }
                        break;
                    case StateBitmapType.RawImage:
                        rootBitmap = GetBitmap(this.AssetName, size, format);
                        break;
                    default:
                        break; // do nothing
                }

            }
            catch (Exception) { }
            return rootBitmap;
        }

        
        private Bitmap GetBitmap(string assetName, Size? size, PixelFormat? format)
        {
            Bitmap assetBitmap = null;

            String fullAssetName = String.Format("BBot.Assets.{0}.bmp", assetName);
            String[] assetNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            
            if (!assetNames.Contains(fullAssetName))
                return assetBitmap; // Asset not found, return null

            Stream ImageAsset = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullAssetName);
            assetBitmap = (Bitmap)System.Drawing.Bitmap.FromStream(ImageAsset);

            if (!size.HasValue && !format.HasValue)
                return assetBitmap; // No need to reformat, return raw asset

            // Copy asset to correct size and format (this will add buffer of black pixels if required)
            Bitmap resizedAssetBitmap = new Bitmap( size.HasValue ? size.Value.Width : assetBitmap.Width,
                                                    size.HasValue ? size.Value.Height : assetBitmap.Height,
                                                    format.HasValue ? format.Value : assetBitmap.PixelFormat);

            using (Graphics g = Graphics.FromImage(resizedAssetBitmap))
            {
                g.DrawImage(assetBitmap, 0,0,assetBitmap.Width, assetBitmap.Height);
            }

            return resizedAssetBitmap;
        }

        private void GetSearchAreaBitmap(ref SearchParams search) // PixelFormat format)
        {
            game.CaptureArea(); // Get search bitmap

            // Set search bounds to captured area
            Rectangle searchLocation = new Rectangle(0, 0, game.GameScreen.Width, game.GameScreen.Height);

            if (game.GameExtents.HasValue)
            {// Search in gamescreen if available
                search.UsingGameExtents = true;

                if (search.QuickCheck)
                { // Use pre-calculated offsets
                    if (this.matchOffset != null)
                    {
                        searchLocation.X += this.matchOffset.X;
                        searchLocation.Y += this.matchOffset.Y;
                    }
                    searchLocation.Width = search.ToFind.Width;
                    searchLocation.Height = search.ToFind.Height;

                }
                else
                { // Add buffer
                    searchLocation.X += 20;
                    searchLocation.Y += 20;
                    searchLocation.Width += 40;
                    searchLocation.Height += 40;
                }
            }

            // Copy gamescreen to search area (this will add buffer of black pixels if required
            search.SearchArea = new Bitmap(searchLocation.Width,searchLocation.Height,search.ToFind.PixelFormat);

            using (Graphics g = Graphics.FromImage(search.SearchArea))
            {
                g.DrawImage(game.GameScreen, searchLocation.Location); 
            }

        }


        #endregion

    }
}
