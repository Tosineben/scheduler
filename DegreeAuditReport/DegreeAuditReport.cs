using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DegreeAuditReport
{
    public enum Status
    {
        RequirementSatisfied, //OK
        InProgress, //IP
        SubsetCompleted, //+ 
        SatisfiedCourse, //SC
        SatisfiedRequirement, //SR
        CreditPending, //CP
        MultiTermCourse, //>+
        RetakenCourse, //>R
        DuplicateClass, //>D
        NoGrade, //NG
        RequirementNotSatisfied, //NO
        FutureTerm, //FT
        SubsetIncomplete, //- 
        AdvancedPlacement, //AP
        TransferCreditFromColumbia, //TI
        YearLongCourse, //YC
        TransferCreditFromOther, // TR
        UnofficialWithdrawl, // UW
    }

    public class AcademicProgressReport
    {
        public double CreditsEarned { get; set; } //EARNED:106.50 Points
        public double CreditsInProgress { get; set; } //IN PROGRESS 17.00 Points
        public double CumulativeGpa { get; set; } //4.240 GPA
        public List<SemesterSummary> Semesters { get; set; } 
    }

    public enum Term
    {
        Fall,
        Summer,
        Spring
    }

    public class SemesterSummary
    {
        public int Year { get; set; }
        public Term Term { get; set; }
        public double CreditsEarned { get; set; }
        public double Gpa { get; set; }
        public List<CourseSummary> Courses { get; set; } 
    }

    public class CourseSummary
    {
        public int Year { get; set; }
        public Term Term { get; set; }
        public string Department { get; set; }
        public int CourseNumber { get; set; }
        public char CourseLetter { get; set; }
        public int SectionNumber { get; set; }
        public double Credits { get; set; }
        public string Grade { get; set; } // could be AP, B-, A , FT>+, NG, P >+, IP, ....
        public string CourseName { get; set; }
        public string ProcessedAs { get; set; } // on next line: PROCESSED AS: CHEM2407C07CHEM
        public string MatchedAs { get; set; } // on next line: >>MATCHED AS: COMS4119       
        public Status Status { get; set; } // check this for transfer, AP, future course, etc.
    }

    public class NonTechRequirements
    {
        public bool Completed { get; set; } // NO     SUMMARY:  NON-TECHNICAL (27 PTS)      
        public double CreditsEarned { get; set; } //EARNED: 14.00 Points 
        public double CreditsInProgress { get; set; } //IN PROGRESS  7.00 Points  
        public int RequirementsNeeded { get; set; } //NEED:                             2 REQUIREMENTS
        public Section NonTechCourses { get; set; }
        public Section GlobalCore { get; set; }
        public Section Electives { get; set; }
        public Section Gateway { get; set; }
        public Section PhysEd { get; set; }
    }

    public class Section
    {
        public string Name { get; set; }
        public bool Complete { get; set; }
        public List<SubSection> SubSections { get; set; } 
        // don't need the optional CreditsEarned, just a sum of sub sections
    }

    public class SubSection
    {
        public bool Complete { get; set; }
        public string Name { get; set; } // might be NULL
        public List<CourseSummary> Courses { get; set; } 
        public string SelectFrom { get; set; }
        // don't need the optional CreditsEarned, just a sum courses
        public List<SubSection> Or { get; set; } 
    }

    public class MajorRequirements
    {
        public string ProgramName { get; set; } //MAJOR IN COMPUTER SCIENCE REQUIREMENTS
        public List<Section> Sections { get; set; } 
    }

    public class MajorStatus
    {
        public bool HasRemainingReqs { get; set; }
        public double CreditsEarned { get; set; }
        public List<Section> Sections { get; set; } 
    }

    public class DegreeRequirements
    {
        public bool Completed { get; set; }
        public double CreditsRequired { get; set; }
        public double RequiredMinGpa { get; set; }
        public double CreditsEarned { get; set; }
        public double CreditsInProgress { get; set; }
        public double CreditsNeeded { get; set; }
        public double CreditsFromTransferOrAp { get; set; }
        public double CreditsFromColumbia { get; set; }
        public double Gpa { get; set; }
        public List<CourseSummary> Courses { get; set; } 
    }

    public class MinorRequirements
    {
        public string ProgramName { get; set; }
        public int SubsetsEarned { get; set; }
        public int SubsetsNeeded { get; set; }
        public List<SubSection> SubSections { get; set; } 
    }

    public class DegreeAuditReport
    {
        public DateTime PreparedDate { get; set; } //PREPARED: 09/01/13 - 11:18 
        public DateTime? GraduationDate { get; set; } //GRADUATION DATE: **/**/**
        public int CatalogYear { get; set; } //CATALOG YEAR: 2012  
        public string ProgramCode { get; set; } //PROGRAM CODE: ENCOMS
        public string Program { get; set; } //DEGREE AUDIT FOR COMPUTER SCIENCE PROGRAM 
        public bool HasRemainingReqs { get; set; } //---->  AT LEAST ONE REQUIREMENT HAS NOT BEEN SATISFIED <----
        public NonTechRequirements NonTechReqs { get; set; }
        public MajorRequirements MajorReqs { get; set; }
        public MajorStatus MajorStatus { get; set; }
        public MinorRequirements MinorReqs { get; set; }
        public List<CourseSummary> NotApplied { get; set; }
        public DegreeRequirements DegreeReqs { get; set; }
        public AcademicProgressReport Summary { get; set; }

        public static DegreeAuditReport Parse(string text)
        {
            var dar = new DegreeAuditReport();

            var sections = text.Split(new []{BREAK}, StringSplitOptions.RemoveEmptyEntries).ToList();

            FillMainSection(dar, sections[0]);

            dar.HasRemainingReqs = sections[1].Contains(REMAINING_REQ);

            for (var i = 2; i < sections.Count; i++)
            {
                var section = sections[i];

                if (section.Contains(NONTECH))
                {
                    var nonTechSections = new List<string>{section};
                    for (var j = i + 1; j < sections.Count; j++)
                    {
                        //if (section.Contains())
                        //{
                            
                        //}
                    }
                }
            }

            return dar;
        }

        private static void FillMainSection(DegreeAuditReport dar, string mainSection)
        {
            var prepared = Regex.Match(mainSection, "PREPARED:(.*)\n").Groups[1].Value.Trim();
            dar.PreparedDate = DateTime.ParseExact(prepared, "MM/dd/yy - HH:mm", CultureInfo.InvariantCulture);

            var graduation = Regex.Match(mainSection, "GRADUATION DATE:(.*)\n").Groups[1].Value.Trim();
            DateTime tmp;
            if (DateTime.TryParse(graduation, out tmp))
            {
                dar.GraduationDate = tmp;
            }

            var catalog = Regex.Match(mainSection, "CATALOG YEAR:(.*)\n").Groups[1].Value.Trim();
            dar.CatalogYear = int.Parse(catalog);

            dar.ProgramCode = Regex.Match(mainSection, "PROGRAM CODE:(.*)CATALOG YEAR:").Groups[1].Value.Trim();
            dar.Program = Regex.Match(mainSection, "DEGREE AUDIT FOR (.*) PROGRAM").Groups[1].Value.Trim();
        }

        private readonly Dictionary<string, Status> _statuses = new Dictionary<string, Status>
        {
            {"OK", Status.RequirementSatisfied},
            {"IP", Status.InProgress},
            {"+ ", Status.SubsetCompleted},
            {"SC", Status.SatisfiedCourse},
            {"SR", Status.SatisfiedRequirement},
            {"CP", Status.CreditPending},
            {">+", Status.MultiTermCourse},
            {">R", Status.RetakenCourse},
            {">D", Status.DuplicateClass},
            {"NG", Status.NoGrade},
            {"NO", Status.RequirementNotSatisfied},
            {"FT", Status.FutureTerm},
            {"- ", Status.SubsetIncomplete},
            {"AP", Status.AdvancedPlacement},
            {"TI", Status.TransferCreditFromColumbia},
            {"YC", Status.YearLongCourse},
            {"TR", Status.TransferCreditFromOther},
            {"UW", Status.UnofficialWithdrawl},
        };

        private const string BREAK = @"_________________________________________________________________";
        private const string REMAINING_REQ = @"---->  AT LEAST ONE REQUIREMENT HAS NOT BEEN SATISFIED <----";
        private const string NONTECH = @">>>>>>>>>         NON-TECHNICAL REQUIREMENTS        <<<<<<<<<<";
        private const string BEGIN_AUDIT = @"======          BEGINNING OF AUDIT FOR          ======";

        private const string EXAMPLE = @"
PREPARED: 09/01/13 - 11:18                                       
                                        GRADUATION DATE: **/**/**
PROGRAM CODE: ENCOMS                         CATALOG YEAR: 2012  
UNOFFICIAL DOCUMENT -- FOR ADVISEMENT ONLY                       
            DEGREE AUDIT FOR COMPUTER SCIENCE PROGRAM            
_________________________________________________________________
---->  AT LEAST ONE REQUIREMENT HAS NOT BEEN SATISFIED <----     
_________________________________________________________________
 THIS DOCUMENT IS A GUIDE FOR ADVISEMENT.  YOUR ADVISOR          
 IS THE FINAL AUTHORITY ON WHETHER REQUIREMENTS ARE MET.         
                                                                 
>>>FOR QUESTIONS REGARDING AP & TRANSFER CREDIT, AND FIRST 2     
   YEAR REQUIREMENT EXEMPTIONS, CONTACT YOUR CLASS CENTER        
                                                                 
>>>FOR QUESTIONS REGARDING YOUR MAJOR, AND EXCEPTIONS FOR        
   MAJOR REQUIREMENTS CONTACT YOUR DEPARTMENTAL ADVISOR          
                                                                 
A KEY TO ABBREVIATIONS IN THIS REPORT APPEARS AT THE END.        
_________________________________________________________________
=================================================================
>>>>>>>>>         NON-TECHNICAL REQUIREMENTS        <<<<<<<<<<   
NO     SUMMARY:  NON-TECHNICAL (27 PTS)                          
       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                          
    EARNED: 14.00 Points                                         
IN PROGRESS  7.00 Points                                         
    NEED:                             2 REQUIREMENTS             
_________________________________________________________________
NO     NON-TECHNICAL COURSE REQUIREMENTS                         
    EARNED:  7.00 Points                                         
                                                                 
  +     UNIVERSITY WRITING                                       
      SP10 ENGL 1010 C 32 3.00 A-   UNIVERSITY WRITING           
                                                                 
  +     ECONOMICS                                                
      SP10 ECON 1105 W 02 4.00 A+   PRINCIPLES OF ECONOMICS      
                                                                 
  -     HUMANITIES                                               
        SELECT FROM: HUMA1121,1123                               
_________________________________________________________________
CC GLOBAL CORE REQUIREMENT (MCGLOBAL)                            
OK     (2)Approved Courses taken for a letter grade.             
          See College Bulletin for details.                      
 >I                                                              
                                                                 
+     SP11 ASCM 2008 V 01 4.00 A    CONTEMP ISLAMIC CIVILIZAT    
      FA13 ASCE 2002 V 01 4.00 FT   INTRO MAJOR TOPICS: EAST     
_________________________________________________________________
       ELECTIVES (9-11 PTS):   NON-TECHNICAL                     
                                                                 
  -     Only courses on the approved list available from your    
        Academic Advisor or Class Dean qualify                   
        No Studio, Performance, Rehearsal, Laboratory,           
        Field Work, Speech, Visual Arts, A&HG, A&HA courses.     
           6.00 Points Earned                                    
      SP13 ECON 2257 W 01 3.00 A    THE GLOBAL ECONOMY           
      FA13 ECON 3265 V 01 3.00 FT   ECONOMICS OF MONEY AND BA    
_________________________________________________________________
OK     GATEWAY LAB                                               
                                                                 
+     FA09 ENGI 1102 E 04 4.00 A    DES FUND USING ADV COMP      
_________________________________________________________________
OK     PHYSICAL EDUCATION                                        
                                                                 
  +     2 courses in Physical Education                          
      FA09 PHED 1001 C 24 1.00 P >+ PHYSICAL ED: BASKETBALL      
      FA10 PHED 1001 C 52 1.00 P >+ PHYSICAL ED: BASKETBALL      
_________________________________________________________________
======          BEGINNING OF AUDIT FOR          ======           
======  MAJOR IN COMPUTER SCIENCE REQUIREMENTS  ======           
_________________________________________________________________
NO     MATHEMATICS REQUIREMENT (BULLETIN YEAR 2010 & ABOVE)      
                                                                 
          MATH 1101, 1102, 1201                                  
                                                                 
-     FA09 MATH 1201 V 06 3.00 A+   CALCULUS III                 
_________________________________________________________________
OK     CHEMISTRY LECTURE REQUIREMENT                             
                                                                 
      FA09 CHEM 1403 C 02 3.50 A+   GENERAL CHEMISTRY I-LECTU    
_________________________________________________________________
OK     PHYSICS LECTURE REQUIREMENT                               
                                                                 
      FA09 PHYS 1601 C 01 3.50 A+   PHYSICS I:MECHANICS/RELAT    
      SP10 PHYS 1602 C 01 3.50 A+   PHYSICS II: THERMO, ELEC     
_________________________________________________________________
OK     LABORATORY REQUIREMENT                                    
                                                                 
      SP11 PHYS 1494 C 10 3.00 A    INTRO TO EXPERIMENTAL PHY    
_________________________________________________________________
OK     PROFESSIONAL COURSE REQUIREMENT                           
       (1ST & 2ND YEAR TECHNICAL ELECTIVE)                       
                                                                 
      FA09 MECE 1001 E 01 3.00 A+   MECH ENGI:MICROMACH/JUMBO    
_________________________________________________________________
======    MAJOR IN COMPUTER SCIENCE COMPLETION STATUS ======     
======        INCLUDES CORE CURRICULUM AND ONE TRACK  ======     
======              CATALOG YEARS 2012 AND ABOVE      ======     
NO     >>>  AT LEAST ONE REQUIREMENT HAS NOT BEEN COMPLETED <<<  
    EARNED: 37.00 Points                                         
_________________________________________________________________
MAJOR IN COMPUTER SCIENCE                                        
OK     TRACK DECLARATION REQUIRED COURSE                         
 >I                                                              
  +     APPLICATIONS TRACK                                       
      SP13 COMS 0004 E 01 0.00 NG   APPLICATIONS-TRACK           
      FA13 COMS 0004 E 01 0.00 FT   APPLICATIONS-TRACK           
_________________________________________________________________
OK     COMPUTER SCIENCE 1ST & 2ND YEAR REQUIR'TS (CYR '12 & UP)  
                                                                 
  +     Intro to Computer Science (COMS1004 OR 1007)             
      FA10 COMS 1004 W 01 3.00 A+   INTRO-COMPUT SCI/PROGRAM     
                                                                 
  +     ENGI 1006 (REQUIRED)                                     
      ENGI1006 EXEMPT     0.00 SC                                
                                                                 
  +     Required Courses                                         
      SP11 COMS 3134 W 01 3.00 A+   DATA STRUCTURES IN JAVA      
      FA12 COMS 3157 W 01 4.00 A+   ADVANCED PROGRAMMING         
      FA12 COMS 3203 W 01 3.00 A    INTRO-COMBINATORICS/GRAPH    
                                                                 
_________________________________________________________________
MAJOR IN COMPUTER SCIENCE                                        
NO     COMPUTER SCIENCE 3 & 4TH YEAR CORE REQUIREMENTS           
                                                                 
-     SP13 COMS 3261 W 01 3.00 A+   COMPUTER SCIENCE THEORY      
      FA12 CSEE 3827 W 01 3.00 A+   FUNDAMENTALS OF COMPUTER     
         NEED:                  2 COURSES                        
        SELECT FROM: COMS3251  SIEO4150 OR 3600                  
_________________________________________________________________
MAJOR IN COMPUTER SCIENCE                                        
NO     TRACK:  APPLICATIONS                                      
                                                                 
  +     REQUIRED COURSES                                         
      FA13 COMS 4115 W 01 3.00 FT   PROGRAMMING LANG & TRANSL    
      FA12 COMS 4170 W 01 3.00 A+   USER INTERFACE DESIGN        
      SP13 COMS 4701 W 01 3.00 A    ARTIFICIAL INTELLIGENCE      
                                                                 
  +     ELECTIVE COURSES (4)                                     
      FA12 COMS 4111 W 02 3.00 A+   INTRODUCTION TO DATABASES    
      SP13 CSEE 4119 W 01 3.00 A+   COMPUTER NETWORKS            
                                    >>MATCHED AS: COMS4119       
      FA13 COMS 4167 W 01 3.00 FT   COMPUTER ANIMATION           
      FA13 COMS 4733 W 01 3.00 FT   COMPUTATNL ASPECTS OF ROB    
                                                                 
  -     Possible Advisor Approved Electives (COMSAPPADV)         
         NEED:    1.50 Points   5 COURSES                        
_________________________________________________________________
 ==== END OF AUDIT FOR THE MAJOR IN COMPUTER SCIENCE  ====       
_________________________________________________________________
=================================================================
MINOR IN ECONOMICS                                               
NO     MINOR IN ECONOMICS                                        
    EARNED:                           6 SUBSETS                  
    NEED:                             1 SUBSET                   
                                                                 
  +  1) Principles of Economics                                  
      SP10 ECON 1105 W 02 4.00 A+   PRINCIPLES OF ECONOMICS      
                                                                 
  +  2) Intermediate Microeconomics                              
      SP11 ECON 3211 W 01 3.00 A+   INTERMEDIATE MICROECONOMI    
                                                                 
  +  3) Intermediate Macroeconomics                              
      FA10 ECON 3213 W 01 3.00 A+   INTERMEDIATE MACROECONOM     
                                                                 
  +  4) Introduction to Econometrics                             
      SP13 ECON 3412 W 02 3.00 A+   INTRODUCTION TO ECONOMETR    
                                                                 
  +  5) Economics Elective One (at least 3 pts)                  
      SP13 ECON 2257 W 01 3.00 A    THE GLOBAL ECONOMY           
                                                                 
  +  6) Economics Elective Two (at least 3 pts)                  
      FA13 ECON 3265 V 01 3.00 FT   ECONOMICS OF MONEY AND BA    
                                                                 
  -  7) Statistics                                               
        SELECT FROM: SIEO3600,4150  STAT1211                     
                                                                 
  - OR) Statistics                                               
         NEED:                  2 COURSES                        
        SELECT FROM: SIEO3658  STAT3659 OR 4107W                 
                                                                 
  - OR) Statistics                                               
        SELECT FROM: SIEO3658,3602                               
