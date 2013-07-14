using System.Collections.Generic;

namespace Composer.Infrastructure
{
    public static class Slot
    {
        public static List<_Slot> Slots = new List<_Slot>();
        public static IDictionary<int, int> Normalize_Y = new Dictionary<int, int>();
        public static IDictionary<string, int> LedgerMap = new Dictionary<string, int>();
        public static IDictionary<string, _Enum.Orientation> OrientationMap = new Dictionary<string, _Enum.Orientation>();
        public static IDictionary<int, string> Slot_Y = new Dictionary<int, string>();

        static Slot()
        {
            InitializeYCoordinateSlotMap();
            initializeYCoordinateSlotTranspositionMap();
            InitializeYCoordinateSlotNormalizationMap();
            InitializeSlotOrientationMap();
            initializeLedgerMap();
        }

        private static void InitializeYCoordinateSlotNormalizationMap()
        {
            Normalize_Y.Add(7, 9);
            Normalize_Y.Add(8, 9);
            Normalize_Y.Add(9, 9);
            Normalize_Y.Add(10, 9);

            Normalize_Y.Add(11, 13);
            Normalize_Y.Add(12, 13);
            Normalize_Y.Add(13, 13);
            Normalize_Y.Add(14, 13);

            Normalize_Y.Add(15, 17);
            Normalize_Y.Add(16, 17);
            Normalize_Y.Add(17, 17);
            Normalize_Y.Add(18, 17);

            Normalize_Y.Add(19, 21);
            Normalize_Y.Add(20, 21);
            Normalize_Y.Add(21, 21);
            Normalize_Y.Add(22, 21);

            Normalize_Y.Add(23, 25);
            Normalize_Y.Add(24, 25);
            Normalize_Y.Add(25, 25);
            Normalize_Y.Add(26, 25);

            Normalize_Y.Add(27, 29);
            Normalize_Y.Add(28, 29);
            Normalize_Y.Add(29, 29);
            Normalize_Y.Add(30, 29);

            Normalize_Y.Add(31, 33);
            Normalize_Y.Add(32, 33);
            Normalize_Y.Add(33, 33);
            Normalize_Y.Add(34, 33);

            Normalize_Y.Add(35, 37);
            Normalize_Y.Add(36, 37);
            Normalize_Y.Add(37, 37);
            Normalize_Y.Add(38, 37);

            Normalize_Y.Add(39, 41);
            Normalize_Y.Add(40, 41);
            Normalize_Y.Add(41, 41);
            Normalize_Y.Add(42, 41);

            Normalize_Y.Add(43, 45);
            Normalize_Y.Add(44, 45);
            Normalize_Y.Add(45, 45);
            Normalize_Y.Add(46, 45);

            Normalize_Y.Add(47, 49);
            Normalize_Y.Add(48, 49);
            Normalize_Y.Add(49, 49);
            Normalize_Y.Add(50, 49);

            Normalize_Y.Add(51, 53);
            Normalize_Y.Add(52, 53);
            Normalize_Y.Add(53, 53);
            Normalize_Y.Add(54, 53);

            Normalize_Y.Add(55, 57);
            Normalize_Y.Add(56, 57);
            Normalize_Y.Add(57, 57);
            Normalize_Y.Add(58, 57);

            Normalize_Y.Add(59, 61);
            Normalize_Y.Add(60, 61);
            Normalize_Y.Add(61, 61);
            Normalize_Y.Add(62, 61);

            Normalize_Y.Add(63, 65);
            Normalize_Y.Add(64, 65);
            Normalize_Y.Add(65, 65);
            Normalize_Y.Add(66, 65);

            Normalize_Y.Add(67, 69);
            Normalize_Y.Add(68, 69);
            Normalize_Y.Add(69, 69);
            Normalize_Y.Add(70, 69);

            Normalize_Y.Add(71, 73);
            Normalize_Y.Add(72, 73);
            Normalize_Y.Add(73, 73);
            Normalize_Y.Add(74, 73);

            Normalize_Y.Add(75, 77);
            Normalize_Y.Add(76, 77);
            Normalize_Y.Add(77, 77);
            Normalize_Y.Add(78, 77);

            Normalize_Y.Add(79, 81);
            Normalize_Y.Add(80, 81);
            Normalize_Y.Add(81, 81);
            Normalize_Y.Add(82, 81);

            Normalize_Y.Add(83, 85);
            Normalize_Y.Add(84, 85);
            Normalize_Y.Add(85, 85);
            Normalize_Y.Add(86, 85);

            Normalize_Y.Add(87, 89);
            Normalize_Y.Add(88, 89);
            Normalize_Y.Add(89, 89);
            Normalize_Y.Add(90, 89);

            Normalize_Y.Add(91, 93);
            Normalize_Y.Add(92, 93);
            Normalize_Y.Add(93, 93);
            Normalize_Y.Add(94, 93);

            Normalize_Y.Add(95, 97);
            Normalize_Y.Add(96, 97);
            Normalize_Y.Add(97, 97);
            Normalize_Y.Add(98, 97);

            Normalize_Y.Add(99, 101);
            Normalize_Y.Add(100, 101);
            Normalize_Y.Add(101, 101);
            Normalize_Y.Add(102, 101);

            Normalize_Y.Add(103, 105);
            Normalize_Y.Add(104, 105);
            Normalize_Y.Add(105, 105);
            Normalize_Y.Add(106, 105);

            Normalize_Y.Add(107, 109);
            Normalize_Y.Add(108, 109);
            Normalize_Y.Add(109, 109);
            Normalize_Y.Add(110, 109);

            Normalize_Y.Add(111, 113);
            Normalize_Y.Add(112, 113);
            Normalize_Y.Add(113, 113);
            Normalize_Y.Add(114, 113);

            Normalize_Y.Add(115, 117);
            Normalize_Y.Add(116, 117);
            Normalize_Y.Add(117, 117);
            Normalize_Y.Add(118, 117);

            Normalize_Y.Add(119, 121);
            Normalize_Y.Add(120, 121);
            Normalize_Y.Add(121, 121);
            Normalize_Y.Add(122, 121);

            Normalize_Y.Add(123, 125);
            Normalize_Y.Add(124, 125);
            Normalize_Y.Add(125, 125);
            Normalize_Y.Add(126, 125);

            Normalize_Y.Add(127, 129);
            Normalize_Y.Add(128, 129);
            Normalize_Y.Add(129, 129);
            Normalize_Y.Add(130, 129);

            Normalize_Y.Add(131, 133);
            Normalize_Y.Add(132, 133);
            Normalize_Y.Add(133, 133);
            Normalize_Y.Add(134, 133);

            Normalize_Y.Add(135, 137);
            Normalize_Y.Add(136, 137);
            Normalize_Y.Add(137, 137);
            Normalize_Y.Add(138, 137);

            Normalize_Y.Add(139, 141);
            Normalize_Y.Add(140, 141);
            Normalize_Y.Add(141, 141);
            Normalize_Y.Add(142, 141);
        }

