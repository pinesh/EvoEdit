using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using EvoEditApp;

namespace testapp1
{
    public static class BlockTypes
    {
        public static short CoreId = 1;

        public static short GlassWedgeId = 329;
        public static short GlassCornerId = 330;

        private static Dictionary<short, Vector3i> _thinSlab = new Dictionary<short, Vector3i>()
        {
            { 698, SmColor.Grey },
            { 701, SmColor.Grey },
            { 704, SmColor.Grey },
            { 707, SmColor.Grey },
            { 710, SmColor.Grey },
            { 713, SmColor.Grey },
            { 716, SmColor.Grey },
            { 719, SmColor.Grey },
            { 722, SmColor.Grey },
            { 725, SmColor.Grey },
            { 728, SmColor.Grey },
            { 731, SmColor.Grey },
            { 734, SmColor.Grey },
            { 737, SmColor.Grey },
            { 740, SmColor.Grey },
            { 743, SmColor.Grey },
            { 746, SmColor.Grey },
            { 749, SmColor.Grey },
            { 752, SmColor.Grey },
            { 755, SmColor.Grey },
            { 758, SmColor.Grey },
            { 761, SmColor.Grey },
            { 764, SmColor.Grey },
            { 767, SmColor.Grey },
            { 770, SmColor.Grey },
            { 773, SmColor.Grey },
            { 776, SmColor.Grey },
            { 779, SmColor.Grey },
            { 782, SmColor.Grey },
            { 785, SmColor.Grey },
            { 788, SmColor.Grey },
            { 791, SmColor.Grey },
            { 794, SmColor.Grey },
            { 797, SmColor.Grey },
            { 800, SmColor.Grey },
            { 803, SmColor.Grey },
            { 806, SmColor.Grey },
            { 809, SmColor.Grey },
            { 812, SmColor.Grey },
            { 815, SmColor.Grey },
            {833,SmColor.DarkGrey},
            {836,SmColor.DarkGrey},
            {839,SmColor.DarkGrey},
            {851,SmColor.DarkGrey},
            {854,SmColor.DarkGrey},
            {857,SmColor.DarkGrey},
            {860,SmColor.DarkGrey},
            {863,SmColor.DarkGrey}

        };
        private static Dictionary<short, Vector3i> _midSlab = new Dictionary<short, Vector3i>()
        {
            {699,SmColor.Grey},
            {702,SmColor.Grey},
            {705,SmColor.Grey},
            {708,SmColor.Grey},
            {711,SmColor.Grey},
            {714,SmColor.Grey},
            {717,SmColor.Grey},
            {720,SmColor.Grey},
            {723,SmColor.Grey},
            {726,SmColor.Grey},
            {729,SmColor.Grey},
            {732,SmColor.Grey},
            {735,SmColor.Grey},
            {738,SmColor.Grey},
            {741,SmColor.Grey},
            {744,SmColor.Grey},
            {747,SmColor.Grey},
            {750,SmColor.Grey},
            {753,SmColor.Grey},
            {756,SmColor.Grey},
            {759,SmColor.Grey},
            {762,SmColor.Grey},
            {765,SmColor.Grey},
            {768,SmColor.Grey},
            {771,SmColor.Grey},
            {774,SmColor.Grey},
            {777,SmColor.Grey},
            {780,SmColor.Grey},
            {783,SmColor.Grey},
            {786,SmColor.Grey},
            {789,SmColor.Grey},
            {792,SmColor.Grey},
            {795,SmColor.Grey},
            {798,SmColor.Grey},
            {801,SmColor.Grey},
            {804,SmColor.Grey},
            {807,SmColor.Grey},
            {810,SmColor.Grey},
            {813,SmColor.Grey},
            {816,SmColor.Grey},
            {834,SmColor.DarkGrey},
            {837,SmColor.DarkGrey},
            {840,SmColor.DarkGrey},
            {852,SmColor.DarkGrey},
            {855,SmColor.DarkGrey},
            {858,SmColor.DarkGrey},
            {861,SmColor.DarkGrey},
            {864,SmColor.DarkGrey},

        };

