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
    [Serializable()]
    public class GemDefinitionList
    {
        public GemDefinitionList() { Items = new List<Gem>(); }
        [XmlElement("gem")]
        public List<Gem> Items { get; set; }
    }

    public struct Gem
    {
        public GemColor Name { get; set; }
        public Color DisplayColor
        {
            get
            {
                Color displayColor = Color.Black;

                switch (this.Name)
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
        }

        public GemModifier Modifiers { get; set; }

        [XmlIgnore()]
        public Color Color { get; set; }

        [XmlElement("Color")]
        public string ColorRGB
        {
            get
            {
                return string.Format("{0}:{1}:{2}:{3}",
                    Color.A, Color.R, Color.G, Color.B);
            }
            set
            {
                byte a, r, g, b;

                string[] pieces = value.Split(new char[] { ':' });

                a = byte.Parse(pieces[0]);
                r = byte.Parse(pieces[1]);
                g = byte.Parse(pieces[2]);
                b = byte.Parse(pieces[3]);

                Color = Color.FromArgb(a, r, g, b);
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


    public static class GemDefinitions
    {
        public static void SerializeDefinition( List<Gem> listGems, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(GemDefinitionList));
            GemDefinitionList list = new GemDefinitionList();
            list.Items.AddRange(listGems);

            
            
            using (StreamWriter writer = new StreamWriter(path))
            {
                ser.Serialize(writer, list);
            }

        }

        public static List<Gem> DeserializeDefinition(string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(GemDefinitionList));
            GemDefinitionList gemList = new GemDefinitionList();
            
            StringBuilder sb = new StringBuilder();

            // Check path exists
            if (!File.Exists(path))
                return gemList.Items;

            try
            {
                using (StreamReader reader = new StreamReader(path))
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
