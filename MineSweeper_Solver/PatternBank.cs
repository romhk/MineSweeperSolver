using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{

    public struct patternTest
    {
        public string Name;
        public matrix GameField;
        public matrix Pattern;
        public bool Result;
    }

    public class PatternBank
    {
        // это нужно сгенерировать и сохранить в файл
        public static string[][] bankBasic = new string[][] {
            new string[] {
                "   ",
                " 1 ",
                "  M",
            },

            new string[] {
                "   ",
                " 2 ",
                " MM",
            },

            new string[] {
                "   ",
                " 2 ",
                "M M",
            },

            new string[] {
                "M  ",
                " 2 ",
                "M  ",
            },

            new string[] {
                " M ",
                " 2 ",
                "M  ",
            },

            new string[] {
                "  M",
                " 2 ",
                "M  ",
            },

            new string[] {
                " M ",
                " 2 ",
                " M ",
            },

            new string[] {
                "   ",
                " 2M",
                " M ",
            },
            new string[] {
                "   ",
                " 3 ",
                "MMM",
            },

            new string[] {
                "   ",
                " 3M",
                " MM",
            },

            new string[] {
                "   ",
                " 4M",
                "MMM",
            },

            new string[] {
                "M  ",
                " 4 ",
                "MMM",
            },

            new string[] {
                " M ",
                " 4 ",
                "MMM",
            },

            new string[] {
                "  M",
                " 5M",
                "MMM",
            },

            new string[] {
                " MM",
                " 5 ",
                "MMM",
            },

            new string[] {
                "M M",
                " 5 ",
                "MMM",
            },

            new string[] {
                "M M",
                " 5M",
                "MM ",
            },

            new string[] {
                "MM ",
                " 5M",
                "MM ",
            },

            new string[] {
                " MM",
                " 6M",
                "MMM",
            },

            new string[] {
                "MMM",
                " 7M",
                "MMM",
            },

        };

        public static string[][] bankNormal = new string[][] {
            new string[] {
                "   ",
                "121",
                "M^M",
            },

            new string[] {
                "    ",
                "1221",
                "^MM^",
            },


            new string[] {
                "     ",
                " 112 ",
                " #M#M",
            },

            new string[] {
                "    ",
                " 12 ",
                " ##M",
            },

            new string[] {
                "    ",
                " 11 ",
                " ##^",
            },

            new string[] {
                "    ",
                "W21 ",
                " ##^",
            },

            new string[] {
                "    ",
                "W31 ",
                "W##^",
            },

            new string[] {
                "W   ",
                "W31 ",
                " ##^",
            },

            new string[] {
                "W   ",
                "W32 ",
                " ##M",
            },

            new string[] {
                "    ",
                "W22 ",
                " ##M",
            },

            new string[] {
                "    ",
                "W32 ",
                "W##M",
            },

            new string[] {
                " W^",
                " 2#",
                " 1#",
                "   ",
            },

            new string[] {
                "W ^",
                " 2#",
                " 1#",
                "   ",
            },
        };

        // набор тестов для дебага
        public static patternTest[] ptTestsBank = new patternTest[]
        {
            new patternTest {
                Name = "111 FREE CELL",

                GameField = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "    ",
                    " 111",
                    " ###",
                }),

                Pattern = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "    ",
                    " 111",
                    " ##^",
                }),

                Result = true,
            },

            new patternTest {
                Name = "W21 FREE CELL",

                GameField = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "    ",
                    "M21 ",
                    " ###",
                }),

                Pattern = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "    ",
                    "W21 ",
                    " ##^",
                }),

                Result = true,
            },

            new patternTest {
                Name = "121 CLASSIC",

                GameField = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "   ",
                    "121",
                    "###",
                }),

                Pattern = PatternAnalyzer.loadPatternFromStringArr(new string[] {
                    "   ",
                    "121",
                    "M#M",
                }),

                Result = true,
            },
        };
    }
}
