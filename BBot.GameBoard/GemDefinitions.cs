using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;


namespace BBot.GameDefinitions
{
    public static class BoardDefinition
    {

        public static Bitmap GenerateBoardImage(GameBoard Board)
        {
            Bitmap bmpBoardGems = new Bitmap(Board.BoardSize.Width, Board.BoardSize.Height);

            
            /*
            using (Graphics g = Graphics.FromImage(bmpBoardGems))
            {
                // Across
                for (int x = 0; x < Board.GridSize; x++)
                {
                    // Down
                    for (int y = 0; y < Board.GridSize; y++)
                    {
                        Gem boardGem = Board.Board[x + 3, y + 3];



                        g.FillRectangle(new SolidBrush(GemDefinitions.GetDisplayColorForGem(boardGem.Name)), x * Board.Cellsize, y * Board.Cellsize, Board.Cellsize, Board.Cellsize);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Star))
                            g.FillRectangle(new SolidBrush(Color.White), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Hypercube))
                            g.FillRectangle(new SolidBrush(Color.LimeGreen), x * Board.Cellsize, y * Board.Cellsize, Board.Cellsize, Board.Cellsize);


                        if (boardGem.Modifiers.HasFlag(GemModifier.Multiplier))
                            g.FillRectangle(new SolidBrush(Color.HotPink), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Background))
                            g.FillRectangle(new SolidBrush(Color.Black), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Coin))
                            g.FillRectangle(new SolidBrush(Color.Yellow), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);
                    }
                }
            }*/

            return bmpBoardGems;

        }
    }


    [Serializable()]
    public class GemDefinitionList
    {
        public GemDefinitionList() { Items = new List<Gem>(); }
        [XmlElement("gem")]
        public List<Gem> Items { get; set; }
    }

    public enum GemColor
    {
        None,
        White,
        Purple,
        Blue,
        Green,
        Yellow,
        Orange,
        Red
    }

    [Flags]
    public enum GemModifier
    {
        None = 0,
        ExtraColor = 2,
        Flame = 4,
        Star = 8,
        Multiplier = 16,
        Coin = 32,
        Background = 64,
        Hypercube = 128
    }

    public struct Gem
    {
        public GemColor Name { get; set; }

        public GemModifier Modifiers { get; set; }

        [XmlIgnore()]
        public Color Color { get; set; }

        [XmlElement("Color")]
        public string ColorRGB
        {
            get
            {
                return GemDefinitions.SerializeColor(Color);
            }
            set
            {
                Color = GemDefinitions.DeserializeColor(value);
            }
        }

        public bool Equals(Gem gemCompare)
        {
            bool ColorsEqual = false;

            if (gemCompare.Modifiers.HasFlag(GemModifier.Hypercube) || this.Modifiers.HasFlag(GemModifier.Hypercube))
                ColorsEqual = true;

            if (gemCompare.Name == this.Name)
                ColorsEqual = true;

            if (gemCompare.Modifiers.HasFlag(GemModifier.Background) || this.Modifiers.HasFlag(GemModifier.Background))
                ColorsEqual = false;

            if (gemCompare.Name == GemColor.None || this.Name == GemColor.None)
                ColorsEqual = false;

            return ColorsEqual;
        }
    }

    public static class GemDefinitions
    {

        public static Color GetDisplayColorForGem(GemColor gemColor)
        {
            Color displayColor = Color.Black;

            switch (gemColor)
            {
                case GemColor.Green:
                    displayColor = Color.DarkGreen;
                    break;
                case GemColor.Blue:
                    displayColor = Color.DarkBlue;
                    break;
                case GemColor.Orange:
                    displayColor = Color.DarkOrange;
                    break;
                case GemColor.Purple:
                    displayColor = Color.Purple;
                    break;
                case GemColor.Red:
                    displayColor = Color.DarkRed;
                    break;
                case GemColor.White:
                    displayColor = Color.LightGray;
                    break;
                case GemColor.Yellow:
                    displayColor = Color.LemonChiffon;
                    break;
            }

            return displayColor;
        }

        public static string SerializeColor(Color color)
        {
                return string.Format("{0}:{1}:{2}:{3}",
                    color.A, color.R, color.G, color.B);
        }

