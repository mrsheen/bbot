using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.IO;
using System.Threading;

namespace BBot.GameDefinitions
{
    public class GameScreenDefinition
    {
        public readonly Size GameScreenSize = new Size(525, 435); // The size of the Bejeweled gem grid (in pixels, GridSize * CellSize)
        public readonly Point GameBoardOnScreen = new Point(175, 55);

        public string AssetName;
        public int MinimumConfidence;

        private FindBitmapWorker findBitmapWorker;

        public GameScreenDefinition(CancellationToken cancellationToken)
        {
            findBitmapWorker = new FindBitmapWorker(cancellationToken);
        }

        public Action<BBot.GameDefinitions.FindBitmapWorker.ImageSearchDetails> ImageDebugAction { get; set; }

        public MatchingPoint FindGameScreenInImage(Bitmap imageToSearch, bool quickCheck = false)
        {
            findBitmapWorker.ImageDebugAction = this.ImageDebugAction;

            SearchParams search = new SearchParams();
            MatchingPoint match = new MatchingPoint();

            using (search.SearchArea = imageToSearch.Clone(new Rectangle(0, 0, imageToSearch.Width, imageToSearch.Height), imageToSearch.PixelFormat))
            {
                search.MinimumConfidence =  this.MinimumConfidence;
                search.QuickCheck = quickCheck;

                //Get image to find and mask(if available)
                search.ToFind = GetBitmapByType(StateBitmapType.RawImage,null,imageToSearch.PixelFormat);
                
                //Do search
                if (FindUsingMasks(search, ref match))
                    return match;
            }
            return match;

        }


        private struct SearchParams
        {
            public Bitmap SearchArea;
            public Bitmap ToFind;
            public Bitmap Mask;
            public int MinimumConfidence;
            public bool QuickCheck;
        }

        private enum StateBitmapType
        {
            RawImage,
            Mask,
            Blue,
            SmartMask
        }

        private bool FindUsingMasks(SearchParams search, ref MatchingPoint match)
        {
            Stack<StateBitmapType> typesToCheck = new Stack<StateBitmapType>();
            typesToCheck.Push(StateBitmapType.Blue);
            typesToCheck.Push(StateBitmapType.Mask);
            typesToCheck.Push(StateBitmapType.SmartMask);


            while (typesToCheck.Count > 0)
            {
                if (match.AbortedSearch)
                    return false;
                StateBitmapType maskType = typesToCheck.Pop();

                using (search.Mask = GetBitmapByType(maskType, search.ToFind.Size, search.ToFind.PixelFormat))
                {
                    if (match.AbortedSearch)
                        return false;
                    if (FindExact(search, ref match))
                        return true;
                    if (search.QuickCheck)
                        return false; // First match didnt work, but asked to be quick
                    if (match.AbortedSearch)
                        return false;
                    if (FindHazy(search, ref match))
                        return true;
                    if (match.AbortedSearch)
                        return false;
                }
            }

            return false;
        }

        private bool FindExact(SearchParams search, ref MatchingPoint match)
        {
            if (search.QuickCheck && search.SearchArea.Size == search.ToFind.Size)
            {
                match = findBitmapWorker.CheckExactMatch(search.SearchArea, search.ToFind, search.Mask, search.MinimumConfidence);

                return match.Confident;
            }

            return false;
        }

        private bool FindHazy(SearchParams search, ref MatchingPoint match)
        {
            match = findBitmapWorker.FindInScreen(search.SearchArea, search.ToFind, search.Mask, search.QuickCheck, search.MinimumConfidence);

            return match.Confident;
        }



        private Bitmap GetBitmapByType(StateBitmapType bitmapType, Size? size = null, PixelFormat? format = null)
        {
            Bitmap rootBitmap = null;
            Bitmap filterBitmap = null;
            string assetName =  String.Empty;
            AForge.Imaging.Filters.Add addFilter;
            try
            {
                // Add given masks
                switch (bitmapType)
                {
                    case StateBitmapType.SmartMask:
                        rootBitmap = GetBitmap(this.AssetName, size, format);
                        assetName =  String.Format("{0}.smartmask", this.AssetName);
                        using (filterBitmap = GetBitmap(assetName, size, format))
                        {
                            if (filterBitmap != null)
                            { // Special smartmask found, use it
                                rootBitmap = GetBitmap("wholegame.blankmask", size, format);
                                addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                                addFilter.ApplyInPlace(rootBitmap);
                                goto case StateBitmapType.Blue;
                            }
                        }
                        // Use normal tofind but add background as mask
                        assetName =  "wholegame.background";
                        using (filterBitmap = GetBitmap(assetName, size, format))
                        {
                            if (filterBitmap != null)
                            {
                                addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                                addFilter.ApplyInPlace(rootBitmap);
                            }
                        }
                        goto case StateBitmapType.Blue; // farking c#: http://stackoverflow.com/a/174223
                    case StateBitmapType.Blue:
                        if (rootBitmap == null)
                            rootBitmap = GetBitmap("wholegame.blankmask", size, format);
                        assetName =  String.Format("{0}.blue", this.AssetName);
                        using (filterBitmap = GetBitmap(assetName, size, format))
                        {
                            if (filterBitmap != null)
                            {
                                addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                                addFilter.ApplyInPlace(rootBitmap);
                            }
                        }
                        goto case StateBitmapType.Mask; // farking c#: http://stackoverflow.com/a/174223
                    case StateBitmapType.Mask:
                        // Get area to find
                        if (rootBitmap == null)
                            rootBitmap = GetBitmap("wholegame.blankmask", size, format);
                        assetName =  String.Format("{0}.mask", this.AssetName);
                        using (filterBitmap = GetBitmap(assetName, size, format))
                        {
                            if (filterBitmap != null)
                            {
                                addFilter = new AForge.Imaging.Filters.Add(filterBitmap);
                                addFilter.ApplyInPlace(rootBitmap);
                            }
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


        private static Bitmap GetBitmap(string assetName, Size? size, PixelFormat? format)
        {
            String fullAssetName =  String.Format("BBot.GameDefinitions.Assets.{0}.bmp", assetName);
            String[] assetNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            if (!assetNames.Contains(fullAssetName))
                return null; // Asset not found, return null

            Bitmap returnBitmap;

            Stream ImageAsset = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullAssetName);
            using (Bitmap assetBitmap = (Bitmap)Bitmap.FromStream(ImageAsset))
            {
                if (!size.HasValue && !format.HasValue)
                    returnBitmap = assetBitmap.CloneExact();

                // Copy asset to correct size and format (this will add buffer of black pixels if required)
                using (Bitmap resizedAssetBitmap = new Bitmap(size.HasValue ? size.Value.Width : assetBitmap.Width,
                                                        size.HasValue ? size.Value.Height : assetBitmap.Height,
                                                        format.HasValue ? format.Value : assetBitmap.PixelFormat))
                {

                    using (Graphics g = Graphics.FromImage(resizedAssetBitmap))
                    {
                        g.DrawImage(assetBitmap, 0, 0, assetBitmap.Width, assetBitmap.Height);
                    }
                    returnBitmap = resizedAssetBitmap.CloneExact();
                }
            }

            return returnBitmap;
        }

    }
}
