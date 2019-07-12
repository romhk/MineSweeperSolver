using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{


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
                "M#M",
            },

            new string[] {
                "    ",
                "1221",
                "#MM#",
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

        };
    }
}
