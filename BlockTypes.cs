using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private static readonly Dictionary<short, Vector3i> _thinSlab = new Dictionary<short, Vector3i>()
        {
            { 698, Grey },
            { 701, Grey },
            { 704, Grey },
            { 707, Grey },
            { 710, Grey },
            { 713, Grey },
            { 716, Grey },
            { 719, Grey },
            { 722, Grey },
            { 725, Grey },
            { 728, Grey },
            { 731, Grey },
            { 734, Grey },
            { 737, Grey },
            { 740, Grey },
            { 743, Grey },
            { 746, Grey },
            { 749, Grey },
            { 752, Grey },
            { 755, Grey },
            { 758, Grey },
            { 761, Grey },
            { 764, Grey },
            { 767, Grey },
            { 770, Grey },
            { 773, Grey },
            { 776, Grey },
            { 779, Grey },
            { 782, Grey },
            { 785, Grey },
            { 788, Grey },
            { 791, Grey },
            { 794, Grey },
            { 797, Grey },
            { 800, Grey },
            { 803, Grey },
            { 806, Grey },
            { 809, Grey },
            { 812, Grey },
            { 815, Grey },
            {833,DarkGrey},
            {836,DarkGrey},
            {839,DarkGrey},
            {851,DarkGrey},
            {854,DarkGrey},
            {857,DarkGrey},
            {860,DarkGrey},
            {863,DarkGrey}

        };
        private static readonly Dictionary<short, Vector3i> _midSlab = new Dictionary<short, Vector3i>()
        {
            {699,Grey},
            {702,Grey},
            {705,Grey},
            {708,Grey},
            {711,Grey},
            {714,Grey},
            {717,Grey},
            {720,Grey},
            {723,Grey},
            {726,Grey},
            {729,Grey},
            {732,Grey},
            {735,Grey},
            {738,Grey},
            {741,Grey},
            {744,Grey},
            {747,Grey},
            {750,Grey},
            {753,Grey},
            {756,Grey},
            {759,Grey},
            {762,Grey},
            {765,Grey},
            {768,Grey},
            {771,Grey},
            {774,Grey},
            {777,Grey},
            {780,Grey},
            {783,Grey},
            {786,Grey},
            {789,Grey},
            {792,Grey},
            {795,Grey},
            {798,Grey},
            {801,Grey},
            {804,Grey},
            {807,Grey},
            {810,Grey},
            {813,Grey},
            {816,Grey},
            {834,DarkGrey},
            {837,DarkGrey},
            {840,DarkGrey},
            {852,DarkGrey},
            {855,DarkGrey},
            {858,DarkGrey},
            {861,DarkGrey},
            {864,DarkGrey},

        };

        private static readonly Dictionary<short, Vector3i> _thickSlab = new Dictionary<short, Vector3i>()
        {
            {700,Grey},
            {703,Grey},
            {706,Grey},
            {709,Grey},
            {712,Grey},
            {715,Grey},
            {718,Grey},
            {721,Grey},
            {724,Grey},
            {727,Grey},
            {730,Grey},
            {733,Grey},
            {736,Grey},
            {739,Grey},
            {742,Grey},
            {745,Grey},
            {748,Grey},
            {751,Grey},
            {754,Grey},
            {757,Grey},
            {760,Grey},
            {763,Grey},
            {766,Grey},
            {769,Grey},
            {772,Grey},
            {775,Grey},
            {778,Grey},
            {781,Grey},
            {784,Grey},
            {787,Grey},
            {790,Grey},
            {793,Grey},
            {796,Grey},
            {799,Grey},
            {802,Grey},
            {805,Grey},
            {808,Grey},
            {811,Grey},
            {814,Grey},
            {817,Grey},
            {835,DarkGrey},
            {838,DarkGrey},
            {841,DarkGrey},
            {853,DarkGrey},
            {856,DarkGrey},
            {859,DarkGrey},
            {862,DarkGrey}

        };


        private static readonly Dictionary<short, Vector3i> _wc = new Dictionary<short, Vector3i>()
        {
             {599, Grey},
            {293, Grey},
            {311, Grey},

            //BLACK
            {604, Black},
            {296, Black},
            {312, Black},
     

            //WHITE
            {609, White},
            {301, White},
            {319, White},
      

            //PURPLE
            {614, Purple},
            {294, Purple},
            {314, Purple},
         


            //BLUE
            {619, Blue},
            {298, Blue},
            {315, Blue},
   
            // GREEN +HAZARRD
            {624, Green},
            {299, Green},
            {316, Green},
        
            {439, Green},//haz

            //YELLOW +HAZARD
            {629, Yellow},
            {300, Yellow},
            {318, Yellow},
          
            {437, Yellow},

            //ORANGE
            {634, Orange},
            {427, Orange},
            {432, Orange},
         

            //RED
            {639, Red},
            {297, Red},
            {313, Red},
  

            //BROWN
            {644, Brown},
            {295, Brown},
            {317, Brown},
      


            //DARK GREY
            {829, DarkGrey},
            {819, DarkGrey},
            {824, DarkGrey},

            //TEAL
            {879, Teal},
            {869, Teal},
            {874, Teal},
          

            //PINK
            {913, Pink},
            {903, Pink},
            {908, Pink},

            //potential glass
            {594, Black},
            {508, White},
            {538, Purple},
            {533, Blue},
            {528, Green},
            {523, Yellow},
            {518, Orange},
            {513, Red},
            {691, Brown},
            {884, Teal},
            {918, Pink}

        };
        private static readonly Dictionary<short, Vector3i> _bc = new Dictionary<short, Vector3i>()
        {
               //GREYS
            {598, Grey},
            {5, Grey},
            {263, Grey},
            {232, Grey},

            //BLACK
            {603, Black},
            {75, Black},
            {264, Black},
     

            //WHITE
            {608, White},
            {81, White},
            {271, White},
         
            //PURPLE
            {613, Purple},
            {69, Purple},
            {266, Purple},
       


            //BLUE
            {618, Blue},
            {77, Blue},
            {267, Blue},
        

            // GREEN +HAZARRD
            {623, Green},
            {78, Green},
            {268, Green},
            {438, Green},//haz

            //YELLOW +HAZARD
            {628, Yellow},
            {79, Yellow},
            {270, Yellow},
            {436, Yellow},

            //ORANGE
            {633, Orange},
            {426, Orange},
            {431, Orange},
         
            //RED
            {638, Red},
            {76, Red},
            {265, Red},
          

            //BROWN
            {643, Brown},
            {70, Brown},
            {269, Brown},
           
         

            //DARK GREY
            {828, DarkGrey},
            {818, DarkGrey},
            {823, DarkGrey},
            {254, DarkGrey},

            //TEAL
            {878, Teal},
            {868, Teal},
            {873, Teal},
      

            //PINK
            {912, Pink},
            {902, Pink},
            {907, Pink},
            {593, Black},
            {507, White},
            {537, Purple},
            {532, Blue},
            {527, Green},
            {522, Yellow},
            {517, Orange},
            {512, Red},
            {690, Brown},
            {883, Teal},
            {917, Pink}

        };
        private static readonly Dictionary<short, Vector3i> _hc = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {601, Grey},
            {357, Grey},
            {401, Grey},

            //BLACK
            {606, Black},
            {385, Black},
            {369, Black},
            {596, Black},

            //WHITE
            {611, White},
            {392, White},
            {376, White},
            {510, White},

            //PURPLE
            {616, Purple},
            {387, Purple},
            {371, Purple},
            {540, Purple},

            //BLUE
            {621, Blue},
            {388, Blue},
            {372, Blue},
            {535, Blue},

            // GREEN +HAZARRD
            {626, Green},
            {389, Green},
            {373, Green},
            {530, Green},
            {652, Green},//haz

            //YELLOW +HAZARD
            {631, Yellow},
            {391, Yellow},
            {375, Yellow},
            {525, Yellow},
            {649, Yellow},//HAZ

            //ORANGE
            {636, Orange},
            {429, Orange},
            {434, Orange},
            {520, Orange},

            //RED
            {641, Red},
            {386, Red},
            {370, Red},
            {515, Red},

            //BROWN
            {646, Brown},
            {403, Brown},
            {374, Brown},
            {693, Brown},


            //DARK GREY
            {831, DarkGrey},
            {821, DarkGrey},
            {826, DarkGrey},

            //TEAL
            {881, Teal},
            {871, Teal},
            {876, Teal},
            {886, Teal},

            //PINK
            {915, Pink},
            {905, Pink},
            {910, Pink},
            {920, Pink}
        };
        private static readonly Dictionary<short, Vector3i> _tc = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {602, Grey},
            {348, Grey},
            {402, Grey},

            //BLACK
            {607, Black},
            {393, Black},
            {377, Black},
            {597, Black},

            //WHITE
            {612, White},
            {400, White},
            {384, White},
            {511, White},

            //PURPLE
            {617, Purple},
            {395, Purple},
            {379, Purple},
            {541, Purple},

            //BLUE
            {622, Blue},
            {396, Blue},
            {380, Blue},
            {536, Blue},

            // GREEN +HAZARRD
            {627, Green},
            {397, Green},
            {381, Green},
            {531, Green},
            {653, Green},//haz

            //YELLOW +HAZARD
            {632, Yellow},
            {398, Yellow},
            {383, Yellow},
            {526, Yellow},
            {650, Yellow},//HAZ

            //ORANGE
            {637, Orange},
            {430, Orange},
            {435, Orange},
            {521, Orange},

            //RED
            {642, Red},
            {394, Red},
            {378, Red},
            {516, Red},

            //BROWN
            {647, Brown},
            {404, Brown},
            {382, Brown},
            {694, Brown},


            //DARK GREY
            {832, DarkGrey},
            {822, DarkGrey},
            {827, DarkGrey},

            //TEAL
            {882, Teal},
            {872, Teal},
            {877, Teal},
            {887, Teal},

            //PINK
            {916, Pink},
            {906, Pink},
            {911, Pink},
            {921, Pink},
        };
        private static readonly Dictionary<short, Vector3i> _cc = new Dictionary<short, Vector3i>()
        {
             //GREYS
            {600, Grey},
            {302, Grey},
            {320, Grey},

            //BLACK
            {605, Black},
            {305, Black},
            {321, Black},
            {595, Black},

            //WHITE
            {610, White},
            {310, White},
            {328, White},
            {509, White},

            //PURPLE
            {615, Purple},
            {303, Purple},
            {323, Purple},
            {539, Purple},

            //BLUE
            {620, Blue},
            {307, Blue},
            {324, Blue},
            {534, Blue},

            // GREEN +HAZARRD
            {625, Green},
            {308, Green},
            {325, Green},
            {529, Green},
            {651, Green},//haz

            //YELLOW +HAZARD
            {630, Yellow},
            {309, Yellow},
            {327, Yellow},
            {524, Yellow},
            {648, Yellow},//HAZ

            //ORANGE
            {635, Orange},
            {428, Orange},
            {433, Orange},
            {519, Orange},

            //RED
            {640, Red},
            {306, Red},
            {322, Red},
            {514, Red},

            //BROWN
            {645, Brown},
            {304, Brown},
            {326, Brown},
            {692, Brown},


            //DARK GREY
            {830, DarkGrey},
            {820, DarkGrey},
            {825, DarkGrey},

            //TEAL
            {880, Teal},
            {870, Teal},
            {875, Teal},
            {885, Teal},

            //PINK
            {914, Pink},
            {904, Pink},
            {909, Pink},
            {919, Pink},
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
        public static short GetDefault(short type)
        {
            if (IsHull(type))
            {
                return 5;
            }
            if (IsWedge(type))
            {
                return 293;
            }
            if (IsCorner(type))
            {
                return 302;
            }
            if (IsHepta(type))
            {
                return 357;
            }
            if (IsTetra(type))
            {
                return 348;
            }
            if (IsThickSlab(type))
            {
                return 703;
            }
            if (IsMidSlab(type))
            {
                return 702;
            }
            if (IsThinSlab(type))
            {
                return 701;
            }

            return 0;
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
    }
}
