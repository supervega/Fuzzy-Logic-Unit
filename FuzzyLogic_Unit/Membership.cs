using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuzzyLogic_Unit
{
    public class Membership:Variable
    {
        private int mid;
        private string name;
        private float outValue;
        private List<int> parameters;

        public int MID
        {
            get
            {
                return mid;
            }
            set
            {
                mid = value;
            }
        }

        public string MembershipName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public float OutValue
        {
            get
            {
                return outValue;
            }
            set
            {
                outValue = value;
            }
        }

        public List<int> MembershipParameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        public Membership()
        {
            MembershipParameters = new List<int>();
        }

        public Membership(List<int> Data)
        {
            MembershipParameters = Data;
        }


    }
}
