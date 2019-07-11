using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{


    public class PatternBank
    {
        public static string[][] BankMain = new string[][] {
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
                " 3 ",
                "MMM",
            },

            new string[] {
                "   ",
                " 3М",
                " MM",
            },

            new string[] {
                "    ",
                " 12 ",
                " ##M",
            },

            new string[] {
                "   ",
                " 4M",
                "MMM",
            },

            new string[] {
                "  M",
                " 5M",
                "MMM",
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
