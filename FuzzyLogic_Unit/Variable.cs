using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FuzzyLogic_Unit
{
  
    public class Variable
    {
        private int id;
        private string variableName;
        private Range variableRange;
        private VarType variabletype;
        public enum VarType {Input,Output,Input_Output}
        private List<Membership> membershipfunctions;
        private Graphics g;
        private Pen Drawingpen;
        private Pen WritingPen;
        private int Fuzzy1 = 100;
        private int Fuzzy0 = 300;
        private int perfectvalue;
        private int scale;
        private int Centroid_StepSize;
        private Pen CrispPen;
        private float crisp;
        private float crispoutput;

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string VariableName
        {
            get
            {
                return variableName;
            }
            set
            {
                variableName = value;
            }
        }

        public Range VariableRange
        {
            get
            {
                return variableRange;
            }
            set
            {
                variableRange = value;
            }
        }

        public VarType VariableType
        {
            get
            {
                return variabletype;
            }
            set
            {
                variabletype = value;
            }
        }

        public List<Membership> MemberShipFunctions
        {
            get
            {
                return membershipfunctions;
            }
            set
            {
                membershipfunctions = value;
            }
        }

        public Graphics G
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
            }
        }

        public Pen DrawingPen
        {
            get
            {
                return Drawingpen;
            }
            set
            {
                Drawingpen = value;
            }
        }

        public int Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public int Fuzzy_0
        {
            get
            {
                return Fuzzy0;
            }
            set
            {
                Fuzzy0 = value;
            }
        }

        public int Fuzzy_1
        {
            get
            {
                return Fuzzy1;
            }
            set
            {
                Fuzzy1 = value;
            }
        }

        public int Perfect_Value
        {
            get
            {
                return perfectvalue;
            }
            set
            {
                perfectvalue = value;
            }
        }

        public int CSS
        {
            get
            {
                return Centroid_StepSize;
            }
            set
            {
                Centroid_StepSize = value;
            }
        }

        public float Crisp
        {
            get
            {
                return crisp;
            }
            set
            {
                crisp = value;
            }
        }

        public float CrispOutput
        {
            get            
            {
                return crispoutput;
            }
            set
            {
                crispoutput = value;
            }
        }

        public Variable() 
        {
            ID = Core.VariableCount;
            VariableName = "var"+ID;
            VariableType = VarType.Input;
            MemberShipFunctions = new List<Membership>();
            DrawingPen = new Pen(Color.Black);
            WritingPen = new Pen(Color.Blue, 2);
            CrispPen = new Pen(Color.Red);
            crisp = float.MinValue;
        } 

        public Variable(float from,float to)
        {
            ID = Core.VariableCount;
            VariableName = "var" + ID;
            VariableType = VarType.Input;
            VariableRange.Start = from;
            VariableRange.End = to;
            MemberShipFunctions = new List<Membership>();
            DrawingPen = new Pen(Color.Black);
            CrispPen = new Pen(Color.Red);
            crisp = float.MinValue;
        }

        public void Draw()
        {
            try
            {
                DrawCrisp();
                int MinValue = Convert.ToInt32(MemberShipFunctions[0].MembershipParameters[0]) * -1;
                if (MinValue < 0)
                    MinValue = 0;
                G.DrawLine(DrawingPen, new Point((Perfect_Value + MinValue) * scale, Fuzzy0), new Point((Perfect_Value + MinValue) * scale, Fuzzy1 - 30));
                G.DrawLine(DrawingPen, new Point((Perfect_Value + MinValue) * scale, Fuzzy1 - 30), new Point((Perfect_Value + MinValue) * scale + 5, Fuzzy1 - 20));
                G.DrawLine(DrawingPen, new Point((Perfect_Value + MinValue) * scale, Fuzzy1 - 30), new Point((Perfect_Value + MinValue) * scale - 5, Fuzzy1 - 20));
                G.DrawLine(DrawingPen, new Point(0, Fuzzy0), new Point((int)G.VisibleClipBounds.Width, Fuzzy0));
                G.DrawString(Perfect_Value.ToString(), new Font(FontFamily.GenericSerif, 12, FontStyle.Bold), WritingPen.Brush, new PointF((Perfect_Value + MinValue) * Scale - 5, Fuzzy0 + 22));
                G.DrawString(VariableName, new Font(FontFamily.GenericSerif, 16, FontStyle.Bold), WritingPen.Brush, new PointF((G.VisibleClipBounds.Width / 2) - 20, Fuzzy0 + 35));
                foreach (Membership MEM in MemberShipFunctions)
                {
                    G.DrawString(MEM.MembershipParameters[1].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[1] + MinValue) * Scale - 5, Fuzzy0 + 10));
                    G.DrawString(MEM.MembershipParameters[2].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[2] + MinValue) * Scale - 5, Fuzzy0 + 10));

                    if (MEM.MembershipParameters.Count == 3)
                    {
                        if (MEM.MembershipParameters[0] <= VariableRange.Start)
                        {
                            G.DrawString(MEM.MembershipParameters[0].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[0] + MinValue) * Scale, Fuzzy0 + 10));
                            G.DrawLines(DrawingPen, new Point[] { new Point((MEM.MembershipParameters[0] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[1] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[2] + MinValue) * scale, Fuzzy0) });
                            G.DrawString(MEM.MembershipName, new Font(FontFamily.GenericSerif, 12, FontStyle.Bold), WritingPen.Brush, new PointF(2, (Fuzzy0 - Fuzzy1) / 2 + Fuzzy1));
                        }
                        else
                            if (MEM.MembershipParameters[2] >= VariableRange.End)
                            {
                                G.DrawString(MEM.MembershipParameters[0].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[0] + MinValue) * Scale - 5, Fuzzy0 + 10));
                                G.DrawLines(DrawingPen, new Point[] { new Point((MEM.MembershipParameters[0] + MinValue) * scale, Fuzzy0), new Point((MEM.MembershipParameters[1] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[2] + MinValue) * scale, Fuzzy1) });
                                G.DrawString(MEM.MembershipName, new Font(FontFamily.GenericSerif, 12, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[1] + MinValue) * scale, (Fuzzy0 - Fuzzy1) / 2 + Fuzzy1));
                            }
                            else
                            {
                                G.DrawString(MEM.MembershipParameters[0].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[0] + MinValue) * Scale - 5, Fuzzy0 + 10));
                                G.DrawLines(DrawingPen, new Point[] { new Point((MEM.MembershipParameters[0] + MinValue) * scale, Fuzzy0), new Point((MEM.MembershipParameters[1] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[2] + MinValue) * scale, Fuzzy0) });
                                G.DrawString(MEM.MembershipName, new Font(FontFamily.GenericSerif, 12, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[1] + MinValue) * scale, (Fuzzy0 - Fuzzy1) / 2 + Fuzzy1));
                            }
                    }
                    if (MEM.MembershipParameters.Count == 4)
                    {
                        G.DrawString(MEM.MembershipParameters[0].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[0] + MinValue) * Scale - 5, Fuzzy0 + 10));
                        G.DrawString(MEM.MembershipParameters[3].ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[3] + MinValue) * Scale - 5, Fuzzy0 + 10));
                        G.DrawLines(DrawingPen, new Point[] { new Point((MEM.MembershipParameters[0] + MinValue) * scale, Fuzzy0), new Point((MEM.MembershipParameters[1] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[2] + MinValue) * scale, Fuzzy1), new Point((MEM.MembershipParameters[3] + MinValue) * scale, Fuzzy0) });
                        G.DrawString(MEM.MembershipName, new Font(FontFamily.GenericSerif, 12, FontStyle.Bold), WritingPen.Brush, new PointF((MEM.MembershipParameters[1] + MinValue) * scale, (Fuzzy0 - Fuzzy1) / 2 + Fuzzy1));
                    }
                }
            }
            catch (Exception ex)
            {
 
            }
        }

        public void DrawCrisp()
        {
            if (VariableType==Variable.VarType.Input && crisp != float.MinValue)
            {
                G.DrawString(crisp.ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), CrispPen.Brush, new PointF((int)crisp * scale, Fuzzy1 - 45));
                G.DrawLine(CrispPen, new Point((int)crisp * scale, Fuzzy0), new Point((int)crisp * scale, Fuzzy1 - 30));
                //crisp = float.MinValue;
            }
            if (VariableType == Variable.VarType.Output)
            {
                G.DrawString(CrispOutput.ToString(), new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), CrispPen.Brush, new PointF((int)CrispOutput * scale, Fuzzy1 - 45));
                G.DrawLine(CrispPen, new Point((int)CrispOutput * scale, Fuzzy0), new Point((int)CrispOutput * scale, Fuzzy1 - 30));
                //crisp = float.MinValue;
            }
        }

    }
}
