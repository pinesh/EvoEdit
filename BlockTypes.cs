using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoEditApp;

namespace testapp1
{
    public static class BlockTypes
    {
        public static short CoreId = 1;

        public static short GlassWedgeId = 329;
        public static short GlassCornerId = 330;

        private static Dictionary<short, Vector3i> _wc = new Dictionary<short, Vector3i>()
        {
             {599, SmColor.Grey},
            {293, SmColor.Grey},
            {311, SmColor.Grey},

            //BLACK
            {604, SmColor.Black},
            {296, SmColor.Black},
            {312, SmColor.Black},
            {594, SmColor.Black},

            //WHITE
            {609, SmColor.White},
            {301, SmColor.White},
            {319, SmColor.White},
            {508, SmColor.White},

            //PURPLE
            {614, SmColor.Purple},
            {294, SmColor.Purple},
            {314, SmColor.Purple},
            {538, SmColor.Purple},

            //BLUE
            {619, SmColor.Blue},
            {298, SmColor.Blue},
            {315, SmColor.Blue},
            {533, SmColor.Blue},

            // GREEN +HAZARRD
            {624, SmColor.Green},
            {299, SmColor.Green},
            {316, SmColor.Green},
            {528, SmColor.Green},
            {439, SmColor.Green},//haz

            //YELLOW +HAZARD
            {629, SmColor.Yellow},
            {300, SmColor.Yellow},
            {318, SmColor.Yellow},
            {523, SmColor.Yellow},
            {437, SmColor.Yellow},

            //ORANGE
            {634, SmColor.Orange},
            {427, SmColor.Orange},
            {432, SmColor.Orange},
            {518, SmColor.Orange},

            //RED
            {639, SmColor.Red},
            {297, SmColor.Red},
            {313, SmColor.Red},
            {513, SmColor.Red},

            //BROWN
            {644, SmColor.Brown},
            {295, SmColor.Brown},
            {317, SmColor.Brown},
            {691, SmColor.Brown},


            //DARK GREY
            {829, SmColor.DarkGrey},
            {819, SmColor.DarkGrey},
            {824, SmColor.DarkGrey},

            //TEAL
            {879, SmColor.Teal},
            {869, SmColor.Teal},
            {874, SmColor.Teal},
            {884, SmColor.Teal},

            //PINK
            {913, SmColor.Pink},
            {903, SmColor.Pink},
            {908, SmColor.Pink},
            {918, SmColor.Pink}
        };
        private static Dictionary<short, Vector3i> _bc = new Dictionary<short, Vector3i>()
        {
               //GREYS
            {598, SmColor.Grey},
            {5, SmColor.Grey},
            {263, SmColor.Grey},

            //BLACK
            {603, SmColor.Black},
            {75, SmColor.Black},
            {264, SmColor.Black},
            {593, SmColor.Black},

            //WHITE
            {608, SmColor.White},
            {81, SmColor.White},
            {271, SmColor.White},
            {507, SmColor.White},

            //PURPLE
            {613, SmColor.Purple},
            {69, SmColor.Purple},
            {266, SmColor.Purple},
            {537, SmColor.Purple},

            //BLUE
            {618, SmColor.Blue},
            {77, SmColor.Blue},
            {267, SmColor.Blue},
            {532, SmColor.Blue},

            // GREEN +HAZARRD
            {623, SmColor.Green},
            {78, SmColor.Green},
            {268, SmColor.Green},
            {527, SmColor.Green},
            {438, SmColor.Green},//haz

            //YELLOW +HAZARD
            {628, SmColor.Yellow},
            {79, SmColor.Yellow},
            {270, SmColor.Yellow},
            {522, SmColor.Yellow},
            {436, SmColor.Yellow},

            //ORANGE
            {633, SmColor.Orange},
            {426, SmColor.Orange},
            {431, SmColor.Orange},
            {517, SmColor.Orange},

            //RED
            {638, SmColor.Red},
            {76, SmColor.Red},
            {265, SmColor.Red},
            {512, SmColor.Red},

            //BROWN
            {643, SmColor.Brown},
            {70, SmColor.Brown},
            {269, SmColor.Brown},
            {690, SmColor.Brown},
         

            //DARK GREY
            {828, SmColor.DarkGrey},
            {818, SmColor.DarkGrey},
            {823, SmColor.DarkGrey},

            //TEAL
            {878, SmColor.Teal},
            {868, SmColor.Teal},
            {873, SmColor.Teal},
            {883, SmColor.Teal},

            //PINK
            {912, SmColor.Pink},
            {902, SmColor.Pink},
            {907, SmColor.Pink},
            {917, SmColor.Pink}
        };
        private static Dictionary<short, Vector3i> _hc = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {601, SmColor.Grey},
            {357, SmColor.Grey},
            {401, SmColor.Grey},