        private static Dictionary<short, Vector3i> _thickSlab = new Dictionary<short, Vector3i>()
        {
            {700,SmColor.Grey},
            {703,SmColor.Grey},
            {706,SmColor.Grey},
            {709,SmColor.Grey},
            {712,SmColor.Grey},
            {715,SmColor.Grey},
            {718,SmColor.Grey},
            {721,SmColor.Grey},
            {724,SmColor.Grey},
            {727,SmColor.Grey},
            {730,SmColor.Grey},
            {733,SmColor.Grey},
            {736,SmColor.Grey},
            {739,SmColor.Grey},
            {742,SmColor.Grey},
            {745,SmColor.Grey},
            {748,SmColor.Grey},
            {751,SmColor.Grey},
            {754,SmColor.Grey},
            {757,SmColor.Grey},
            {760,SmColor.Grey},
            {763,SmColor.Grey},
            {766,SmColor.Grey},
            {769,SmColor.Grey},
            {772,SmColor.Grey},
            {775,SmColor.Grey},
            {778,SmColor.Grey},
            {781,SmColor.Grey},
            {784,SmColor.Grey},
            {787,SmColor.Grey},
            {790,SmColor.Grey},
            {793,SmColor.Grey},
            {796,SmColor.Grey},
            {799,SmColor.Grey},
            {802,SmColor.Grey},
            {805,SmColor.Grey},
            {808,SmColor.Grey},
            {811,SmColor.Grey},
            {814,SmColor.Grey},
            {817,SmColor.Grey},
            {835,SmColor.DarkGrey},
            {838,SmColor.DarkGrey},
            {841,SmColor.DarkGrey},
            {853,SmColor.DarkGrey},
            {856,SmColor.DarkGrey},
            {859,SmColor.DarkGrey},
            {862,SmColor.DarkGrey}

        };


