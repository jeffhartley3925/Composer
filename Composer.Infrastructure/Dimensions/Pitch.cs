using System.Collections.Generic;

namespace Composer.Infrastructure
{
    public static class Pitch
    {
        public static IDictionary<int, int> YCoordinatePitchNormalizationMap = new Dictionary<int, int>();

        static Pitch()
        {
            InitNormalizationMap();
        }

        private static void InitNormalizationMap()
        {
            YCoordinatePitchNormalizationMap.Add(7, 9);
            YCoordinatePitchNormalizationMap.Add(8, 9);
            YCoordinatePitchNormalizationMap.Add(9, 9);
            YCoordinatePitchNormalizationMap.Add(10, 9);

            YCoordinatePitchNormalizationMap.Add(11, 13);
            YCoordinatePitchNormalizationMap.Add(12, 13);
            YCoordinatePitchNormalizationMap.Add(13, 13);
            YCoordinatePitchNormalizationMap.Add(14, 13);

            YCoordinatePitchNormalizationMap.Add(15, 17);
            YCoordinatePitchNormalizationMap.Add(16, 17);
            YCoordinatePitchNormalizationMap.Add(17, 17);
            YCoordinatePitchNormalizationMap.Add(18, 17);

            YCoordinatePitchNormalizationMap.Add(19, 21);
            YCoordinatePitchNormalizationMap.Add(20, 21);
            YCoordinatePitchNormalizationMap.Add(21, 21);
            YCoordinatePitchNormalizationMap.Add(22, 21);

            YCoordinatePitchNormalizationMap.Add(23, 25);
            YCoordinatePitchNormalizationMap.Add(24, 25);
            YCoordinatePitchNormalizationMap.Add(25, 25);
            YCoordinatePitchNormalizationMap.Add(26, 25);

            YCoordinatePitchNormalizationMap.Add(27, 29);
            YCoordinatePitchNormalizationMap.Add(28, 29);
            YCoordinatePitchNormalizationMap.Add(29, 29);
            YCoordinatePitchNormalizationMap.Add(30, 29);

            YCoordinatePitchNormalizationMap.Add(31, 33);
            YCoordinatePitchNormalizationMap.Add(32, 33);
            YCoordinatePitchNormalizationMap.Add(33, 33);
            YCoordinatePitchNormalizationMap.Add(34, 33);

            YCoordinatePitchNormalizationMap.Add(35, 37);
            YCoordinatePitchNormalizationMap.Add(36, 37);
            YCoordinatePitchNormalizationMap.Add(37, 37);
            YCoordinatePitchNormalizationMap.Add(38, 37);

            YCoordinatePitchNormalizationMap.Add(39, 41);
            YCoordinatePitchNormalizationMap.Add(40, 41);
            YCoordinatePitchNormalizationMap.Add(41, 41);
            YCoordinatePitchNormalizationMap.Add(42, 41);

            YCoordinatePitchNormalizationMap.Add(43, 45);
            YCoordinatePitchNormalizationMap.Add(44, 45);
            YCoordinatePitchNormalizationMap.Add(45, 45);
            YCoordinatePitchNormalizationMap.Add(46, 45);

            YCoordinatePitchNormalizationMap.Add(47, 49);
            YCoordinatePitchNormalizationMap.Add(48, 49);
            YCoordinatePitchNormalizationMap.Add(49, 49);
            YCoordinatePitchNormalizationMap.Add(50, 49);

            YCoordinatePitchNormalizationMap.Add(51, 53);
            YCoordinatePitchNormalizationMap.Add(52, 53);
            YCoordinatePitchNormalizationMap.Add(53, 53);
            YCoordinatePitchNormalizationMap.Add(54, 53);

            YCoordinatePitchNormalizationMap.Add(55, 57);
            YCoordinatePitchNormalizationMap.Add(56, 57);
            YCoordinatePitchNormalizationMap.Add(57, 57);
            YCoordinatePitchNormalizationMap.Add(58, 57);

            YCoordinatePitchNormalizationMap.Add(59, 61);
            YCoordinatePitchNormalizationMap.Add(60, 61);
            YCoordinatePitchNormalizationMap.Add(61, 61);
            YCoordinatePitchNormalizationMap.Add(62, 61);

            YCoordinatePitchNormalizationMap.Add(63, 65);
            YCoordinatePitchNormalizationMap.Add(64, 65);
            YCoordinatePitchNormalizationMap.Add(65, 65);
            YCoordinatePitchNormalizationMap.Add(66, 65);

            YCoordinatePitchNormalizationMap.Add(67, 69);
            YCoordinatePitchNormalizationMap.Add(68, 69);
            YCoordinatePitchNormalizationMap.Add(69, 69);
            YCoordinatePitchNormalizationMap.Add(70, 69);

            YCoordinatePitchNormalizationMap.Add(71, 73);
            YCoordinatePitchNormalizationMap.Add(72, 73);
            YCoordinatePitchNormalizationMap.Add(73, 73);
            YCoordinatePitchNormalizationMap.Add(74, 73);

            YCoordinatePitchNormalizationMap.Add(75, 77);
            YCoordinatePitchNormalizationMap.Add(76, 77);
            YCoordinatePitchNormalizationMap.Add(77, 77);
            YCoordinatePitchNormalizationMap.Add(78, 77);

            YCoordinatePitchNormalizationMap.Add(79, 81);
            YCoordinatePitchNormalizationMap.Add(80, 81);
            YCoordinatePitchNormalizationMap.Add(81, 81);
            YCoordinatePitchNormalizationMap.Add(82, 81);

            YCoordinatePitchNormalizationMap.Add(83, 85);
            YCoordinatePitchNormalizationMap.Add(84, 85);
            YCoordinatePitchNormalizationMap.Add(85, 85);
            YCoordinatePitchNormalizationMap.Add(86, 85);

            YCoordinatePitchNormalizationMap.Add(87, 89);
            YCoordinatePitchNormalizationMap.Add(88, 89);
            YCoordinatePitchNormalizationMap.Add(89, 89);
            YCoordinatePitchNormalizationMap.Add(90, 89);

            YCoordinatePitchNormalizationMap.Add(91, 93);
            YCoordinatePitchNormalizationMap.Add(92, 93);
            YCoordinatePitchNormalizationMap.Add(93, 93);
            YCoordinatePitchNormalizationMap.Add(94, 93);

            YCoordinatePitchNormalizationMap.Add(95, 97);
            YCoordinatePitchNormalizationMap.Add(96, 97);
            YCoordinatePitchNormalizationMap.Add(97, 97);
            YCoordinatePitchNormalizationMap.Add(98, 97);

            YCoordinatePitchNormalizationMap.Add(99, 101);
            YCoordinatePitchNormalizationMap.Add(100, 101);
            YCoordinatePitchNormalizationMap.Add(101, 101);
            YCoordinatePitchNormalizationMap.Add(102, 101);

            YCoordinatePitchNormalizationMap.Add(103, 105);
            YCoordinatePitchNormalizationMap.Add(104, 105);
            YCoordinatePitchNormalizationMap.Add(105, 105);
            YCoordinatePitchNormalizationMap.Add(106, 105);

            YCoordinatePitchNormalizationMap.Add(107, 109);
            YCoordinatePitchNormalizationMap.Add(108, 109);
            YCoordinatePitchNormalizationMap.Add(109, 109);
            YCoordinatePitchNormalizationMap.Add(110, 109);

            YCoordinatePitchNormalizationMap.Add(111, 113);
            YCoordinatePitchNormalizationMap.Add(112, 113);
            YCoordinatePitchNormalizationMap.Add(113, 113);
            YCoordinatePitchNormalizationMap.Add(114, 113);

            YCoordinatePitchNormalizationMap.Add(115, 117);
            YCoordinatePitchNormalizationMap.Add(116, 117);
            YCoordinatePitchNormalizationMap.Add(117, 117);
            YCoordinatePitchNormalizationMap.Add(118, 117);
        }
    }
}
