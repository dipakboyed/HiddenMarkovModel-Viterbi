using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSEP590A.HW4.HMMViterbi
{
    internal enum HMMState : int
    {
        LowGCState = 0,
        HighGCState = 1,
        BeginState = 2,
    }

    internal enum DNABase : int
    {
        A = 0,
        C = 1,
        G = 2,
        T = 3,
    }

    internal class SingleStateData
    {
        public DNABase Base;
        public double[,] probabilities;
        public double maxpLowGCState;
        public double maxpHighGCState;

        public HMMState ViterbiPath;

        public SingleStateData(DNABase dnaBase)
        {
            this.Base = dnaBase;
            this.probabilities = new double[2, 2];
        }
    }

    internal class Foo : HMMViterbiAlgorithm
    {
        internal Foo(string fastaFile, string emissionFile, string transitionFile) : base(fastaFile, emissionFile, transitionFile) { }
    }
    /// <summary>
    ///  Class        : HMMViterbiAlgorithm
    ///  Author       : Dipak Boyed
    ///  Description  : class representing the HMM Viterbi algorithm to perform.
    /// </summary>
    internal class HMMViterbiAlgorithm
    {
        #region private Members
        private double[,] Emissions = new double[,] {
        /*                            A     C     G     T    */
        /* LowGCState  (State1) */ { 0.25, 0.25, 0.25, 0.25},
        /* HighGCState (State2) */ { 0.20, 0.30, 0.30, 0.20}
        };

        private double[,] Transitions = new double[,] {
        /*                           State1, State2 */
        /* LowGCState  (State1) */ { 0.9999, 0.0001},
        /* HighGCState (State2) */ { 0.01  , 0.99  },
        /* BeginState           */ { 0.9999, 0.0001}
        };
        private List<SingleStateData> ViterbiData;
        private double[,] emissionRelatedData;
        private double[,] transitionRelatedData;
        #endregion

        #region Constructors
        /// <summary>
        ///  Method       : Ctor
        ///  Author       : Dipak Boyed
        ///  Description  : Constructs an HMMViterbiAlgorithm instance.
        ///                 Also parses the DNA sequence from a FASTA file
        /// </summary>
        /// <param name="sequence1">FASTA file name containing DNA sequence.</param>
        /// <param name="emissionFile">File containing comma-separated emission values</param>
        /// <param name="transitionFile">File containing comma-separated transition values</param>
        internal HMMViterbiAlgorithm(string fastaFile, string emissionFile, string transitionFile)
        {
            this.ViterbiData = new List<SingleStateData>();
            this.ReadSequence(fastaFile);
            if (!String.IsNullOrEmpty(emissionFile))
            {
                this.ReadInitialEmissionValues(emissionFile);
            }
            if (!String.IsNullOrEmpty(transitionFile))
            {
                this.ReadInitialTransitionValues(transitionFile);
            }
            this.emissionRelatedData = new double[2, 4];
            this.transitionRelatedData = new double[2, 2];
        }
        #endregion

        #region Helper Methods
        /// <summary>
        ///  Method       : ValidateSequence
        ///  Author       : Dipak Boyed
        ///  Description  : Ensures a valid FASTA file is specified.
        ///                 Also ensures each character in the sequence is a valid DNA base.
        ///                 Reads DNA sequence into internal viterbi Data structure.
        /// </summary>
        /// <param name="sequence">string representing FASTA file name.</param>
        private void ReadSequence(string fastaFile)
        {
            Console.WriteLine();
            Console.WriteLine(this.GetType().FullName);
            Console.WriteLine();
            string validatedSequence = String.Empty;
            if (String.IsNullOrEmpty(fastaFile))
            {
                throw new ArgumentException("FASTA file name cannot be null or empty string.", fastaFile);
            }

            if (File.Exists(fastaFile))
            {
                // sequence specified as a FASTA file
                Console.WriteLine("     File '{0}' exists. Attempting to read FASTA file...", Path.GetFileName(fastaFile));
                int lineNumber = 0;
                Console.Write("Reading line number: {0,5}", lineNumber);
                using (StreamReader reader = new StreamReader(fastaFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (Console.CursorLeft >= 5)
                        {
                            Console.SetCursorPosition(Console.CursorLeft - 5, Console.CursorTop);
                            Console.Write("{0,5}", ++lineNumber);
                        }
                        if (line.StartsWith(">"))
                        {
                            if (Console.CursorLeft >= 26)
                            {
                                Console.SetCursorPosition(Console.CursorLeft - 26, Console.CursorTop);
                                Console.WriteLine("     Ignoring comments:\t'{0}'", line);
                                Console.Write("Reading line number: {0,5}", lineNumber);
                            }
                        }
                        else
                        {
                            validatedSequence += line;
                        }
                    }
                }
                Console.WriteLine();
            }
            else
            {
               throw new ArgumentException(String.Format("FASTA file '{0}' not found. Must specify a valid FASTA file", Path.GetFullPath(fastaFile)));
            }

            // Ensure each character in the sequence is a valid amino acid
            for (int i = 0; i < validatedSequence.Length; i++)
            {
                DNABase currentBase = DNABase.T;
                switch(validatedSequence[i])
                {
                    case 'A':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                        currentBase = DNABase.A;
                        break;
                    case 'C':
                    case '6':
                        currentBase = DNABase.C;
                        break;
                    case 'G':
                        currentBase = DNABase.G;
                        break;
                    case 'T':
                        currentBase = DNABase.T;
                        break;
                    default:
                        Console.WriteLine("Found unknown base '{0}' at position '{1}'. Treating it as base 'T'.", validatedSequence[i], i);
                        currentBase = DNABase.T;
                        break;
                }
                this.ViterbiData.Add(new SingleStateData(currentBase));
            }
            Console.WriteLine("     Successfully read sequence of length {0}.", this.ViterbiData.Count);
        }

        private void ReadInitialEmissionValues(string emissionFile)
        {
            if (String.IsNullOrEmpty(emissionFile))
            {
                throw new ArgumentException("Emission file name cannot be null or empty string.", emissionFile);
            }

            if (File.Exists(emissionFile))
            {
                int validLineNumber = 0;
                using (StreamReader reader = new StreamReader(emissionFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        if (!String.IsNullOrEmpty(line))
                        {
                            validLineNumber++;
                            string[] values = line.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length != 4)
                            {
                                throw new InvalidDataException(String.Format("Non-empty line {0} of emission file {1} contains {2} values. It must contain exactly four values.",
                                                                              validLineNumber,
                                                                              Path.GetFileName(emissionFile),
                                                                              values.Length));
                            }
                            if (validLineNumber > 2)
                            {
                                throw new InvalidDataException(String.Format("Found more than two lines of values in emission file {0}. It must contain exactly two lines of data.",
                                                                             Path.GetFileName(emissionFile)));
                            }
                            this.Emissions[validLineNumber - 1, 0] = Convert.ToDouble(values[0]);
                            this.Emissions[validLineNumber - 1, 1] = Convert.ToDouble(values[1]);
                            this.Emissions[validLineNumber - 1, 2] = Convert.ToDouble(values[2]);
                            this.Emissions[validLineNumber - 1, 3] = Convert.ToDouble(values[3]);
                        }
                    }
                }
                if (validLineNumber != 2)
                {
                    throw new InvalidDataException(String.Format("Found less than two lines of values in emission file {0}. It must contain exactly two lines of data.",
                                                                             Path.GetFileName(emissionFile)));
                }
            }
            else
            {
                throw new ArgumentException(String.Format("Emission file '{0}' not found. Must specify a valid file name.", Path.GetFullPath(emissionFile)));
            }
        }

        private void ReadInitialTransitionValues(string transitionFile)
        {
            if (String.IsNullOrEmpty(transitionFile))
            {
                throw new ArgumentException("Transition file name cannot be null or empty string.", transitionFile);
            }

            if (File.Exists(transitionFile))
            {
                int validLineNumber = 0;
                using (StreamReader reader = new StreamReader(transitionFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        if (!String.IsNullOrEmpty(line))
                        {
                            validLineNumber++;
                            string[] values = line.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length != 2)
                            {
                                throw new InvalidDataException(String.Format("Non-empty line {0} of transition file {1} contains {2} values. It must contain exactly two values.",
                                                                              validLineNumber,
                                                                              Path.GetFileName(transitionFile),
                                                                              values.Length));
                            }
                            if (validLineNumber > 3)
                            {
                                throw new InvalidDataException(String.Format("Found more than three lines of values in emission file {0}. It must contain exactly three lines of data.",
                                                                             Path.GetFileName(transitionFile)));
                            }
                            this.Transitions[validLineNumber - 1, 0] = Convert.ToDouble(values[0]);
                            this.Transitions[validLineNumber - 1, 1] = Convert.ToDouble(values[1]);
                        }
                    }
                }
                if (validLineNumber != 3)
                {
                    throw new InvalidDataException(String.Format("Found less than three lines of values in emission file {0}. It must contain exactly three lines of data.",
                                                                             Path.GetFileName(transitionFile)));
                }
            }
            else
            {
                throw new ArgumentException(String.Format("Transition file '{0}' not found. Must specify a valid file name.", Path.GetFullPath(transitionFile)));
            }
        }
        #endregion

        #region public Methods
        public void Compute()
        {
            // compute the state probabilities for each base in the sequence
            double previousLowGCStateLogProb  = Math.Log(this.Transitions[(int)HMMState.BeginState, (int)HMMState.LowGCState]);
            double previousHighGCStateLogProb = Math.Log(this.Transitions[(int)HMMState.BeginState, (int)HMMState.HighGCState]);
            for (int i = 0; i < this.ViterbiData.Count; i++)
            {
                double lowGCEmissionLogProb  = Math.Log(this.Emissions[(int)HMMState.LowGCState,  (int)this.ViterbiData[i].Base]);
                double highGCEmissionLogProb = Math.Log(this.Emissions[(int)HMMState.HighGCState, (int)this.ViterbiData[i].Base]);
                if (i == 0)
                {
                    this.ViterbiData[i].probabilities[0, 0] = previousLowGCStateLogProb +
                                                              lowGCEmissionLogProb;
                    this.ViterbiData[i].probabilities[0, 1] = this.ViterbiData[i].probabilities[0, 0];

                    this.ViterbiData[i].probabilities[1, 0] = previousHighGCStateLogProb +
                                                              highGCEmissionLogProb;
                    this.ViterbiData[i].probabilities[1, 1] = this.ViterbiData[i].probabilities[1, 0];
                }
                else
                {
                    this.ViterbiData[i].probabilities[0, 0] = previousLowGCStateLogProb +
                                                              Math.Log(this.Transitions[(int)HMMState.LowGCState, (int)HMMState.LowGCState]) +
                                                              lowGCEmissionLogProb;
                    this.ViterbiData[i].probabilities[0, 1] = previousHighGCStateLogProb +
                                                              Math.Log(this.Transitions[(int)HMMState.HighGCState, (int)HMMState.LowGCState]) +
                                                              lowGCEmissionLogProb;

                    this.ViterbiData[i].probabilities[1, 0] = previousLowGCStateLogProb +
                                                              Math.Log(this.Transitions[(int)HMMState.LowGCState, (int)HMMState.HighGCState]) +
                                                              highGCEmissionLogProb;
                    this.ViterbiData[i].probabilities[1, 1] = previousHighGCStateLogProb +
                                                              Math.Log(this.Transitions[(int)HMMState.HighGCState, (int)HMMState.HighGCState]) +
                                                              highGCEmissionLogProb;
                }
                this.ViterbiData[i].maxpLowGCState  = Math.Max(this.ViterbiData[i].probabilities[0, 0],
                                                               this.ViterbiData[i].probabilities[0, 1]);
                this.ViterbiData[i].maxpHighGCState = Math.Max(this.ViterbiData[i].probabilities[1, 0],
                                                               this.ViterbiData[i].probabilities[1, 1]);

                previousLowGCStateLogProb = this.ViterbiData[i].maxpLowGCState;
                previousHighGCStateLogProb = this.ViterbiData[i].maxpHighGCState;
            }

            // trace back to calculate the Viterbi Path now
            // also keep track of emission/transition statistics that can be used to re-compute Emission/Transition values (E-M)
            for (int i = this.ViterbiData.Count - 1; i >= 0; i--)
            {
                if (i == this.ViterbiData.Count - 1)
                {
                    if (this.ViterbiData[i].maxpHighGCState > this.ViterbiData[i].maxpLowGCState)
                    {
                        this.ViterbiData[i].ViterbiPath = HMMState.HighGCState;
                    }
                    else
                    {
                        this.ViterbiData[i].ViterbiPath = HMMState.LowGCState;
                    }
                }
                else
                {
                    if (((this.ViterbiData[i + 1].ViterbiPath == HMMState.HighGCState) &&
                         (this.ViterbiData[i + 1].maxpHighGCState == this.ViterbiData[i + 1].probabilities[1, 1])) ||
                        ((this.ViterbiData[i + 1].ViterbiPath == HMMState.LowGCState) &&
                         (this.ViterbiData[i + 1].maxpLowGCState == this.ViterbiData[i + 1].probabilities[0, 1])))
                    {
                        this.ViterbiData[i].ViterbiPath = HMMState.HighGCState;
                    }
                    else
                    {
                        this.ViterbiData[i].ViterbiPath = HMMState.LowGCState;
                    }
                    this.transitionRelatedData[(int)this.ViterbiData[i].ViterbiPath, (int)this.ViterbiData[i + 1].ViterbiPath]++;
                }
                this.emissionRelatedData[(int)this.ViterbiData[i].ViterbiPath, (int)this.ViterbiData[i].Base]++;
            }
        }

        /// <summary>
        ///  Method       : Print
        ///  Author       : Dipak Boyed
        ///  Description  : Print results of the sequence alignment.
        ///                 Prints the following:
        ///                 1. Alignment matrix if sequence length less than 20
        ///                 2. optimal score value and location
        ///                 3. optimal sequence (BLAST format)
        ///                 4. p-value
        /// </summary>
        public void Print()
        {
            Console.WriteLine();
            StringBuilder stringBuilder = new StringBuilder(" Printing Emission values...");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(" ---------------------------------------------");
            stringBuilder.AppendLine("                      |  A     C     G     T  ");
            stringBuilder.AppendLine(" ---------------------------------------------");
            stringBuilder.Append(" LowGCState  (State1) |");
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append(String.Format(" {0:0.00},", this.Emissions[0,i]));
            }
            stringBuilder.AppendLine();
            stringBuilder.Append(" HighGCState (State2) |");
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append(String.Format(" {0:0.00},", this.Emissions[1, i]));
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(" ---------------------------------------------");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(" Printing Transition values...");
            stringBuilder.AppendLine(" --------------------------------------");
            stringBuilder.AppendLine("                      |  State1, State2");
            stringBuilder.AppendLine(" --------------------------------------");
            stringBuilder.Append(" LowGCState  (State1) |");
            stringBuilder.Append(String.Format(" {0} , {1}", this.Transitions[0, 0], this.Transitions[0, 1]));
            stringBuilder.AppendLine();
            stringBuilder.Append(" HighGCState (State2) |");
            stringBuilder.Append(String.Format(" {0} , {1}", this.Transitions[1, 0], this.Transitions[1, 1]));
            stringBuilder.AppendLine();
            stringBuilder.Append(" BeginState           |");
            stringBuilder.Append(String.Format(" {0} , {1}", this.Transitions[2, 0], this.Transitions[2, 1]));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(" --------------------------------------");
            Console.WriteLine(stringBuilder.ToString());
            Console.WriteLine();
            Dictionary<int, int> hitAreas = new Dictionary<int, int>();
            bool currentlyInHitArea = false;
            int currentHitStart = -1;
            int currentHitEnd = -1;
            for (int i = 0; i < this.ViterbiData.Count; i++)
            {
                if (!currentlyInHitArea && (this.ViterbiData[i].ViterbiPath == HMMState.HighGCState))
                {
                    currentHitStart = i + 1;
                    currentlyInHitArea = true;
                }
                else if (currentlyInHitArea && (this.ViterbiData[i].ViterbiPath == HMMState.LowGCState))
                {
                    currentHitEnd = i;
                    hitAreas.Add(currentHitStart, currentHitEnd);
                    currentlyInHitArea = false;
                    currentHitStart = -1;
                    currentHitEnd = -1;
                }
            }
            if (currentlyInHitArea)
            {
                hitAreas.Add(currentHitStart, this.ViterbiData.Count);
            }
            Console.WriteLine(" No. of hit areas: '{0}'", hitAreas.Count);
            Console.WriteLine(" Location,  Length");
            foreach (int key in hitAreas.Keys)
            {
                Console.WriteLine(" {0}...{1}, {2}", key, hitAreas[key], hitAreas[key] - key + 1);
            }
            Console.WriteLine(" Log Probability of Viterbi Path : {0:e}", 
                               Math.Max(this.ViterbiData[this.ViterbiData.Count-1].maxpHighGCState,
                                        this.ViterbiData[this.ViterbiData.Count-1].maxpLowGCState)
                             );
        }

        public void RecalculateEmissionTransmission()
        {
            for (int i = 0; i < 2; i++)
            {
                double total = this.emissionRelatedData[i,0] + this.emissionRelatedData[i,1] + this.emissionRelatedData[i,2] + this.emissionRelatedData[i,3];
                if ((int)this.ViterbiData[this.ViterbiData.Count - 1].ViterbiPath == i)
                {
                    // the last base in the sequence does not transition into a following base
                    // so don't account for the last base
                    total--;
                }
                for (int j = 0; j < 4; j++)
                {
                    if (total <= 0)
                    {
                        this.Emissions[i, j] = 0;
                    }
                    else
                    {
                        this.Emissions[i, j] = this.emissionRelatedData[i, j] / total;
                    }
                }
                for (int j = 0; j < 2; j++)
                {
                    if (total <= 0)
                    {
                        this.Transitions[i, j] = 0;
                    }
                    else
                    {
                        this.Transitions[i, j] = this.transitionRelatedData[i, j] / total;
                    }
                }
            }

            // reset data
            this.emissionRelatedData = new double[2, 4];
            this.transitionRelatedData = new double[2, 2];
            for (int i = 0; i < this.ViterbiData.Count; i++)
            {
                this.ViterbiData[i] = new SingleStateData(this.ViterbiData[i].Base);
            }
        }
        #endregion
    }
}