        private static Dictionary<short, Vector3i> _wc = new Dictionary<short, Vector3i>()
        {
             {599, SmColor.Grey},
            {293, SmColor.Grey},
            {311, SmColor.Grey},

            //BLACK
            {604, SmColor.Black},
            {296, SmColor.Black},
            {312, SmColor.Black},
     

            //WHITE
            {609, SmColor.White},
            {301, SmColor.White},
            {319, SmColor.White},
      

            //PURPLE
            {614, SmColor.Purple},
            {294, SmColor.Purple},
            {314, SmColor.Purple},
         


            //BLUE
            {619, SmColor.Blue},
            {298, SmColor.Blue},
            {315, SmColor.Blue},
   
            // GREEN +HAZARRD
            {624, SmColor.Green},
            {299, SmColor.Green},
            {316, SmColor.Green},
        
            {439, SmColor.Green},//haz

            //YELLOW +HAZARD
            {629, SmColor.Yellow},
            {300, SmColor.Yellow},
            {318, SmColor.Yellow},
          
            {437, SmColor.Yellow},

            //ORANGE
            {634, SmColor.Orange},
            {427, SmColor.Orange},
            {432, SmColor.Orange},
         

            //RED
            {639, SmColor.Red},
            {297, SmColor.Red},
            {313, SmColor.Red},
  

            //BROWN
            {644, SmColor.Brown},
            {295, SmColor.Brown},
            {317, SmColor.Brown},
      


            //DARK GREY
            {829, SmColor.DarkGrey},
            {819, SmColor.DarkGrey},
            {824, SmColor.DarkGrey},

            //TEAL
            {879, SmColor.Teal},
            {869, SmColor.Teal},
            {874, SmColor.Teal},
          

            //PINK
            {913, SmColor.Pink},
            {903, SmColor.Pink},
            {908, SmColor.Pink},

            //potential glass
            {594, SmColor.Black},
            {508, SmColor.White},
            {538, SmColor.Purple},
            {533, SmColor.Blue},
            {528, SmColor.Green},
            {523, SmColor.Yellow},
            {518, SmColor.Orange},
            {513, SmColor.Red},
            {691, SmColor.Brown},
            {884, SmColor.Teal},
            {918, SmColor.Pink}

        };
        private static Dictionary<short, Vector3i> _bc = new Dictionary<short, Vector3i>()
        {
               //GREYS
            {598, SmColor.Grey},
            {5, SmColor.Grey},
            {263, SmColor.Grey},
            {232, SmColor.Grey},

            //BLACK
            {603, SmColor.Black},
            {75, SmColor.Black},
            {264, SmColor.Black},
     

            //WHITE
            {608, SmColor.White},
            {81, SmColor.White},
            {271, SmColor.White},
         
            //PURPLE
            {613, SmColor.Purple},
            {69, SmColor.Purple},
            {266, SmColor.Purple},
       


            //BLUE
            {618, SmColor.Blue},
            {77, SmColor.Blue},
            {267, SmColor.Blue},
        

            // GREEN +HAZARRD
            {623, SmColor.Green},
            {78, SmColor.Green},
            {268, SmColor.Green},
            {438, SmColor.Green},//haz

            //YELLOW +HAZARD
            {628, SmColor.Yellow},
            {79, SmColor.Yellow},
            {270, SmColor.Yellow},
            {436, SmColor.Yellow},

            //ORANGE
            {633, SmColor.Orange},
            {426, SmColor.Orange},
            {431, SmColor.Orange},
         
            //RED
            {638, SmColor.Red},
            {76, SmColor.Red},
            {265, SmColor.Red},
          

            //BROWN
            {643, SmColor.Brown},
            {70, SmColor.Brown},
            {269, SmColor.Brown},
           
         

            //DARK GREY
            {828, SmColor.DarkGrey},
            {818, SmColor.DarkGrey},
            {823, SmColor.DarkGrey},
            {254, SmColor.DarkGrey},

            //TEAL
            {878, SmColor.Teal},
            {868, SmColor.Teal},
            {873, SmColor.Teal},
      

            //PINK
            {912, SmColor.Pink},
            {902, SmColor.Pink},
            {907, SmColor.Pink},
            {593, SmColor.Black},
            {507, SmColor.White},
            {537, SmColor.Purple},
            {532, SmColor.Blue},
            {527, SmColor.Green},
            {522, SmColor.Yellow},
            {517, SmColor.Orange},
            {512, SmColor.Red},
            {690, SmColor.Brown},
            {883, SmColor.Teal},
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
        /*
        private static Dictionary<short, Vector3i> _glassBlocks = new Dictionary<short,Vector3i>()
        {
            {593, SmColor.Black},
            {507, SmColor.White},
            {537, SmColor.Purple},
            {532, SmColor.Blue},
            {527, SmColor.Green},
            {522, SmColor.Yellow},
            {517, SmColor.Orange},
            {512, SmColor.Red},
            {690, SmColor.Brown},
            {883, SmColor.Teal},
            {917, SmColor.Pink},
        };
        private static Dictionary<short, Vector3i> _glassWedges = new Dictionary<short, Vector3i>()
        {
            {594, SmColor.Black},
            {508, SmColor.White},
            {538, SmColor.Purple},
            {533, SmColor.Blue},
            {528, SmColor.Green},
            {523, SmColor.Yellow},
            {518, SmColor.Orange},
            {513, SmColor.Red},
            {691, SmColor.Brown},
            {884, SmColor.Teal},
            {918, SmColor.Pink},
        };

        public static bool IsGlassBlock(short blockId)
        {
            return _glassBlocks.ContainsKey(blockId);
        }

        public static bool IsGlassWedge(short blockId)
        {
            return _glassWedges.ContainsKey(blockId);
        }
        */

        public static bool IsThinSlab(short blockId)
        {
            return _thinSlab.ContainsKey(blockId);
        }
        public static bool IsThickSlab(short blockId)
        {
            return _thickSlab.ContainsKey(blockId);
        }
        public static bool IsMidSlab(short blockId)
        {
            return _midSlab.ContainsKey(blockId);
        }


        public static bool IsSlab(short blockId)
        {
            return IsMidSlab(blockId) || IsThickSlab(blockId) || IsThinSlab(blockId);
        }
        public static bool IsHull(short blockId)
        {
            return _bc.ContainsKey(blockId);
        }

        public static bool IsWedge(short blockId)
        {
            //|| _glassWedges.ContainsKey(blockId);
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
            //glass|| IsGlass(blockId)
            return IsHull(blockId) || IsCorner(blockId) || IsWedge(blockId) || IsHepta(blockId) || IsTetra(blockId) || IsSlab(blockId) ;
        }
        /*
        public static bool IsGlass(short blockId)
        {
            return IsGlassWedge(blockId) || IsGlassBlock(blockId);
        }*/

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
            if (IsMidSlab(blockId))
                return 243;
            if (IsThinSlab(blockId))
                return 244;
            if (IsThickSlab(blockId))//temp
                return 245;
            return 0;
            /*
            if (IsGlassBlock(blockId))
                return 166;
            if (IsGlassWedge(blockId))
             return 168;
            */
        }

        public static Vector3i Sevo_Paint(short blockId)
        {
            return IsHull(blockId) ? _bc[blockId] :
                IsCorner(blockId) ? _cc[blockId] :
                IsWedge(blockId) ? _wc[blockId] :
                IsHepta(blockId) ? _hc[blockId] :
                IsTetra(blockId) ? _tc[blockId] :
                IsThickSlab(blockId) ? _thickSlab[blockId] :
                IsThinSlab(blockId) ? _thinSlab[blockId] :
                IsMidSlab(blockId) ? _midSlab[blockId] : new Vector3i(0, 0, 0);
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