            //BLACK
            {606, SmColor.Black},
            {385, SmColor.Black},
            {369, SmColor.Black},
            {596, SmColor.Black},

            //WHITE
            {611, SmColor.White},
            {392, SmColor.White},
            {376, SmColor.White},
            {510, SmColor.White},

            //PURPLE
            {616, SmColor.Purple},
            {387, SmColor.Purple},
            {371, SmColor.Purple},
            {540, SmColor.Purple},

            //BLUE
            {621, SmColor.Blue},
            {388, SmColor.Blue},
            {372, SmColor.Blue},
            {535, SmColor.Blue},

            // GREEN +HAZARRD
            {626, SmColor.Green},
            {389, SmColor.Green},
            {373, SmColor.Green},
            {530, SmColor.Green},
            {652, SmColor.Green},//haz

            //YELLOW +HAZARD
            {631, SmColor.Yellow},
            {391, SmColor.Yellow},
            {375, SmColor.Yellow},
            {525, SmColor.Yellow},
            {649, SmColor.Yellow},//HAZ

            //ORANGE
            {636, SmColor.Orange},
            {429, SmColor.Orange},
            {434, SmColor.Orange},
            {520, SmColor.Orange},

            //RED
            {641, SmColor.Red},
            {386, SmColor.Red},
            {370, SmColor.Red},
            {515, SmColor.Red},

            //BROWN
            {646, SmColor.Brown},
            {403, SmColor.Brown},
            {374, SmColor.Brown},
            {693, SmColor.Brown},


            //DARK GREY
            {831, SmColor.DarkGrey},
            {821, SmColor.DarkGrey},
            {826, SmColor.DarkGrey},

            //TEAL
            {881, SmColor.Teal},
            {871, SmColor.Teal},
            {876, SmColor.Teal},
            {886, SmColor.Teal},

            //PINK
            {915, SmColor.Pink},
            {905, SmColor.Pink},
            {910, SmColor.Pink},
            {920, SmColor.Pink}
        };
        private static Dictionary<short, Vector3i> _tc = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {602, SmColor.Grey},
            {348, SmColor.Grey},
            {402, SmColor.Grey},

            //BLACK
            {607, SmColor.Black},
            {393, SmColor.Black},
            {377, SmColor.Black},
            {597, SmColor.Black},

            //WHITE
            {612, SmColor.White},
            {400, SmColor.White},
            {384, SmColor.White},
            {511, SmColor.White},

            //PURPLE
            {617, SmColor.Purple},
            {395, SmColor.Purple},
            {379, SmColor.Purple},
            {541, SmColor.Purple},

            //BLUE
            {622, SmColor.Blue},
            {396, SmColor.Blue},
            {380, SmColor.Blue},
            {536, SmColor.Blue},

            // GREEN +HAZARRD
            {627, SmColor.Green},
            {397, SmColor.Green},
            {381, SmColor.Green},
            {531, SmColor.Green},
            {653, SmColor.Green},//haz

            //YELLOW +HAZARD
            {632, SmColor.Yellow},
            {398, SmColor.Yellow},
            {383, SmColor.Yellow},
            {526, SmColor.Yellow},
            {650, SmColor.Yellow},//HAZ

            //ORANGE
            {637, SmColor.Orange},
            {430, SmColor.Orange},
            {435, SmColor.Orange},
            {521, SmColor.Orange},

            //RED
            {642, SmColor.Red},
            {394, SmColor.Red},
            {378, SmColor.Red},
            {516, SmColor.Red},

            //BROWN
            {647, SmColor.Brown},
            {404, SmColor.Brown},
            {382, SmColor.Brown},
            {694, SmColor.Brown},


            //DARK GREY
            {832, SmColor.DarkGrey},
            {822, SmColor.DarkGrey},
            {827, SmColor.DarkGrey},

            //TEAL
            {882, SmColor.Teal},
            {872, SmColor.Teal},
            {877, SmColor.Teal},
            {887, SmColor.Teal},