_________________________________________________________________
=================================================================
       COURSES NOT APPLIED ABOVE                                 
                                                                 
      SP11 ASCM 2118 V 07  0.0 NG   CONTMP ISLAMIC CIV - DISC    
      FA09 CHEM 1407 C 07  0.0 NG   GENERAL CHEMISTRY - REC      
                                    PROCESSED AS: CHEM2407C07CHEM
      SP10 ECON 1155 W 13  0.0 NG   PRINCIPLES OF ECON - DISC    
      FA10 IEOR 2261 E 01 3.00 A+   INTRO TO ACCOUNTING & FIN    
      SP10 MATH 1202 V 01 3.00 A+   CALCULUS IV                  
      FA10 MATH 1210 E 02 3.00 A+   ORDINRY DIFFERENTIAL EQUA    
      SP10 MATH 2010 V 02 3.00 A+   LINEAR ALGEBRA               
      FA13 PHED 1001 C 04 1.00 FT>+ PHYSICAL ED: BASKETBALL      
      FA10 STAT 3105 W 01 3.00 A+   INTRODUCTION TO PROBABILI    
      SP11 STAT 3107 W 01 3.00 A+   INTRO TO STATISTICAL INF     
_________________________________________________________________
=================================================================
NO     *****         SUMMARY OF DEGREE REQUIREMENTS         *****
       *** 128 POINTS NEEDED -- MINIMUM CUMULATIVE GPA OF 2.0 ***
                        *  *  *  *  *                            
          Earned points include Advanced Placement and           
                       Transfer points                           
                        *  *  *  *  *                            
    EARNED:106.50 Points                              4.240 GPA  
