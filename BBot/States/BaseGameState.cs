using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;

namespace BBot.States
{
    public class BaseGameState
    {
        internal GameEngine game;

        public string AssetName;
        public int MinimumConfidence;

        public Point matchOffset;

        protected bool bStarted = false;
        protected bool bStarting = false;

        public BaseGameState()
        {
            AssetName = "-none-";
            MinimumConfidence = 1;
            matchOffset = new Point(0, 0);
        }

        public virtual void Init(GameEngine gameRef)
        {
            game = gameRef;
            bStarted = false;
        }

        public virtual void Start()
        {
            if (bStarted)
                throw new ApplicationException("Game state already started");

            if (bStarting)
                return;

            bStarting = true;
            if (!game.GameExtentsOnScreen.HasValue)
                FindStateFromScreen(true);
            bStarted = true;
            bStarting = false;
        }

        public virtual void Cleanup() { }

        public virtual void Pause() { }

        public virtual void Resume() { }

        public virtual bool HandleEvents()
        {
            if (!bStarted) // Bail out if havent finished init
                return true;

            if (!game.GameExtentsOnScreen.HasValue)
            {
                game.StateManager.PopState();
                return true;
            }
            return false;
        }

        public virtual void Update() { }

        public virtual void Draw() { }

        public FindBitmap.MatchingPoint FindStateFromScreen(bool quickCheck = false)
        {
            SearchParams search = new SearchParams();

            Rectangle rSearchLocation = game.ScreenRectangle;
            FindBitmap.MatchingPoint match = new FindBitmap.MatchingPoint();

            search.UsingGameExtents = false;
            search.MinimumConfidence = this.MinimumConfidence;
            search.QuickCheck = quickCheck;

            //STEP1: Get image to find and mask(if available)
            search.ToFind = GetBitmapByType(StateBitmapType.RawImage);
            search.Mask = GetBitmapByType(StateBitmapType.Mask, search.ToFind.Size, search.ToFind.PixelFormat);

            //STEP2: Get area of screen to search
            GetSearchLocation(ref search);
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
            public Rectangle SearchLocation;

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
                match.X += (game.GameExtentsOnScreen.Value.X - 20);
                match.Y += (game.GameExtentsOnScreen.Value.Y - 20);
            }

            if (match.Confident)
                game.UpdateGameExtents(match.X - this.matchOffset.X, match.Y - this.matchOffset.Y);

            return match.Confident;
        }


        private Bitmap GetBitmapByType(StateBitmapType bitmapType, Size? size = null, PixelFormat? format = null)
        {
            Bitmap rootBitmap = null;

            String resourceName = String.Format("BBot.Assets.{0}.bmp", this.AssetName);
            
            try
            {
                // Get area to find
                rootBitmap = (Bitmap)System.Drawing.Bitmap.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
                rootBitmap = rootBitmap.Clone(
                    new Rectangle(0, 0,
                        size.HasValue ? size.Value.Width : rootBitmap.Width,
                        size.HasValue ? size.Value.Height : rootBitmap.Height),
                    format.HasValue ? format.Value : rootBitmap.PixelFormat);

                switch (bitmapType)
                {
                    case StateBitmapType.Mask:
                    case StateBitmapType.Blue:                        
                    case StateBitmapType.SmartMask:
                        AForge.Imaging.Filters.Add addFilter = new AForge.Imaging.Filters.Add(GetBackgroundBitmap(size, format));
                        addFilter.ApplyInPlace(rootBitmap);
                        break;
                    case StateBitmapType.RawImage:
                    default:
                        break; // do nothing
                }

            }
            catch (Exception) { }
            return rootBitmap;
        }

        private Bitmap GetBackgroundBitmap(Size? size, PixelFormat? format)
        {
            Bitmap filterBitmap = (Bitmap)System.Drawing.Bitmap.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BBot.Assets.wholegame.background.bmp"));
            filterBitmap = filterBitmap.Clone(
                new Rectangle(0, 0,
                    size.HasValue ? size.Value.Width : filterBitmap.Width,
                    size.HasValue ? size.Value.Height : filterBitmap.Height),
                format.HasValue ? format.Value : filterBitmap.PixelFormat);
            return filterBitmap;
        }

        private void GetSearchLocation(ref SearchParams search)
        {
            search.SearchLocation = game.ScreenRectangle;

            if (game.GameExtentsOnScreen.HasValue)
            {// Search in gamescreen if available
                search.UsingGameExtents = true;
                search.SearchLocation = game.GameExtentsOnScreen.Value;

                if (search.QuickCheck)
                { // Use pre-calculated offsets
                    if (this.matchOffset != null)
                    {
                        search.SearchLocation.X += this.matchOffset.X;
                        search.SearchLocation.Y += this.matchOffset.Y;
                    }
                    search.SearchLocation.Width = search.ToFind.Width;
                    search.SearchLocation.Height = search.ToFind.Height;

                }
                else
                { // Add buffer
                    search.SearchLocation.X -= 20;
                    search.SearchLocation.Y -= 20;
                    search.SearchLocation.Width += 40;
                    search.SearchLocation.Height += 40;
                }
            }

        }

        private void GetSearchAreaBitmap(ref SearchParams search) // PixelFormat format)
        {
            // Build up search area from screen
            search.SearchArea = new Bitmap(search.SearchLocation.Width, search.SearchLocation.Height);

            using (Graphics g = Graphics.FromImage(search.SearchArea))
            {
                g.CopyFromScreen(
                    search.SearchLocation.X, search.SearchLocation.Y,
                    0, 0,
                    search.SearchLocation.Size);
            }

            // Set pixel format
            search.SearchArea = search.SearchArea.Clone(new Rectangle(0, 0, search.SearchArea.Width, search.SearchArea.Height), search.ToFind.PixelFormat);
        }


        #endregion

    }
}