        private static void InitializeYCoordinateSlotMap()
        {
            Slot_Y.Add(9, "65,A");
            Slot_Y.Add(13, "64,G");
            Slot_Y.Add(17, "63,F");
            Slot_Y.Add(21, "62,E");
            Slot_Y.Add(25, "61,D");
            Slot_Y.Add(29, "60,C");
            Slot_Y.Add(33, "56,B");
            Slot_Y.Add(37, "55,A");
            Slot_Y.Add(41, "54,G");
            Slot_Y.Add(45, "53,F");
            Slot_Y.Add(49, "52,E");
            Slot_Y.Add(53, "51,D");
            Slot_Y.Add(57, "50,C");
            Slot_Y.Add(61, "46,B");
            Slot_Y.Add(65, "45,A");
            Slot_Y.Add(69, "44,G");
            Slot_Y.Add(73, "43,F");
            Slot_Y.Add(77, "42,E");
            Slot_Y.Add(81, "41,D");
            Slot_Y.Add(85, "40,C");
            Slot_Y.Add(89, "36,B");
            Slot_Y.Add(93, "35,A");
            Slot_Y.Add(97, "34,G");
            Slot_Y.Add(101, "33,F");
            Slot_Y.Add(105, "32,E");
            Slot_Y.Add(109, "31,D");
            Slot_Y.Add(113, "30,C");
            Slot_Y.Add(117, "26,B");
            Slot_Y.Add(121, "25,A");
            Slot_Y.Add(125, "24,G");
            Slot_Y.Add(129, "23,F");
            Slot_Y.Add(133, "22,E");
            Slot_Y.Add(137, "21,D");
            Slot_Y.Add(141, "20,C");
        }
        private static void initializeYCoordinateSlotTranspositionMap()
        {
            Slots.Add(new _Slot(9, 65, "A", 9, new Enharmonic("A", "")));
            Slots.Add(new _Slot(9, 65, "Ab", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(13, 64, "Gs", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(13, 64, "G", 7, new Enharmonic("G", "")));
            Slots.Add(new _Slot(13, 64, "Gb", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(17, 63, "Fs", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(17, 63, "F", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(21, 62, "Es", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(17, 63, "Fb", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(21, 62, "E", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(21, 62, "Eb", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(25, 61, "Ds", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(25, 61, "D", 2, new Enharmonic("D", "")));
            Slots.Add(new _Slot(25, 61, "Db", 1, new Enharmonic("Db", "Cs")));
            Slots.Add(new _Slot(29, 60, "Cs", 1, new Enharmonic("B", "Cs")));
            Slots.Add(new _Slot(29, 60, "C", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(29, 56, "Cb", 11, new Enharmonic("B", "Cb")));

            Slots.Add(new _Slot(33, 56, "Bs", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(33, 56, "B", 11, new Enharmonic("B", "Cb")));
            Slots.Add(new _Slot(33, 56, "Bb", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(37, 55, "As", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(37, 55, "A", 9, new Enharmonic("A", "")));
            Slots.Add(new _Slot(37, 55, "Ab", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(41, 54, "Gs", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(41, 54, "G", 7, new Enharmonic("G", "")));
            Slots.Add(new _Slot(41, 54, "Gb", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(45, 53, "Fs", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(45, 53, "F", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(49, 52, "Es", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(45, 53, "Fb", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(49, 52, "E", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(49, 52, "Eb", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(53, 51, "Ds", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(53, 51, "D", 2, new Enharmonic("D", "")));
            Slots.Add(new _Slot(53, 51, "Db", 1, new Enharmonic("Db", "Cs")));
            Slots.Add(new _Slot(57, 50, "Cs", 1, new Enharmonic("B", "Cs")));
            Slots.Add(new _Slot(57, 50, "C", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(57, 46, "Cb", 11, new Enharmonic("B", "Cb")));

            Slots.Add(new _Slot(61, 46, "Bs", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(61, 46, "B", 11, new Enharmonic("B", "Cb")));
            Slots.Add(new _Slot(61, 46, "Bb", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(65, 45, "As", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(65, 45, "A", 9, new Enharmonic("A", "")));
            Slots.Add(new _Slot(65, 45, "Ab", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(69, 44, "Gs", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(69, 44, "G", 7, new Enharmonic("G", "")));
            Slots.Add(new _Slot(69, 44, "Gb", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(73, 43, "Fs", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(73, 43, "F", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(77, 42, "Es", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(73, 43, "Fb", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(77, 42, "E", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(77, 42, "Eb", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(81, 41, "Ds", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(81, 41, "D", 2, new Enharmonic("D", "")));
            Slots.Add(new _Slot(81, 41, "Db", 1, new Enharmonic("Db", "Cs")));
            Slots.Add(new _Slot(85, 40, "Cs", 1, new Enharmonic("B", "Cs")));
            Slots.Add(new _Slot(85, 40, "C", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(85, 36, "Cb", 11, new Enharmonic("B", "Cb")));
            
            Slots.Add(new _Slot(89, 36, "Bs", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(89, 36, "B", 11, new Enharmonic("B", "Cb")));
            Slots.Add(new _Slot(89, 36, "Bb", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(93, 35, "As", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(93, 35, "A", 9, new Enharmonic("A", "")));
            Slots.Add(new _Slot(93, 35, "Ab", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(97, 34, "Gs", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(97, 34, "G", 7, new Enharmonic("G", "")));
            Slots.Add(new _Slot(97, 34, "Gb", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(101, 33, "Fs", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(101, 33, "F", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(105, 32, "Es", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(101, 33, "Fb", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(105, 32, "E", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(105, 32, "Eb", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(109, 31, "Ds", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(109, 31, "D", 2, new Enharmonic("D", "")));
            Slots.Add(new _Slot(109, 31, "Db", 1, new Enharmonic("Db", "Cs")));
            Slots.Add(new _Slot(113, 30, "Cs", 1, new Enharmonic("B", "Cs")));
            Slots.Add(new _Slot(113, 30, "C", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(113, 26, "Cb", 11, new Enharmonic("B", "Cb")));

            Slots.Add(new _Slot(117, 26, "Bs", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(117, 26, "B", 11, new Enharmonic("B", "Cb")));
            Slots.Add(new _Slot(117, 26, "Bb", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(121, 25, "As", 10, new Enharmonic("Bb", "As")));
            Slots.Add(new _Slot(121, 25, "A", 9, new Enharmonic("A", "")));
            Slots.Add(new _Slot(121, 25, "Ab", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(125, 24, "Gs", 8, new Enharmonic("Ab", "Gs")));
            Slots.Add(new _Slot(125, 24, "G", 7, new Enharmonic("G", "")));
            Slots.Add(new _Slot(125, 24, "Gb", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(129, 23, "Fs", 6, new Enharmonic("Fs", "Gb")));
            Slots.Add(new _Slot(129, 23, "F", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(133, 22, "Es", 5, new Enharmonic("F", "Es")));
            Slots.Add(new _Slot(129, 23, "Fb", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(133, 22, "E", 4, new Enharmonic("E", "Fb")));
            Slots.Add(new _Slot(133, 22, "Eb", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(137, 21, "Ds", 3, new Enharmonic("Eb", "Ds")));
            Slots.Add(new _Slot(137, 21, "D", 2, new Enharmonic("D", "")));
            Slots.Add(new _Slot(137, 21, "Db", 1, new Enharmonic("Db", "Cs")));
            Slots.Add(new _Slot(141, 20, "Cs", 1, new Enharmonic("B", "Cs")));
            Slots.Add(new _Slot(141, 20, "C", 0, new Enharmonic("C", "Bs")));
            Slots.Add(new _Slot(141, 16, "Cb", 11, new Enharmonic("B", "Cb")));
        }

        private static void InitializeSlotOrientationMap()
        {
            OrientationMap.Add("65", _Enum.Orientation.Down);
            OrientationMap.Add("64", _Enum.Orientation.Down);
            OrientationMap.Add("63", _Enum.Orientation.Down);
            OrientationMap.Add("62", _Enum.Orientation.Down);
            OrientationMap.Add("61", _Enum.Orientation.Down);
            OrientationMap.Add("60", _Enum.Orientation.Down);
            OrientationMap.Add("56", _Enum.Orientation.Down);
            OrientationMap.Add("55", _Enum.Orientation.Down);
            OrientationMap.Add("54", _Enum.Orientation.Down);
            OrientationMap.Add("53", _Enum.Orientation.Down);
            OrientationMap.Add("52", _Enum.Orientation.Down);
            OrientationMap.Add("51", _Enum.Orientation.Down);
            OrientationMap.Add("50", _Enum.Orientation.Down);
            OrientationMap.Add("46", _Enum.Orientation.Up);
            OrientationMap.Add("45", _Enum.Orientation.Up);
            OrientationMap.Add("44", _Enum.Orientation.Up);
            OrientationMap.Add("43", _Enum.Orientation.Up);
            OrientationMap.Add("42", _Enum.Orientation.Up);
            OrientationMap.Add("41", _Enum.Orientation.Up);
            OrientationMap.Add("40", _Enum.Orientation.Up);
            OrientationMap.Add("36", _Enum.Orientation.Up);
            OrientationMap.Add("35", _Enum.Orientation.Up);
            OrientationMap.Add("34", _Enum.Orientation.Up);
            OrientationMap.Add("33", _Enum.Orientation.Up);
            OrientationMap.Add("32", _Enum.Orientation.Up);
            OrientationMap.Add("31", _Enum.Orientation.Up);
            OrientationMap.Add("30", _Enum.Orientation.Up);
            OrientationMap.Add("26", _Enum.Orientation.Up);
            OrientationMap.Add("25", _Enum.Orientation.Up);
            OrientationMap.Add("24", _Enum.Orientation.Up);
            OrientationMap.Add("23", _Enum.Orientation.Up);
            OrientationMap.Add("22", _Enum.Orientation.Up);
            OrientationMap.Add("21", _Enum.Orientation.Up);
            OrientationMap.Add("20", _Enum.Orientation.Up);
        }

        private static void initializeLedgerMap()
        {
            LedgerMap.Add("70", 5);
            LedgerMap.Add("66", 4);
            LedgerMap.Add("65", 4);
            LedgerMap.Add("64", 3);
            LedgerMap.Add("63", 5);
            LedgerMap.Add("62", 5);
            LedgerMap.Add("61", 5);
            LedgerMap.Add("60", 4);
            LedgerMap.Add("56", 4);
            LedgerMap.Add("55", 4);
            LedgerMap.Add("54", 3);
            LedgerMap.Add("53", 3);
            LedgerMap.Add("52", 2);
            LedgerMap.Add("51", 2);
            LedgerMap.Add("50", 1);
            LedgerMap.Add("46", 1);
            LedgerMap.Add("45", 0);
            LedgerMap.Add("44", 0);
            LedgerMap.Add("43", 0);
            LedgerMap.Add("42", 0);
            LedgerMap.Add("41", 0);
            LedgerMap.Add("40", 0);
            LedgerMap.Add("36", 0);
            LedgerMap.Add("35", 0);
            LedgerMap.Add("34", 0);
            LedgerMap.Add("33", -1);
            LedgerMap.Add("32", -1);
            LedgerMap.Add("31", -2);
            LedgerMap.Add("30", -2);
            LedgerMap.Add("26", -3);
            LedgerMap.Add("25", -3);
            LedgerMap.Add("24", -4);
            LedgerMap.Add("23", -4);
            LedgerMap.Add("22", -5);
            LedgerMap.Add("21", -5);
            LedgerMap.Add("20", -5);
        }

        public class _Slot
        {
            public int Location_Y { get; set; }
            public int Value { get; set; }
            public string Pitch { get; set; }
            public int Octave { get; set; }
            public string Note { get; set; }
            public Enharmonic Enharmonic { get; set; }
            public int Index;
            public _Slot(int locationY, int value, string pitch, int index, Enharmonic enharmonic)
            {
                Location_Y = locationY;
                Value = value;
                Pitch = pitch;
                Octave = int.Parse(value.ToString().ToCharArray()[0].ToString());
                Note = pitch.ToCharArray()[0].ToString();
                Index = index;
                Enharmonic = enharmonic;
            }
        }
    }

    public class Enharmonic
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public List<string> EnharmonicList { get; set; }
        public Enharmonic(string value1, string value2)
        {
            Value1 = value1;
            Value2 = value2;
            EnharmonicList = new List<string> {value1, value2};
        }
    }
}