IN PROGRESS 17.00 Points                                         
    NEED:    4.50 Points                                         
                                                                 
        TRANSFER/ADVANCED PLACEMENT POINTS                       
        No more than 68 points may apply, unless you             
        are a dual-degree student.                               
                                                                 
           9.00 Points Earned                                    
      FA10 BIOL 2000 9 01 3.00 AP   BIOLOGY                      
      FA10 MATH 2000 9 01 6.00 AP   MATHEMATICS                  
                                                                 
  +     SUMMARY of courses which may be applied toward your      
        degree, listed by subject area.                          
                                                                 
          97.50 Points Earned                                    
IN-PROG>  17.00 Points                                           
      FA13 ASCE 2002 V 01 4.00 FT   INTRO MAJOR TOPICS: EAST     
      SP11 ASCM 2008 V 01 4.00 A    CONTEMP ISLAMIC CIVILIZAT    
      SP11 ASCM 2118 V 07  0.0 NG   CONTMP ISLAMIC CIV - DISC    
      FA09 CHEM 1403 C 02 3.50 A+   GENERAL CHEMISTRY I-LECTU    
      FA09 CHEM 1407 C 07  0.0 NG   GENERAL CHEMISTRY - REC      
                                    PROCESSED AS: CHEM2407C07CHEM
      SP13 COMS 0004 E 01  0.0 NG   APPLICATIONS-TRACK           
      FA13 COMS 0004 E 01 0.00 FT   APPLICATIONS-TRACK           
      FA10 COMS 1004 W 01 3.00 A+   INTRO-COMPUT SCI/PROGRAM     
      SP11 COMS 3134 W 01 3.00 A+   DATA STRUCTURES IN JAVA      
      FA12 COMS 3157 W 01 4.00 A+   ADVANCED PROGRAMMING         
      FA12 COMS 3203 W 01 3.00 A    INTRO-COMBINATORICS/GRAPH    
      SP13 COMS 3261 W 01 3.00 A+   COMPUTER SCIENCE THEORY      
      FA12 COMS 4111 W 02 3.00 A+   INTRODUCTION TO DATABASES    
      FA13 COMS 4115 W 01 3.00 FT   PROGRAMMING LANG & TRANSL    
      FA13 COMS 4167 W 01 3.00 FT   COMPUTER ANIMATION           
      FA12 COMS 4170 W 01 3.00 A+   USER INTERFACE DESIGN        
      SP13 COMS 4701 W 01 3.00 A    ARTIFICIAL INTELLIGENCE      
      FA13 COMS 4733 W 01 3.00 FT   COMPUTATNL ASPECTS OF ROB    
      FA12 CSEE 3827 W 01 3.00 A+   FUNDAMENTALS OF COMPUTER     
      SP13 CSEE 4119 W 01 3.00 A+   COMPUTER NETWORKS            
      SP10 ECON 1105 W 02 4.00 A+   PRINCIPLES OF ECONOMICS      
      SP10 ECON 1155 W 13  0.0 NG   PRINCIPLES OF ECON - DISC    
      SP13 ECON 2257 W 01 3.00 A    THE GLOBAL ECONOMY           
      SP11 ECON 3211 W 01 3.00 A+   INTERMEDIATE MICROECONOMI    
      FA10 ECON 3213 W 01 3.00 A+   INTERMEDIATE MACROECONOM     
      FA13 ECON 3265 V 01 3.00 FT   ECONOMICS OF MONEY AND BA    
      SP13 ECON 3412 W 02 3.00 A+   INTRODUCTION TO ECONOMETR    
      FA09 ENGI 1102 E 04 4.00 A    DES FUND USING ADV COMP      
      SP10 ENGL 1010 C 32 3.00 A-   UNIVERSITY WRITING           
      FA10 IEOR 2261 E 01 3.00 A+   INTRO TO ACCOUNTING & FIN    
      FA09 MATH 1201 V 06 3.00 A+   CALCULUS III                 
      SP10 MATH 1202 V 01 3.00 A+   CALCULUS IV                  
      FA10 MATH 1210 E 02 3.00 A+   ORDINRY DIFFERENTIAL EQUA    
      SP10 MATH 2010 V 02 3.00 A+   LINEAR ALGEBRA               
      FA09 MECE 1001 E 01 3.00 A+   MECH ENGI:MICROMACH/JUMBO    
      FA13 PHED 1001 C 04 1.00 FT>+ PHYSICAL ED: BASKETBALL      
      FA09 PHED 1001 C 24 1.00 P >+ PHYSICAL ED: BASKETBALL      
      FA10 PHED 1001 C 52 1.00 P >+ PHYSICAL ED: BASKETBALL      
      SP11 PHYS 1494 C 10 3.00 A    INTRO TO EXPERIMENTAL PHY    
      FA09 PHYS 1601 C 01 3.50 A+   PHYSICS I:MECHANICS/RELAT    
      SP10 PHYS 1602 C 01 3.50 A+   PHYSICS II: THERMO, ELEC     
      FA10 STAT 3105 W 01 3.00 A+   INTRODUCTION TO PROBABILI    
      SP11 STAT 3107 W 01 3.00 A+   INTRO TO STATISTICAL INF     
