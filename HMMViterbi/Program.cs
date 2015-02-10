using System;
using System.IO;
using System.Reflection;

namespace CSEP590A.HW4.HMMViterbi
{
    /// <summary>
    /// Entry point for Program DiBoyed_HMMViterbi.exe
    /// Author      : Dipak Boyed
    /// Description : This program performs the Hidden Markov Model Viterbi
    ///               algorithm on a given data. It outputs the optimal score, 
    ///               alignment produced by trace-back, P value and the 
    ///               alignment matrix (optionally when sequence length is 
    ///               less than 20) on a given sequence pair.
    /// </summary>
    class Program
    {
        #region static Member
        private static string FASTAFile = String.Empty;
        private static string EmissionFile = String.Empty;
        private static string TransitionFile = String.Empty;
        #endregion

        #region static Methods
        /// <summary>
        ///  Method       : ShowUsage
        ///  Author       : Dipak Boyed
        ///  Description  : Prints Help menu
        /// </summary>
        static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Help Menu: '{0}'", Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine();
            Console.WriteLine("Performs HMM Viterbi algorithm on a given DNA sequence to find high GC patches.");
            Console.WriteLine("Outputs the number and size of high GC content patches, log probability of the Viterbi path.");
            Console.WriteLine("Also implements Viterbi training over 10 iterations.");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine("{0} <sequence> [-e <emission file>] [-t <transition file>]", Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine();
            Console.WriteLine("Options: ");
            Console.WriteLine(" <sequence1>            FASTA file representing the DNA sequence.");
            Console.WriteLine(" [-e <emission file>]   Optional file containing initial emission values.");
            Console.WriteLine(" [-t <transition file>] Optional file containing initial transition values.");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("By Default, uses the following emissions and transition values:");
            Console.WriteLine(" Emissions: ");
            Console.WriteLine("                          A     C     G     T ");
            Console.WriteLine("   LowGCState  (State1)  0.25, 0.25, 0.25, 0.25");
            Console.WriteLine("   HighGCState (State2)  0.20, 0.30, 0.30, 0.20");
            Console.WriteLine(" Transitions: ");
            Console.WriteLine("                         State1, State2");
            Console.WriteLine("   LowGCState  (State1)  0.9999, 0.0001");
            Console.WriteLine("   HighGCState (State2)  0.01  , 0.99  ");
            Console.WriteLine("   BeginState            0.9999, 0.0001");
            Console.WriteLine("<emission file> and <transition file> must contain a ',' separated list of values");
            Console.WriteLine("only with one line for each row of values and the same order shown above.");
        }

        /// <summary>
        ///  Method       : ValidateArguments
        ///  Author       : Dipak Boyed
        ///  Description  : Validate the arguments count and values passed by user.
        /// </summary>
        /// <param name="args">string array representing the arguments</param>
        /// <returns>True if arguments are valid, false otherwise.</returns>
        static bool ValidateArguments(string[] args)
        {
            if (args.Length < 1 || args.Length > 5 || args.Length % 2 == 0)
            {
                return false;
            }
            else
            {
                Program.FASTAFile = args[0];
                if (args.Length > 1)
                {
                    if (args[1].Equals("-e", StringComparison.OrdinalIgnoreCase))
                    {
                        Program.EmissionFile = args[2];
                    }
                    else if (args[1].Equals("-t", StringComparison.OrdinalIgnoreCase))
                    {
                        Program.TransitionFile = args[2];
                    }
                    else
                    {
                        Console.WriteLine("Unknown arg '{0}' found. Must be either '-e' or '-t'.", args[1]);
                        return false;
                    }
                }
                if (args.Length == 5)
                {
                    if (args[3].Equals("-e", StringComparison.OrdinalIgnoreCase))
                    {
                        if (String.IsNullOrEmpty(Program.EmissionFile))
                        {
                            Program.EmissionFile = args[4];
                        }
                        else
                        {
                            Console.WriteLine("Cannot use '-e' option more than once.");
                            return false;
                        }
                    }
                    else if (args[3].Equals("-t", StringComparison.OrdinalIgnoreCase))
                    {
                        if (String.IsNullOrEmpty(Program.TransitionFile))
                        {
                            Program.TransitionFile = args[4];
                        }
                        else
                        {
                            Console.WriteLine("Cannot use '-t' option more than once.");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown arg '{0}' found. Must be either '-e' or '-t'.", args[3]);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///  Method       : Main
        ///  Author       : Dipak Boyed
        ///  Description  : Entry point of the program DiBoyed_SequenceAlignment
        /// </summary>
        /// <param name="args">string array representing the arguments passed by user</param>
        static void Main(string[] args)
        {
            if (!Program.ValidateArguments(args))
            {
                Program.ShowUsage();
                return;
            }
            try
            {
                Console.WriteLine("Performing HMM Viterbi algorithm...");
                Foo viterbi = new Foo(Program.FASTAFile, Program.EmissionFile, Program.TransitionFile);
                for (int i = 1; i <= 10; i++)
                {
                    viterbi.Compute();
                    Console.WriteLine("*************************************");
                    Console.WriteLine("Displaying Results of iteration {0}:", i);
                    Console.WriteLine();
                    viterbi.Print();
                    Console.WriteLine("*************************************");
                    Console.WriteLine();
                    viterbi.RecalculateEmissionTransmission();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
            }
        }
        #endregion
    }
}
