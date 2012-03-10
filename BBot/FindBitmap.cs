using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AForge.Imaging.Filters;
using AForge.Imaging;



namespace BBot
{
    public class FindBitmap
    {
        private const bool Debug = true;

        public struct ImageSearchDetails
        {
            public double thresholdValue;
            public double currentValue;
            public double minimumValue;
            public double maxValue;
            public ImageSearchDetailsType type;

        }

        public enum ImageSearchDetailsType
        {
            MatchCertainty,
            CertaintyDelta

        }


        public delegate void ImageSearchDelegate(ImageSearchDetails details);
        public static event ImageSearchDelegate ImageSearchEvent;

        public static void UpdateCertainty(int searchCertainty, int minimumValue, int maxValue)
        {
            double minPercentMark = 10D;
            double scaleFactor = (1 - (1 / minPercentMark)) / Math.Log(maxValue);
            double adjFactor = ((1 / minPercentMark)) - (Math.Log(minimumValue) * scaleFactor);
            double currentValue = (100D * ((Math.Log(searchCertainty) * scaleFactor) + adjFactor));


            ImageSearchDetails details = new ImageSearchDetails();
            details.currentValue = currentValue;
            details.minimumValue = 0;
            details.maxValue = 100;
            details.thresholdValue = minPercentMark;
            details.type = ImageSearchDetailsType.MatchCertainty;

            if (ImageSearchEvent != null)
                ImageSearchEvent(details);
        }

        public static void UpdateDelta(double seachDelta)
        {
            double scaleFactor = (99D / 100);
            double currentValue = (Math.Max(seachDelta, 0) * scaleFactor) + 1D;

            ImageSearchDetails details = new ImageSearchDetails();
            details.currentValue = currentValue;
            details.minimumValue = 0;
            details.maxValue = 100;
            details.thresholdValue = 10;
            details.type = ImageSearchDetailsType.CertaintyDelta;

            if (ImageSearchEvent != null)
                ImageSearchEvent(details);
        }

        public static MatchingPoint FindInScreen(Bitmap bmpSource, Bitmap bmpToFind, Bitmap bmpMask, bool bQuickCheck, int minimumConfidence = int.MaxValue)
        {
            MatchingPoint match = new MatchingPoint();
            MatchingPoint lastMatch = new MatchingPoint();

            int minimumResolution = 1;

            lastMatch.Certainty = int.MaxValue;


            match.X = 0;
            match.Y = 0;
            match.MinimumCertainty = minimumConfidence;

            int step = 1;
            double scaleFactor = 1d;

            int xN = 0;
            int yN = 0;

            Bitmap bmpSourceScaled;
            Bitmap bmpToFindScaled;
            Bitmap bmpMaskScaled;

            // Work out if source is less than 1.3 times larger, and therefore need simple search


            // Otherwise check each successive zoom
            foreach (int scale in new int[] { 10, 4, 2, 1 })
            {
                scaleFactor = 1d / scale;
                // Resize both images to (1 / scale)%
                bmpSourceScaled = ResizeBitmap(bmpSource, scaleFactor);
                bmpToFindScaled = ResizeBitmap(bmpToFind, scaleFactor);
                bmpMaskScaled = (bmpMask != null) ? ResizeBitmap(bmpMask, scaleFactor) : null;

                step = (int)Math.Max(1, Math.Ceiling(4 * Math.Log(scale)));

                // Downscale found co-ordinates to 25% + buffer
                match.X = Math.Max((int)(match.X * scaleFactor - (step * 3)), 0);
                match.Y = Math.Max((int)(match.Y * scaleFactor - (step * 3)), 0);


                if (xN != 0)
                    xN = match.X + (step * 2 * 3);
                if (yN != 0)
                    yN = match.Y + (step * 2 * 3);

                // Find best match at 25%
                //[x,y] = findwindow2(fullscreen2,gametitle2,x,y,xn,yn,3);
                match = FindSingleChannelImaging(bmpSourceScaled, bmpToFindScaled, bmpMaskScaled, match, xN, yN, step, scale, bQuickCheck);

                if (bQuickCheck && match.MaxCertaintyDelta < 10)
                    return match; // Bail out, could not find

                if ((match.Certainty * 100D / lastMatch.Certainty) > (100 * scale))
                    return lastMatch;

                //if ((match.Confident && match.Resolution <= minimumResolution) || (match.Certainty > lastMatch.Certainty))
                if ((match.Confident && match.Resolution <= minimumResolution))
                    return match;

                lastMatch = match;
                // Reset both on next iteration
                xN = -1;
                yN = -1;
            }

            return match;
        }