_________________________________________________________________
       COURSE CHECKS                                             
                                                                 
        DUPLICATE/RETAKEN COURSES                                
      FA13 PHED 1001 C 04 1.00 FT>+ PHYSICAL ED: BASKETBALL      
      FA09 PHED 1001 C 24 1.00 P >+ PHYSICAL ED: BASKETBALL      
      FA10 PHED 1001 C 52 1.00 P >+ PHYSICAL ED: BASKETBALL      
_________________________________________________________________
              ABBREVIATIONS USED IN THIS REPORT                  
              ---------------------------------                  
  OK-Requirement satisfied     NO-Requirement not satisfied      
  IP-In progress               FT-Future term                    
  + -Subset completed          - -Subset incomplete              
  SC-Satisfied course          AP-Advanced Placement             
  SR-Satisfied requirement     TI-Transfer credit from CU school 
  CP-Credit Pending            YC-Year long course               
  >+-Multi-term course         TR-Transfer credit from outside CU
  >R-Retaken course            UW-Unofficial withdrawal          
  >D-Duplicate class, credit reduced                             
  NG-No grade submitted by instructor                            
_________________________________________________________________
             END OF DARS REPORT FOR MAJORS/MINORS                
_________________________________________________________________
     >>>> ACADEMIC PROGRESS REPORT -- SEMESTER SUMMARY  <<<<<<   
       SEMESTER SUMMARY OF COURSES                               
    EARNED:106.50 Points                              4.240 GPA  