        public static Color DeserializeColor(string color)
        {
            byte a, r, g, b;

            string[] pieces = color.Split(new char[] { ':' });

            a = byte.Parse(pieces[0]);
            r = byte.Parse(pieces[1]);
            g = byte.Parse(pieces[2]);
            b = byte.Parse(pieces[3]);

            return Color.FromArgb(a, r, g, b);
            
        }

        public static List<Gem> GetGemDefinitions()
        {
            // Get definition from file
            List<Gem> listGemColorStats = DeserializeDefinition();

            if (listGemColorStats.Count > 0)
                return listGemColorStats;

            listGemColorStats.Add(new Gem{ Name = GemColor.White, Modifiers = GemModifier.None, Color = Color.FromArgb(165, 165, 165)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Purple, Modifiers = GemModifier.None, Color = Color.FromArgb(135, 28, 129)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Orange, Modifiers = GemModifier.None, Color = Color.FromArgb(192, 104, 37)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Red, Modifiers = GemModifier.None, Color = Color.FromArgb(204, 18, 38)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Yellow, Modifiers = GemModifier.None, Color = Color.FromArgb(160, 136, 30)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Green, Modifiers = GemModifier.None, Color = Color.FromArgb(30, 180, 58)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Blue, Modifiers = GemModifier.None, Color = Color.FromArgb(20, 90, 155)});

            
            // Star gems
            listGemColorStats.Add(new Gem{ Name = GemColor.Red, Modifiers = GemModifier.Star, Color = Color.FromArgb(195, 60, 95)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Yellow, Modifiers = GemModifier.Star, Color = Color.FromArgb(207, 173, 46)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Green, Modifiers = GemModifier.Star, Color = Color.FromArgb(65, 189, 115)});
           
            // Flaming gems
            listGemColorStats.Add(new Gem{ Name = GemColor.Blue, Modifiers = GemModifier.Flame, Color = Color.FromArgb(75, 120, 154)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Orange, Modifiers = GemModifier.Flame, Color = Color.FromArgb(231, 146, 61)});
            listGemColorStats.Add(new Gem{ Name = GemColor.White, Modifiers = GemModifier.Flame, Color = Color.FromArgb(206, 195, 172)});
            

            // Coin gems
            listGemColorStats.Add(new Gem{ Name = GemColor.Yellow, Modifiers = GemModifier.Coin, Color = Color.FromArgb(139, 115, 42)});

            // Multipliers
            listGemColorStats.Add(new Gem{ Name = GemColor.Orange, Modifiers = GemModifier.Multiplier, Color = Color.FromArgb(197, 122, 66)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Purple, Modifiers = GemModifier.Multiplier, Color = Color.FromArgb(162, 76, 166)});
            listGemColorStats.Add(new Gem{ Name = GemColor.Green, Modifiers = GemModifier.Multiplier, Color = Color.FromArgb(59, 153, 62)});

            // Lightning animation
            // - none, incorrectly matches white star

            // Write out default definition
            SerializeDefinition(listGemColorStats);


            return listGemColorStats;
        }

        private static string GetPath()
        {
            return System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "gemDefinitions.xml");
        }

        private static void SerializeDefinition( List<Gem> listGems)
        {
            XmlSerializer ser = new XmlSerializer(typeof(GemDefinitionList));
            GemDefinitionList list = new GemDefinitionList();
            list.Items.AddRange(listGems);

            
            
            using (StreamWriter writer = new StreamWriter(GetPath()))
            {
                ser.Serialize(writer, list);
            }

        }

        private static List<Gem> DeserializeDefinition()
        {
            XmlSerializer ser = new XmlSerializer(typeof(GemDefinitionList));
            GemDefinitionList gemList = new GemDefinitionList();
            
            StringBuilder sb = new StringBuilder();

            // Check path exists
            if (!File.Exists(GetPath()))
                return gemList.Items;

            try
            {
                using (StreamReader reader = new StreamReader(GetPath()))
                {
                    gemList = (GemDefinitionList)ser.Deserialize(reader);
                }
            }
            catch (Exception)
            { }

            return gemList.Items;
        }



    }
}
