using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuzzyLogic_Unit
{
    public class Range
    {
        private float start;
        private float end;

        public float Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }

        public float End
        {
            get
            {
                return end;
            }
            set
            {
                end = value;
            }
        }

        public Range()
        {
 
        }

        public Range(float from, float to)
        {
            start = from;
            end = to;
            if (check() != "")
            {
                start = 0;
                end = 0;
            }
        }

        public string check()
        {
            try
            {
                double n = Convert.ToDouble(Start);
                n = Convert.ToDouble(End);
                return "";
            }
            catch (Exception ex)
            {
                return "The range \"" + Start + "," + End + "\" is not a valid variable range.";
            }
        }
    }
}