        private static MatchingPoint FindSingleChannelImaging(Bitmap bmpSource, Bitmap bmpToFind, Bitmap bmpMask, MatchingPoint match, int xN, int yN, int stepSize, int scale = 1, bool bQuickCheck = true)
        {
            Subtract subFilter = (bmpMask != null) ? new Subtract(bmpMask) : null;

            if (subFilter != null)
                subFilter.ApplyInPlace(bmpToFind);
            Difference filter = new Difference(bmpToFind);

            Bitmap bmpDiff;
            
            // if Debug
            Bitmap bmpBestDiff;
            Bitmap bmpBestMatch;
            Bitmap bmpBestSearch;
            // \if Debug

            int xMin = 0;
            int yMin = 0;
            int maxValue = bmpToFind.Width * bmpToFind.Height * 256 * scale * scale;
            int testMin = maxValue;
            int lastTestAvg = maxValue;


            // Get width of gametitle
            int gWidth = bmpToFind.Width;
            int gHeight = bmpToFind.Height;

            // Get width of fullscreen
            int fWidth = bmpSource.Width;
            int fHeight = bmpSource.Height;

            match.X = Math.Max(match.X, 0);
            match.Y = Math.Max(match.Y, 0);


            if ((bmpSource.Width * bmpSource.Height) < (bmpToFind.Width * bmpToFind.Height * 1.2 * 1.2))
            { // Images are similar size
                stepSize = 1; // force step to each location
            }

            // Determine search limits
            if (xN == 0)
                xN = (fWidth - gWidth) - stepSize;
            else
                xN = Math.Min((fWidth - gWidth) - stepSize, xN);

            if (yN == 0)
                yN = (fHeight - gHeight) - stepSize;
            else
                yN = Math.Min((fHeight - gHeight) - stepSize, yN);

            int xConfDeltaCount = 0;
            int yConfDeltaCount = 0;
            int xSearchBreak = 8;
            int ySearchBreak = 8;



            int prevRoundMax = 0;
            int prevRoundMin = maxValue;

            int missedMinCount = 0;
            double deltaConfidence = 0;
            double maxDeltaConfidence = 0;
            // For each step in x
            for (int x = match.X; x < xN; x++, xConfDeltaCount++)
            {
                if (bQuickCheck && missedMinCount > xSearchBreak)
                    break;

                int currRoundMax = 0;
                int currRoundMin = int.MaxValue;

                //yConfDeltaCount = 0;
                int missedYMinCount = 0;
                // For each step in y
                for (int y = match.Y; y < yN; y++, yConfDeltaCount++)
                {
                    if (bQuickCheck && missedYMinCount > ySearchBreak)
                    {
                        break;
                    }
                    //    break;

                    Bitmap bmpSearchArea = bmpSource.Clone(new Rectangle(x, y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);

                    if (subFilter != null)
                        subFilter.ApplyInPlace(bmpSearchArea);

                    bmpDiff = filter.Apply(bmpSearchArea);

                    #region debug
                    Bitmap bmpRenderedDiff;
                    Bitmap bmpRenderedToFind;
                    Bitmap bmpRenderedSearch;
                    if (Debug)
                    {
                        bmpRenderedDiff = bmpSource.Clone(new Rectangle(x, y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);
                        bmpRenderedToFind = bmpSource.Clone(new Rectangle(x, y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);
                        bmpRenderedSearch = bmpSource.Clone(new Rectangle(x, y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);

                        using (Graphics g = Graphics.FromImage(bmpRenderedDiff))
                        {
                            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                            cm.Matrix43 = 1.0F;

                            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                            ia.SetColorMatrix(cm);

                            g.DrawImage(bmpDiff, new Rectangle(0, 0, bmpDiff.Width, bmpDiff.Height), 0, 0, bmpDiff.Width, bmpDiff.Height, GraphicsUnit.Pixel, ia);
                        }

                        using (Graphics g = Graphics.FromImage(bmpRenderedToFind))
                        {
                            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                            cm.Matrix43 = 1.0F;

                            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                            ia.SetColorMatrix(cm);

                            g.DrawImage(bmpToFind, new Rectangle(0, 0, bmpToFind.Width, bmpToFind.Height), 0, 0, bmpToFind.Width, bmpToFind.Height, GraphicsUnit.Pixel, ia);
                        }

                        using (Graphics g = Graphics.FromImage(bmpRenderedSearch))
                        {
                            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                            cm.Matrix43 = 1.0F;

                            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                            ia.SetColorMatrix(cm);

                            g.DrawImage(bmpSearchArea, new Rectangle(0, 0, bmpToFind.Width, bmpToFind.Height), 0, 0, bmpToFind.Width, bmpToFind.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                    #endregion

                    int testRed = 0;
                    int testBlue = 0;
                    int testGreen = 0;
                    ImageStatistics stats = new ImageStatistics(bmpDiff);
                    if (Debug)
                    {
                        ImageStatistics stats2 = new ImageStatistics(bmpRenderedDiff);
                        ImageStatistics stats3 = new ImageStatistics(bmpRenderedToFind);
                        ImageStatistics stats4 = new ImageStatistics(bmpToFind);
                    }
                    for (int i = 0; i < 256; i++)
                    {
                        testRed += stats.Red.Values[i] * i;
                        testGreen += stats.Green.Values[i] * i;
                        testBlue += stats.Blue.Values[i] * i;
                    }

                    int testAvg = (int)((testRed + testGreen + testBlue) / 3);

                    testAvg = testAvg * scale * scale;
                    FindBitmap.UpdateCertainty(testAvg, match.MinimumCertainty, maxValue);


                    // Work out positioning
                    currRoundMin = Math.Min(testAvg, currRoundMin);
                    currRoundMax = Math.Max(testAvg, currRoundMax);

                    if (testAvg > currRoundMin)
                        missedYMinCount++;
                    else
                        missedYMinCount = 0;

                    deltaConfidence = (100D - (testAvg * 100F / prevRoundMin));

                    if (prevRoundMin != maxValue)
                    {
                        maxDeltaConfidence = Math.Max(deltaConfidence, maxDeltaConfidence);

                    }
                    else
                        deltaConfidence = 0;

                    FindBitmap.UpdateDelta(deltaConfidence);
                    if (testAvg < testMin)
                    { // If smaller than previous min, use new values
                        if (Debug)
                        {
                            bmpBestDiff = bmpRenderedDiff;
                            bmpBestMatch = bmpSource.Clone(new Rectangle(x, y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);
                            bmpBestSearch = bmpRenderedSearch;
                        }
                        xMin = x;
                        yMin = y;
                        testMin = testAvg;
                        
                    }
                }

                if (currRoundMin > prevRoundMin)
                    missedMinCount++;

                prevRoundMax = currRoundMax;
                prevRoundMin = currRoundMin;



            }

            match.X = xMin * scale;
            match.Y = yMin * scale;
            match.MaxCertaintyDelta = maxDeltaConfidence;
            match.Certainty = (maxDeltaConfidence > 10) ? testMin : maxValue;
            match.Confident = (testMin <= match.MinimumCertainty);
            match.Resolution = stepSize * scale;

            return match;

        }


        public static MatchingPoint CheckExactMatch(Bitmap bmpSource, Bitmap bmpToFind, Bitmap bmpMask, int minimumCertainty)
        {
            MatchingPoint match = new MatchingPoint();

            match.X = 0;
            match.Y = 0;
            match.MinimumCertainty = minimumCertainty;

            Subtract subFilter = (bmpMask != null) ? new Subtract(bmpMask) : null;

            if (subFilter != null)
                subFilter.ApplyInPlace(bmpToFind);
            Difference filter = new Difference(bmpToFind);

            Bitmap bmpDiff;

            if (subFilter != null)
                subFilter.ApplyInPlace(bmpSource);

            bmpDiff = filter.Apply(bmpSource);

            #region debug
            Bitmap bmpRenderedDiff;
            Bitmap bmpRenderedToFind;
            if (Debug)
            {
                bmpRenderedDiff = bmpSource.Clone(new Rectangle(match.X, match.Y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);
                bmpRenderedToFind = bmpSource.Clone(new Rectangle(match.X, match.Y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);

                using (Graphics g = Graphics.FromImage(bmpRenderedDiff))
                {
                    System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                    cm.Matrix43 = 1.0F;

                    System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                    ia.SetColorMatrix(cm);

                    g.DrawImage(bmpDiff, new Rectangle(0, 0, bmpDiff.Width, bmpDiff.Height), 0, 0, bmpDiff.Width, bmpDiff.Height, GraphicsUnit.Pixel, ia);
                }

                using (Graphics g = Graphics.FromImage(bmpRenderedToFind))
                {
                    System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                    cm.Matrix43 = 1.0F;

                    System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                    ia.SetColorMatrix(cm);

                    g.DrawImage(bmpToFind, new Rectangle(0, 0, bmpToFind.Width, bmpToFind.Height), 0, 0, bmpToFind.Width, bmpToFind.Height, GraphicsUnit.Pixel, ia);
                }
            }
            #endregion

            int testRed = 0;
            int testBlue = 0;
            int testGreen = 0;
            ImageStatistics stats = new ImageStatistics(bmpDiff);
            if (Debug)
            {
                ImageStatistics stats2 = new ImageStatistics(bmpRenderedDiff);
                ImageStatistics stats3 = new ImageStatistics(bmpRenderedToFind);
                ImageStatistics stats4 = new ImageStatistics(bmpToFind);
            }
            for (int i = 0; i < 256; i++)
            {
                testRed += stats.Red.Values[i] * i;
                testGreen += stats.Green.Values[i] * i;
                testBlue += stats.Blue.Values[i] * i;
            }

            int testAvg = (int)((testRed + testGreen + testBlue) / 3);

            match.Certainty = testAvg;
            match.Confident = (testAvg <= match.MinimumCertainty);
            match.Resolution = 1;

            return match;

        }


        public static MatchingPoint CheckExactMatchReverse(Bitmap bmpSource, Bitmap bmpToFind, Bitmap bmpMask, int minimumCertainty)
        {
            MatchingPoint match = new MatchingPoint();

            match.X = 0;
            match.Y = 0;
            match.MinimumCertainty = minimumCertainty;

            Subtract subFilter = (bmpMask != null) ? new Subtract(bmpMask) : null;

            if (subFilter != null)
                subFilter.ApplyInPlace(bmpToFind);
            Difference filter = new Difference(bmpToFind);

            Bitmap bmpDiff;

            if (subFilter != null)
                subFilter.ApplyInPlace(bmpSource);

            bmpDiff = filter.Apply(bmpSource);

            #region debug
            Bitmap bmpRenderedDiff;
            Bitmap bmpRenderedToFind;
            if (Debug)
            {
                bmpRenderedDiff = bmpSource.Clone(new Rectangle(match.X, match.Y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);
                bmpRenderedToFind = bmpSource.Clone(new Rectangle(match.X, match.Y, bmpToFind.Width, bmpToFind.Height), bmpToFind.PixelFormat);

                using (Graphics g = Graphics.FromImage(bmpRenderedDiff))
                {
                    System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                    cm.Matrix43 = 1.0F;

                    System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                    ia.SetColorMatrix(cm);

                    g.DrawImage(bmpDiff, new Rectangle(0, 0, bmpDiff.Width, bmpDiff.Height), 0, 0, bmpDiff.Width, bmpDiff.Height, GraphicsUnit.Pixel, ia);
                }

                using (Graphics g = Graphics.FromImage(bmpRenderedToFind))
                {
                    System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                    cm.Matrix43 = 1.0F;

                    System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                    ia.SetColorMatrix(cm);

                    g.DrawImage(bmpToFind, new Rectangle(0, 0, bmpToFind.Width, bmpToFind.Height), 0, 0, bmpToFind.Width, bmpToFind.Height, GraphicsUnit.Pixel, ia);
                }
            }
            #endregion

            int testRed = 0;
            int testBlue = 0;
            int testGreen = 0;
            ImageStatistics stats = new ImageStatistics(bmpDiff);
            if (Debug)
            {
                ImageStatistics stats2 = new ImageStatistics(bmpRenderedDiff);
                ImageStatistics stats3 = new ImageStatistics(bmpRenderedToFind);
                ImageStatistics stats4 = new ImageStatistics(bmpToFind);
            }
            for (int i = 0; i < 256; i++)
            {
                testRed += stats.Red.Values[i] * i;
                testGreen += stats.Green.Values[i] * i;
                testBlue += stats.Blue.Values[i] * i;
            }

            int testAvg = (int)((testRed + testGreen + testBlue) / 3);

            match.Certainty = testAvg;
            match.Confident = (testAvg <= match.MinimumCertainty);
            match.Resolution = 1;

            return match;

        }

        private static int CheckMatch(int x, int y, int[,] fullScreen, int[,] gameTitle)
        {
            int match = 0;

            int gWidth = gameTitle.GetLength(0);
            int gHeight = gameTitle.GetLength(1);

            // Get subset of fullscreen to compare
            int[,] subtracted = new int[gWidth, gHeight];

            // Subtract title portion form fullscreen
            for (int i = 0; i < gWidth; i++)
            {
                for (int j = 0; j < gHeight; j++)
                {
                    subtracted[i, j] = Math.Abs(fullScreen[i + x, j + y] - gameTitle[i, j]);
                }
            }

            // Sum all pixel values
            for (int i = 0; i < gWidth; i++)
            {
                for (int j = 0; j < gHeight; j++)
                {
                    match += subtracted[i, j];
                }
            }

            return match;

        }

        private static int[,] GetRedChannel(Bitmap bmpSource)
        {
            Rectangle r2 = new Rectangle(0, 0, bmpSource.Width, bmpSource.Height);


            int[,] arrChannel = new int[r2.Width, r2.Height];
            for (int i = 0; i < r2.Width; i++)
            {
                for (int j = 0; j < r2.Height; j++)
                {
                    Color pixelColor = bmpSource.GetPixel(i, j);
                    arrChannel[i, j] = pixelColor.R;
                }
            }

            return arrChannel;

        }

        private static Bitmap ResizeBitmap(Bitmap bmpSource, double scaleFactor)
        {
            Bitmap scaled = new Bitmap((int)(bmpSource.Width * scaleFactor), (int)(bmpSource.Height * scaleFactor));
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.DrawImage(bmpSource, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }

            return scaled;
        }

        public struct MatchingPoint
        {
            public int X;
            public int Y;
            public int Certainty;
            public int MinimumCertainty;
            public double MaxCertaintyDelta;
            public bool Confident;
            public int Resolution;
        }
    }
}
