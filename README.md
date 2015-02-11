# HiddenMarkovModel-Viterbi
Using HMM (Hidden Markov Model) Viterbi algorithm
http://courses.cs.washington.edu/courses/csep590a/13sp/hw/hw4.html
HW4: HMM Viterbi Algorithm
---------------------------------------------------------------------

List of files in the repro:
------------------------------
Report.pdf  	: Summary Report
HMMViterbi		: Source code directory
OutputLogs		: Directory containing output logs

What is this Project about?
------------------------------------
See the Report pdf for details

How to run this program?
-----------------------------------
1. f5 the project to build and run

Performs HMM Viterbi algorithm on a given DNA sequence to find high GC patches.
Outputs the number and size of high GC content patches, log probability of the Viterbi path.
Also implements Viterbi training over 10 iterations.

Usage:
-----------------------------------
DiBoyed_HMMViterbi.exe <sequence> [-e <emission file>] [-t <transition file>]

Options:

 <sequence1>            FASTA file representing the DNA sequence.
 
 [-e <emission file>]   Optional file containing initial emission values.
 
 [-t <transition file>] Optional file containing initial transition values.
 

Notes:
------------------------------------
By Default, uses the following emissions and transition values:
 Emissions:
 
                          A     C     G     T
                          
   LowGCState  (State1)  0.25, 0.25, 0.25, 0.25
   
   HighGCState (State2)  0.20, 0.30, 0.30, 0.20
   
 Transitions:
 
                         State1, State2
                         
   LowGCState  (State1)  0.9999, 0.0001
   
   HighGCState (State2)  0.01  , 0.99
   
   BeginState            0.9999, 0.0001
   

<emission file> and <transition file> must contain a ',' separated list of values
only with one line for each row of values and the same order shown above.