IN PROGRESS 17.00 Points                                         
                                                                 
        TRANSFER AND AP CREDIT                                   
           9.00 Points Earned                                    
      FA10 BIOL 2000 9 01 3.00 AP   BIOLOGY                      
      FA10 MATH 2000 9 01 6.00 AP   MATHEMATICS                  
                                                                 
        Fall 2009                                                
          18.00 Points Earned                     4.252 GPA      
      FA09 CHEM 1403 C 02 3.50 A+   GENERAL CHEMISTRY I-LECTU    
      FA09 CHEM 1407 C 07  0.0 NG   GENERAL CHEMISTRY - REC      
                                    PROCESSED AS: CHEM2407C07CHEM
      FA09 ENGI 1102 E 04 4.00 A    DES FUND USING ADV COMP      
      FA09 MATH 1201 V 06 3.00 A+   CALCULUS III                 
      FA09 MECE 1001 E 01 3.00 A+   MECH ENGI:MICROMACH/JUMBO    
      FA09 PHED 1001 C 24 1.00 P >+ PHYSICAL ED: BASKETBALL      
      FA09 PHYS 1601 C 01 3.50 A+   PHYSICS I:MECHANICS/RELAT    
                                                                 
        Spring 2010                                              
          16.50 Points Earned                     4.210 GPA      
      SP10 ECON 1105 W 02 4.00 A+   PRINCIPLES OF ECONOMICS      
      SP10 ECON 1155 W 13  0.0 NG   PRINCIPLES OF ECON - DISC    
      SP10 ENGL 1010 C 32 3.00 A-   UNIVERSITY WRITING           
      SP10 MATH 1202 V 01 3.00 A+   CALCULUS IV                  
      SP10 MATH 2010 V 02 3.00 A+   LINEAR ALGEBRA               
      SP10 PHYS 1602 C 01 3.50 A+   PHYSICS II: THERMO, ELEC     
                                                                 
        Fall 2010                                                
          16.00 Points Earned                     4.330 GPA      
      FA10 COMS 1004 W 01 3.00 A+   INTRO-COMPUT SCI/PROGRAM     
      FA10 ECON 3213 W 01 3.00 A+   INTERMEDIATE MACROECONOM     
      FA10 IEOR 2261 E 01 3.00 A+   INTRO TO ACCOUNTING & FIN    
      FA10 MATH 1210 E 02 3.00 A+   ORDINRY DIFFERENTIAL EQUA    
      FA10 PHED 1001 C 52 1.00 P >+ PHYSICAL ED: BASKETBALL      
      FA10 STAT 3105 W 01 3.00 A+   INTRODUCTION TO PROBABILI    
                                                                 
        Spring 2011                                              
          16.00 Points Earned                     4.185 GPA      
      SP11 ASCM 2008 V 01 4.00 A    CONTEMP ISLAMIC CIVILIZAT    
      SP11 ASCM 2118 V 07  0.0 NG   CONTMP ISLAMIC CIV - DISC    
      SP11 COMS 3134 W 01 3.00 A+   DATA STRUCTURES IN JAVA      
      SP11 ECON 3211 W 01 3.00 A+   INTERMEDIATE MICROECONOMI    
      SP11 PHYS 1494 C 10 3.00 A    INTRO TO EXPERIMENTAL PHY    
      SP11 STAT 3107 W 01 3.00 A+   INTRO TO STATISTICAL INF     
                                                                 
        Fall 2012                                                
          16.00 Points Earned                     4.268 GPA      
      FA12 COMS 3157 W 01 4.00 A+   ADVANCED PROGRAMMING         
      FA12 COMS 3203 W 01 3.00 A    INTRO-COMBINATORICS/GRAPH    
      FA12 COMS 4111 W 02 3.00 A+   INTRODUCTION TO DATABASES    
      FA12 COMS 4170 W 01 3.00 A+   USER INTERFACE DESIGN        
      FA12 CSEE 3827 W 01 3.00 A+   FUNDAMENTALS OF COMPUTER     
                                                                 
        Spring 2013                                              
          15.00 Points Earned                     4.198 GPA      
      SP13 COMS 0004 E 01  0.0 NG   APPLICATIONS-TRACK           
      SP13 COMS 3261 W 01 3.00 A+   COMPUTER SCIENCE THEORY      
      SP13 COMS 4701 W 01 3.00 A    ARTIFICIAL INTELLIGENCE      
      SP13 CSEE 4119 W 01 3.00 A+   COMPUTER NETWORKS            
      SP13 ECON 2257 W 01 3.00 A    THE GLOBAL ECONOMY           
      SP13 ECON 3412 W 02 3.00 A+   INTRODUCTION TO ECONOMETR    
                                                                 
        Fall 2013                                                
          17.00 Points Earned                                    
      FA13 ASCE 2002 V 01 4.00 FT   INTRO MAJOR TOPICS: EAST     
      FA13 COMS 0004 E 01 0.00 FT   APPLICATIONS-TRACK           
      FA13 COMS 4115 W 01 3.00 FT   PROGRAMMING LANG & TRANSL    
      FA13 COMS 4167 W 01 3.00 FT   COMPUTER ANIMATION           
      FA13 COMS 4733 W 01 3.00 FT   COMPUTATNL ASPECTS OF ROB    
      FA13 ECON 3265 V 01 3.00 FT   ECONOMICS OF MONEY AND BA    
      FA13 PHED 1001 C 04 1.00 FT>+ PHYSICAL ED: BASKETBALL      
_________________________________________________________________
****************       END OF DARS REPORT       **************** 
";
    }
}