            //PINK
            {916, SmColor.Pink},
            {906, SmColor.Pink},
            {911, SmColor.Pink},
            {921, SmColor.Pink},
        };
        private static Dictionary<short, Vector3i> _cc = new Dictionary<short, Vector3i>()
        {
             //GREYS
            {600, SmColor.Grey},
            {302, SmColor.Grey},
            {320, SmColor.Grey},

            //BLACK
            {605, SmColor.Black},
            {305, SmColor.Black},
            {321, SmColor.Black},
            {595, SmColor.Black},

            //WHITE
            {610, SmColor.White},
            {310, SmColor.White},
            {328, SmColor.White},
            {509, SmColor.White},

            //PURPLE
            {615, SmColor.Purple},
            {303, SmColor.Purple},
            {323, SmColor.Purple},
            {539, SmColor.Purple},

            //BLUE
            {620, SmColor.Blue},
            {307, SmColor.Blue},
            {324, SmColor.Blue},
            {534, SmColor.Blue},

            // GREEN +HAZARRD
            {625, SmColor.Green},
            {308, SmColor.Green},
            {325, SmColor.Green},
            {529, SmColor.Green},
            {651, SmColor.Green},//haz

            //YELLOW +HAZARD
            {630, SmColor.Yellow},
            {309, SmColor.Yellow},
            {327, SmColor.Yellow},
            {524, SmColor.Yellow},
            {648, SmColor.Yellow},//HAZ

            //ORANGE
            {635, SmColor.Orange},
            {428, SmColor.Orange},
            {433, SmColor.Orange},
            {519, SmColor.Orange},

            //RED
            {640, SmColor.Red},
            {306, SmColor.Red},
            {322, SmColor.Red},
            {514, SmColor.Red},

            //BROWN
            {645, SmColor.Brown},
            {304, SmColor.Brown},
            {326, SmColor.Brown},
            {692, SmColor.Brown},


            //DARK GREY
            {830, SmColor.DarkGrey},
            {820, SmColor.DarkGrey},
            {825, SmColor.DarkGrey},

            //TEAL
            {880, SmColor.Teal},
            {870, SmColor.Teal},
            {875, SmColor.Teal},
            {885, SmColor.Teal},

            //PINK
            {914, SmColor.Pink},
            {904, SmColor.Pink},
            {909, SmColor.Pink},
            {919, SmColor.Pink},
        };

        public static bool IsHull(short blockId)
        {
            return _bc.ContainsKey(blockId);
        }

        public static bool IsWedge(short blockId)
        {
            return _wc.ContainsKey(blockId);
        }

        public static bool IsCorner(short blockId)
        {
            return _cc.ContainsKey(blockId);
        }

        public static bool IsHepta(short blockId)
        {
            return _hc.ContainsKey(blockId);
        }

        public static bool IsTetra(short blockId)
        {
            return _tc.ContainsKey(blockId);
        }

        public static bool IsAnyHull(short blockId)
        {
            return IsHull(blockId) || IsCorner(blockId) || IsWedge(blockId) || IsHepta(blockId) || IsTetra(blockId);
        }

        public static int Sevo_ID(short blockId)
        {
            if (IsHull(blockId))
                return 196;
            if (IsCorner(blockId))
                return 199;
            if (IsWedge(blockId))
                return 197;
            if (IsHepta(blockId))
                return 200;
            if (IsTetra(blockId))
                return 198;
            return 0;
        }

        public static Vector3i Sevo_Paint(short blockId)
        {
            return IsHull(blockId) ? _bc[blockId] :
                IsCorner(blockId) ? _cc[blockId] :
                IsWedge(blockId) ? _wc[blockId] :
                IsHepta(blockId) ? _hc[blockId] :
                IsTetra(blockId) ? _tc[blockId] : new Vector3i(0, 0, 0);
        }
        public struct SmColor
        {
            public static Vector3i Grey = new Vector3i(100, 103, 105);
            public static Vector3i Black = new Vector3i(10, 10, 12);
            public static Vector3i White = new Vector3i(220, 220, 220);
            public static Vector3i Purple = new Vector3i(148, 10, 196);
            public static Vector3i Blue = new Vector3i(10, 84, 196);
            public static Vector3i Green = new Vector3i(69, 177, 42);
            public static Vector3i Yellow = new Vector3i(196, 172, 10);
            public static Vector3i Orange = new Vector3i(196, 68, 10);
            public static Vector3i Red = new Vector3i(196, 10, 10);
            public static Vector3i Brown = new Vector3i(142, 75, 49);
            public static Vector3i DarkGrey = new Vector3i(80, 82, 84);
            public static Vector3i Teal = new Vector3i(10, 196, 140);
            public static Vector3i Pink = new Vector3i(196, 10, 150);
        }
    }
}
