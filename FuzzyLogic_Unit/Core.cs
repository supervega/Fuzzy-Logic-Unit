using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ES_Lib;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections;

namespace FuzzyLogic_Unit
{
    public class Core
    {
        public Project Main;
        public static List<Variable> variables;
        public DataGridView ErrorDGV;
        string Codepath,variablesPath;
        string[] Data;        
        public PictureBox VariablesPB;
        private Thread DrawingThread;
        private bool Draw = false;
        private Variable CurrentVariable;
        private Dictionary<FuzzyNumber, string> AntecedentVars = new Dictionary<FuzzyNumber, string>();
        ArrayList IntersectedMEmbership;
        public Graphics G;
        private Pen DrawingPen;
        public ListBox LBFiredRules;
        public delegate void AddRule(string MSG);
        public delegate void ClearRules();
        public int LineIndex = 0;
        Node Temp;
        public ArrayList System_Rules;
        public ArrayList System_Facts;
        public int RuleCount = 0;
        bool postError = false;
        public int DrawingIndex = 0;

        struct FuzzyNumber
        {
            public Membership MEM;
            public float MembershipDegree;
        }

        public bool DrawFlag
        {
            get
            {
                return Draw;
            }
            set
            {
                Draw = value;
            }
        }

        public Core(string FilesPath)
        {
            InitilizeCore(FilesPath);
        }

        bool CompilationDelay = true;

        public void Clear()
        {
            IntersectedMEmbership = new ArrayList();
            variables = new List<Variable>();
            ErrorDGV = new DataGridView();
            System_Facts = new ArrayList();
            LoadVariablesData();
        }

        public void InitilizeCore(string FilesPath)
        {
            variablesPath = FilesPath + "\\EXP.xml";
            Clear();
            Main = new Project("Car Expert unit");
            Main.InitialPath = FilesPath;
            Main.InitilizeEngine();
            Main.ErrorExplorerRef = ErrorDGV;
            DrawingPen = new Pen(Color.White);
            Codepath = FilesPath;            
            LoadCode();
            if (!CompilationDelay)
            {
                CompileCode();
                CompilationDelay = false;
            }
            else
            {
                System_Rules = new ArrayList();
                System_Facts = new ArrayList();
            }
            DrawingThread = new Thread(new ThreadStart(DrawThread));
            DrawingThread.Start();
        }

        public void Dispose()
        {
            if (DrawingThread.IsAlive)
            {
                DrawFlag = false;
                DrawingThread.Abort();
            }
        }
       

        #region CompilationRegion

        private void LoadCode()
        {
            Data = System.IO.File.ReadAllLines(Codepath+"\\Code.dat");
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].Length > 2)// && Data[i][0] == '>' && Data[i][1] == '>'
                    Data[i] = Data[i].Substring(0, Data[i].Length);
                else
                    Data[i] = "";
            }           
        }
                

        public void CompileCode()
        {
            Node Root = new Node();
            Root.NodeKind = "Main";
            Root.isInitial = true;
            Root.Value = ".";
            Main.ResultText = "";
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] != "" && Data[i] != ">>")
                {
                    Main.GET_AST(Data[i], i + 1);
                    Temp = new Node();
                    Root.NodeKind = "Main";
                    Temp.nextstates.Add(Main.ParseRoot);
                    Main.RUN(Temp, 0);
                    Root.nextstates.Add(Main.ParseRoot);
                    //Main.DoChaining();
                }
            }
            RuleCount = Main.RuleCount;
            System_Rules = new ArrayList();
            System_Facts = new ArrayList();
            foreach (Rule R in Main.Rules)
            {
                System_Rules.Add(R);
            }
            foreach (Fact F in Main.Facts)
            {
                System_Facts.Add(F);
            }
            Main.ParseRoot = Root;
            LineIndex= Main.TExtRef.Text.Split('\n').Length;
            Main.TExtRef.Text += "\n>>";
            Main.TExtRef.Select(Main.TExtRef.Text.Length, Main.TExtRef.Text.Length);
        }

        public void DrawAST()
        {
            Main.DrawAST();
        }

        #endregion

        # region VariableCode

        public static int VariableCount
        {
            get
            {
                return variables.Count;
            }
        }

        public int VariablesCount
        {
            get
            {
                return variables.Count;
            }
        }

        public void LoadVariablesData()
        {
            XPathDocument doc;
            XPathNavigator nav;
            XPathExpression expr;
            XPathNodeIterator iterator;

            FileStream stream =new FileStream(variablesPath,FileMode.Open,FileAccess.Read);
            doc = new XPathDocument(stream);
            nav = doc.CreateNavigator();

            // Compile a standard XPath expression
            variables.Clear();
            expr = nav.Compile("root/variables/variable");
            iterator = nav.Select(expr);
            try
            {
				while (iterator.MoveNext())
				{
                    Variable V = new Variable();
					XPathNavigator nav2 = iterator.Current.Clone();
                    nav2.MoveToFirstChild();                    
					V.ID= Convert.ToInt32(nav2.Value);
                    nav2.MoveToNext();
                    V.VariableName = nav2.Value;
                    nav2.MoveToNext();
                    nav2.MoveToFirstChild();
                    V.VariableRange = new Range();
                    V.VariableRange.Start = Convert.ToInt32(nav2.Value);
                    nav2.MoveToNext();
                    V.VariableRange.End = Convert.ToInt32(nav2.Value);
                    nav2.MoveToParent();
                    nav2.MoveToNext();
                    V.Perfect_Value = Convert.ToInt32(nav2.Value);
                    nav2.MoveToNext();
                    string type = nav2.Value;
                    if (type.ToLower() == "input")
                        V.VariableType = Variable.VarType.Input;
                    if (type.ToLower() == "output")
                        V.VariableType = Variable.VarType.Output;
                    if (type.ToLower() == "input_output")
                        V.VariableType = Variable.VarType.Input_Output;
                    nav2.MoveToNext();
                    V.CSS = Convert.ToInt32(nav2.Value);
                    while (nav2.MoveToNext() && nav2.Name == "membership")
                    {
                        Membership M = new Membership();
                        nav2.MoveToFirstChild();
                        M.MID = Convert.ToInt32(nav2.Value);
                        nav2.MoveToNext();
                        M.MembershipName = nav2.Value;
                        nav2.MoveToNext();                        
                        int Counter = 1;
                        nav2.MoveToFirstChild();
                        while (nav2.Name == "param" + Counter)
                        {                            
                            M.MembershipParameters.Add(Convert.ToInt32(nav2.Value));
                            Counter++;
                            nav2.MoveToNext(); 
                        }
                        V.MemberShipFunctions.Add(M);
                        nav2.MoveToParent();
                        nav2.MoveToParent();                       
                    }
                    variables.Add(V);
				}
			}
			catch(Exception ex) 
			{
				//implement exception handling
			} 
        }

        public string AddVariable(Variable var)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(variablesPath);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode currNode;

                XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
                string COntent = "<variable>"+
                "<id>"+(VariableCount+1)+"</id>"+
	            "<name>"+var.VariableName+"</name>"+
	            "<range>"+
		            "<from>"+var.VariableRange.Start+"</from>"+
		            "<to>"+var.VariableRange.End+"</to>"+
	            "</range>"+
	            "<perfect_value>"+var.Perfect_Value+"</perfect_value>"+
	            "<type>"+var.VariableType.ToString()+"</type>"+
	            "<Centroid_StepSize>"+var.CSS+"</Centroid_StepSize>";
                foreach (Membership Mem in var.MemberShipFunctions)
	            {
                    COntent += "<membership>" +
                    "<MID>" + Mem.ID + "</MID>" +
                    "<Mname>" + Mem.MembershipName + "</Mname><parameters>";
                    for (int i = 0; i < Mem.MembershipParameters.Count; i++)
			        {
                        COntent += "<param"+(i+1)+">"+Mem.MembershipParameters[i]+"</param"+(i+1)+">";
			        }	            
	                COntent += "</parameters></membership>";
	            }
	            
	            COntent += "</variable>";
                docFrag.InnerXml = COntent;
                currNode = doc.GetElementsByTagName("variables")[0];
                currNode.InsertAfter(docFrag, currNode.LastChild);
                doc.Save(variablesPath);
                return "";
            }
            catch (Exception ex)
            {
                return "Couldn't add variable.";
            } 
        }

        public string Modify_Variable(Variable var)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(variablesPath);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();

                XmlNode oldCd;
                XmlElement root = doc.DocumentElement;
                oldCd = root.SelectSingleNode("/root/variables/variable[id='" + var.ID + "']");
                root.FirstChild.RemoveChild(oldCd);

                XmlNode newCd = doc.CreateNode(XmlNodeType.Element, "variable", "");

                string COntent = "<id>" + var.ID + "</id>" +
                 "<name>" + var.VariableName + "</name>" +
                 "<range>" +
                     "<from>" + var.VariableRange.Start + "</from>" +
                     "<to>" + var.VariableRange.End + "</to>" +
                 "</range>" +
                 "<perfect_value>" + var.Perfect_Value + "</perfect_value>" +
                 "<type>" + var.VariableType.ToString() + "</type>" +
                 "<Centroid_StepSize>" + var.CSS + "</Centroid_StepSize>";
                foreach (Membership Mem in var.MemberShipFunctions)
                {
                    COntent += "<membership>" +
                    "<MID>" + Mem.MID + "</MID>" +
                    "<Mname>" + Mem.MembershipName + "</Mname><parameters>";
                    for (int i = 0; i < Mem.MembershipParameters.Count; i++)
                    {
                        COntent += "<param" + (i + 1) + ">" + Mem.MembershipParameters[i] + "</param" + (i + 1) + ">";
                    }
                    COntent += "</parameters></membership>";
                }
               
                newCd.InnerXml = COntent;

                root.FirstChild.AppendChild(newCd);
                doc.Save(variablesPath);
                return "";
            }
            catch (Exception ex)
            {
                return "Couldn't modify variable.";
            }
        }

        public string DeleteVariable(int VarID)// Phisycally
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(variablesPath);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();

                XmlNode oldCd;
                XmlElement root = doc.DocumentElement;
                oldCd = root.SelectSingleNode("/root/variables/variable[id='" + VarID + "']");
                root.FirstChild.RemoveChild(oldCd);
                doc.Save(variablesPath);
                return "";
            }
            catch (Exception ex)
            {
                return "Couldn't Delete variable.";
            }
        }

        public void CreateVariable(string VariableName,float from,float to)
        {
            Variable var = new Variable();
            var.VariableName = VariableName;
            var.VariableRange.Start = from;
            var.VariableRange.End = to;
            variables.Add(var);
        }

        public void DeleteVaraible(int ID)
        {
            foreach (Variable v in variables)
            {
                if (v.ID == ID)
                {
                    variables.Remove(v);
                    break;
                }
            }
        }

        public Variable GetVariable(int ID)
        {
            foreach (Variable v in variables)
            {
                if (v.ID == ID)
                    return v;
            }
            return null;
        }

        public void DrawVariable(string variableName,bool Trace)
        {
            foreach (Variable V in variables)
            {
                if (V.VariableName == variableName)
                {
                    V.G = VariablesPB.CreateGraphics();
                    V.Scale = VariablesPB.Width / (int)(V.VariableRange.End - V.VariableRange.Start);
                    V.Fuzzy_1 = VariablesPB.Height / 4;
                    V.Fuzzy_0 = V.Fuzzy_1 * 3;
                    CurrentVariable = V;
                    if (V.Scale == 0)
                        V.Scale = 1;
                    if(!Trace)
                        DrawFlag = true;
                    break;
                }
            }
        }

        #endregion  
      
        private void DrawThread()
        {
            while (true)
            {
                if (DrawFlag)
                {
                    if (CurrentVariable != null)
                    {
                        try
                        {
                            CurrentVariable.G.Clear(Color.White);
                            CurrentVariable.Draw();
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                Thread.Sleep(50);
            }
        }

        #region DrawInference Tracing

        public void InitilizeInferenceDrawing()
        {
            DrawFlag = false;
            DrawVariable(variables[DrawingIndex].VariableName, false);
        }
             

        

        public void Run_Distance_acceleration(float[] CrispInput,bool Draw)
        {
            ArrayList Temp = new ArrayList();
            Temp.Add("fdistance");
            Temp.Add("rdistance");
            Temp.Add("ldistance");
            Temp.Add("fdelta");
            Temp.Add("rdelta");
            Temp.Add("ldelta");

            ArrayList FactsValues = new ArrayList();
            foreach (Variable V in variables)
            {
                if (!Temp.Contains(V.VariableName.ToLower().Trim()))
                {
                    Temp.Add(V.VariableName);
                    foreach (Fact F in Main.Facts)
                    {
                        if (F.Attributes[0].ToString().ToLower() == V.VariableName.ToLower())
                        {
                            FactsValues.Add(F.DegreeOfBelieve);
                            break;
                        }
                    }
                }
            }
            float[] Inp = new float[CrispInput.Length + FactsValues.Count];
            for (int i = 0; i < Inp.Length; i++)
            {
                if (i < CrispInput.Length)
                    Inp[i] = CrispInput[i];
                else
                    Inp[i] = (float)FactsValues[i - CrispInput.Length];
            }
                       
            if (Draw)
            {
                //CurrentVariable = null;
                DrawFlag = true;
            }
            Fuzzification_Inference_DeFuzzification(Inp,Temp,Draw);
        }


        ArrayList TempMembershipNames = new ArrayList();
        public void Run_Item_Profiling(float[] Input, bool Draw)
        {
            ArrayList Temp = new ArrayList();
            TempMembershipNames = new ArrayList();
            Temp.Add("item");

            ArrayList FactsValues = new ArrayList();
            foreach (Variable V in variables)
            {
                if (!Temp.Contains(V.VariableName))
                {
                    Temp.Add(V.VariableName);
                    foreach (Fact F in Main.Facts)
                    {
                        if (F.Attributes[0].ToString().ToLower() == V.VariableName.ToLower())
                        {
                            TempMembershipNames.Add(F.Attributes[2].ToString().ToLower().Trim());
                            FactsValues.Add(F.DegreeOfBelieve);
                            break;
                        }
                    }
                }
            }
            float[] Inp = new float[Input.Length + FactsValues.Count];
            for (int i = 0; i < Inp.Length; i++)
            {
                if (i < Input.Length)
                    Inp[i] = Input[i];
                else
                    Inp[i] = (float)FactsValues[i - Input.Length];
            }

            if (Draw)
            {
                //CurrentVariable = null;
                DrawFlag = true;
            }
            Fuzzification_Inference_DeFuzzification(Inp, Temp, Draw);

        }

        public void GetOffer(bool Draw)
        {
            ArrayList Temp = new ArrayList();
            TempMembershipNames = new ArrayList();
            Temp.Add("gender");
            Temp.Add("age");
            Temp.Add("individual");
            Temp.Add("financial");
            Temp.Add("status");

            ArrayList FactsValues = new ArrayList();
            foreach (Variable V in variables)
            {
                foreach (Fact F in Main.Facts)
                {
                    //TempMembershipNames.Add(F.Attributes[2]);
                    if (F.Attributes[0].ToString().ToLower().Trim().ToString() == V.VariableName.ToLower().Trim())
                    {
                        foreach (Membership MEM in V.MemberShipFunctions)
                        {
                            if(F.Attributes[2].ToString().ToLower().Trim()==MEM.MembershipName.ToLower().Trim())
                                FactsValues.Add(((MEM.MembershipParameters[1]-MEM.MembershipParameters[0])*F.DegreeOfBelieve)+MEM.MembershipParameters[0]);
                        }
                    }                   
                }                
            }
            
            float[] Inp = new float[FactsValues.Count];
            FactsValues.CopyTo(Inp);
            
            if (Draw)
            {
                //CurrentVariable = null;
                DrawFlag = true;
            }
            Main.Facts = new ArrayList();
            Fuzzification_Inference_DeFuzzification(Inp, Temp, Draw);

        }

        
        private void Fuzzification_Inference_DeFuzzification(float[] CrispInput,ArrayList variableNames,bool DrawTrace)
        {
            ClearResult();
            if (DrawTrace)
                G.Clear(Color.Black);
            for (int p = 0; p < variables.Count; p++)
            {
                Variable V = (Variable)variables[p];               
                IntersectedMEmbership.Clear();
                if (variableNames.Contains(V.VariableName.ToLower().Trim()) && (V.VariableType == Variable.VarType.Input_Output || V.VariableType == Variable.VarType.Input))
                {
                    variables.Remove(V);
                    int Counter = 0;
                    
                    foreach (Membership MEM in V.MemberShipFunctions)
                    {
                        FuzzyNumber FN = new FuzzyNumber();
                        FN.MEM = MEM;
                        int VIndex = variableNames.IndexOf(V.VariableName.ToLower().Trim());
                        if (VIndex >= CrispInput.Length || VIndex<0)
                        {
                            break;
                        }          
                        float CurrentCrisp = CrispInput[VIndex];
                        if (CurrentCrisp > 0 && CurrentCrisp < 1)
                        {
                            if (TempMembershipNames.Contains(MEM.MembershipName.ToLower().Trim()))
                            {
                                FN.MembershipDegree = CurrentCrisp;
                                IntersectedMEmbership.Add(FN);
                                break;
                            }
                            else
                                continue;
                        }
                        int J = 2;
                        if (MEM.MembershipParameters.Count == 4)
                            J = 3;
                        if (CurrentCrisp >= MEM.MembershipParameters[0] && CurrentCrisp < MEM.MembershipParameters[J])
                        {
                            if (J == 2 && MEM.MembershipParameters[0] == V.VariableRange.Start)
                            {
                                if (CurrentCrisp >= MEM.MembershipParameters[1] && CurrentCrisp < MEM.MembershipParameters[2])
                                    FN.MembershipDegree = (CurrentCrisp - MEM.MembershipParameters[0]) / (MEM.MembershipParameters[1] - MEM.MembershipParameters[0]);
                                else
                                    FN.MembershipDegree = 1;
                            }
                            else
                                if (J == 2 && MEM.MembershipParameters[2] == V.VariableRange.End)
                                {
                                    if (CurrentCrisp >= MEM.MembershipParameters[0] && CurrentCrisp < MEM.MembershipParameters[1])
                                        FN.MembershipDegree = (CurrentCrisp - MEM.MembershipParameters[0]) / (MEM.MembershipParameters[1] - MEM.MembershipParameters[0]);
                                    else
                                        FN.MembershipDegree = 1;
                                }
                                else
                                    if (J == 2 )
                                    {
                                        FN.MembershipDegree = (CurrentCrisp - MEM.MembershipParameters[0]) / (MEM.MembershipParameters[1] - MEM.MembershipParameters[0]);                                       
                                    }
                                        else
                                        {
                                            if (CurrentCrisp >= MEM.MembershipParameters[0] && CurrentCrisp < MEM.MembershipParameters[1])
                                                FN.MembershipDegree = (CurrentCrisp - MEM.MembershipParameters[0]) / (MEM.MembershipParameters[1] - MEM.MembershipParameters[0]);
                                            if (CurrentCrisp >= MEM.MembershipParameters[1] && CurrentCrisp <= MEM.MembershipParameters[2])
                                                FN.MembershipDegree = 1;
                                            if (CurrentCrisp > MEM.MembershipParameters[2] && CurrentCrisp <= MEM.MembershipParameters[3])
                                                FN.MembershipDegree = 1-((CurrentCrisp - MEM.MembershipParameters[2]) / (MEM.MembershipParameters[3] - MEM.MembershipParameters[2]));
                                        }

                           IntersectedMEmbership.Add(FN);
                           V.Crisp = CurrentCrisp;
                           //CurrentVariable = V;
                           if (DrawTrace && V.VariableName == CurrentVariable.VariableName)
                           {
                               G.DrawString(MEM.MembershipName + " with a Degree of : " + FN.MembershipDegree, new Font(FontFamily.GenericSerif, 14, FontStyle.Bold), DrawingPen.Brush, new PointF(80, 90 + Counter * 40));
                               Counter++;
                           }                          
                           //break;
                         }                        
                    }
                    for (int i = 0; i < IntersectedMEmbership.Count; i++)
                    {                      
                        AntecedentVars.Add((FuzzyNumber)IntersectedMEmbership[i],V.VariableName);
                    }
                    variables.Insert(p, V); 
                }
            }
            
            if (DrawTrace)
            {
                FirstTime = true;
            }

          
            Do_Chaining(DrawTrace);

            CaluculateCentroid();
            
            if (DrawTrace)
            {
                int Y = 0;
                foreach (Variable V in variables)
                {
                    if (V.VariableType == Variable.VarType.Output)
                    {
                        G.DrawString(V.VariableName + " : " + V.CrispOutput, new Font(FontFamily.GenericSerif, 20, FontStyle.Bold), new Pen(Color.Red, 7).Brush, new PointF(130, 350 + Y));
                        Y += 20;
                    }
                }               
            }            
            //System.Windows.Forms.MessageBox.Show(Result.ToString());
        }

        public void Do_Chaining(bool DrawTrace)        
        {
            Random RandomGenerator = new Random(12345);

            if (DrawTrace)
            {
                ClearRules CR = new ClearRules(ClearR);
                CR.Invoke();
            }
            for (int i = 0; i < Main.Rules.Count; i++)
            {
                
                Rule R=(Rule)Main.Rules[i];
                float Ant1 = 0, Ant2 = 0, Ant3 = 0;
                bool[] Marked = new bool[AntecedentVars.Count];
                for (int t = 0; t < Marked.Length; t++)
                {
                    Marked[t] = false;
                }
                Dictionary<string, string> Tem = R.GetAntecedents(); 
                for (int j=0;j<AntecedentVars.Count;j++)
                {
                    KeyValuePair<FuzzyNumber, string> Data = AntecedentVars.ElementAt<KeyValuePair<FuzzyNumber, string>>(j);                                       
                    foreach(KeyValuePair<string, string> Antecedent in Tem)
                    {
                        
                        #region None Type

                        if (R.RuleType == Rule.OperationType.NONE && Antecedent.Key.ToLower().Trim() == Data.Value.ToLower().Trim() && Antecedent.Value.ToLower().Trim() == ((FuzzyNumber)Data.Key).MEM.MembershipName.ToLower().Trim())
                        {
                            R.FuzzyResult = ((FuzzyNumber)Data.Key).MembershipDegree * R.CertaintyFactor;                           
                            FuzzyNumber FN = new FuzzyNumber();
                            string varname = "";
                            string[] RTemp = R.THENPART.Split(new string[] { " is " }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (Variable varr in variables)
                            {
                                foreach (Membership M in varr.MemberShipFunctions)
                                {
                                    if (RTemp.Length == 2 && M.MembershipName.ToLower().Trim() == RTemp[1].ToLower().Trim() && varr.VariableName.ToLower().Trim() == RTemp[0].ToLower().Trim())
                                    {
                                        FN.MEM = M;
                                        FN.MembershipDegree = R.FuzzyResult;
                                        varname = varr.VariableName;
                                        break;
                                    }
                                    else
                                        if (!postError && RTemp.Length != 2)
                                        {
                                            Main.TExtRef.Invoke(new EventHandler(delegate
                                            {
                                                Main.TExtRef.Text += "Rule synatx error in : \" " + R.Data + " \"\n>>";
                                            }));
                                            postError = true;
                                            return;
                                        }                                                                            
                                }
                            }
                            if (FN.MEM!=null && !AntecedentVars.ContainsKey(FN))
                            {
                                AntecedentVars.Add(FN, varname);
                                Fact F = new Fact(R.THENPART);
                                F.ProcessFact();
                                F.DegreeOfBelieve = R.FuzzyResult;
                                bool addF = true;
                                for (int k = 0; k < Main.Facts.Count; k++)
                                {
                                    Fact OldF = (Fact)Main.Facts[k];
                                    if (OldF.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim() == F.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim())
                                    {
                                        Main.Facts.Remove(OldF);
                                        F.Index = RandomGenerator.Next(0, 999);
                                        Main.Facts.Add(F);
                                        addF = false;
                                        break;
                                    }
                                }
                                if (addF)
                                    Main.Facts.Add(F);
                                j = AntecedentVars.Count;
                                //i = 0;
                                if (DrawTrace)
                                {
                                    ClearRules CR = new ClearRules(ClearR);
                                    CR.Invoke();
                                }
                                
                            }                       
                            
                            if (DrawTrace)
                            {
                                AddRule AR = new AddRule(DrawNewRule);
                                AR.Invoke(R.Data);
                            }
                            break;
                        }
                        #endregion

                        #region OR

                        else
                            if (R.RuleType == Rule.OperationType.OR && Antecedent.Key.ToLower().Trim() == Data.Value.ToLower().Trim() && Antecedent.Value.ToLower().Trim() == ((FuzzyNumber)Data.Key).MEM.MembershipName.ToLower().Trim())
                            {
                                if (Ant1 == 0)
                                    Ant1 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                if (Ant2 == 0)
                                    Ant2 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                else
                                {
                                    R.FuzzyResult = Math.Max(Ant1, Ant2) * R.CertaintyFactor;
                                    FuzzyNumber FN = new FuzzyNumber();
                                    string varname = "";
                                    string[] RTemp = R.THENPART.Split(new string[] { " is " }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (Variable varr in variables)
                                    {
                                        foreach (Membership M in varr.MemberShipFunctions)
                                        {
                                            if (RTemp.Length == 2 && M.MembershipName.ToLower().Trim() == RTemp[1].ToLower().Trim() && varr.VariableName.ToLower().Trim() == RTemp[0].ToLower().Trim())
                                            {
                                                FN.MEM = M;
                                                FN.MembershipDegree = R.FuzzyResult;
                                                varname = varr.VariableName;
                                                break;
                                            }
                                            else
                                                if (!postError && RTemp.Length != 2)
                                            {
                                                Main.TExtRef.Invoke(new EventHandler(delegate
                                                {
                                                    Main.TExtRef.Text += "Rule synatx error in : \" " + R.Data + " \"\n>>";
                                                }));
                                                postError = true;
                                                return;
                                            }
                                        }
                                    }
                                    if (FN.MEM != null && !AntecedentVars.ContainsKey(FN))
                                    {
                                        AntecedentVars.Add(FN, varname);
                                        Fact F = new Fact(R.THENPART);
                                        F.ProcessFact();
                                        F.Index = RandomGenerator.Next(0, 999);
                                        F.DegreeOfBelieve = R.FuzzyResult;
                                        bool addF = true;
                                        for (int k=0;k<Main.Facts.Count;k++)
                                        {
                                            Fact OldF=(Fact)Main.Facts[k];
                                            if (OldF.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim() == F.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim())
                                            {
                                                Main.Facts.Remove(OldF);
                                                Main.Facts.Add(F);
                                                addF=false;
                                                break;
                                            }
                                        }
                                        if (addF)
                                            Main.Facts.Add(F);
                                        j = AntecedentVars.Count;
                                        i = 0;
                                        if (DrawTrace)
                                        {
                                            AddRule AR = new AddRule(DrawNewRule);
                                            AR.Invoke(R.Data);
                                        }
                                        break;
                                    }   
                                }
                            }
                            else
                        #endregion

                                #region AND


                        if (R.RuleType == Rule.OperationType.AND && Antecedent.Key.ToLower().Trim() == Data.Value.ToLower().Trim() && Antecedent.Value.ToLower().Trim() == ((FuzzyNumber)Data.Key).MEM.MembershipName.ToLower().Trim() && !Marked[j])
                        {
                            if (Ant1 == 0)
                            {
                                Ant1 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                Marked[j] = true;
                                j = -1;
                                break;

                                //continue;
                            }
                            if (Tem.Count == 3 && Ant2 == 0)
                            {
                                Ant2 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                Marked[j] = true;
                                j = -1;
                                break;
                            }
                            if ((Tem.Count == 2 && Ant2 == 0) || (Tem.Count == 3 && Ant3 == 0))
                            {
                                if (Tem.Count == 2 && Ant2 == 0)
                                {
                                    Ant2 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                    R.FuzzyResult = Math.Min(Ant1, Ant2)*R.CertaintyFactor;
                                }
                                else
                                {
                                    Ant3 = ((FuzzyNumber)Data.Key).MembershipDegree;
                                    float level1 = Math.Min(Ant1, Ant2);
                                    R.FuzzyResult = Math.Min(Ant3, level1) * R.CertaintyFactor;
                                }
                                Marked[j] = true;
                                FuzzyNumber FN = new FuzzyNumber();
                                string varname = "";
                                string[] RTemp = R.THENPART.Split(new string[] { " is " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (Variable varr in variables)
                                {
                                    foreach (Membership M in varr.MemberShipFunctions)
                                    {
                                        if (RTemp.Length == 2 && M.MembershipName.ToLower().Trim() == RTemp[1].ToLower().Trim() && varr.VariableName.ToLower().Trim() == RTemp[0].ToLower().Trim())
                                        {
                                            FN.MEM = M;
                                            FN.MembershipDegree = R.FuzzyResult;
                                            varname = varr.VariableName;
                                            break;
                                        }
                                        else
                                            if (!postError && RTemp.Length != 2)
                                            {
                                                Main.TExtRef.Invoke(new EventHandler(delegate
                                                     {
                                                         Main.TExtRef.Text += "Rule synatx error in : \" " + R.Data + " \"\n>>";
                                                     }));
                                                postError = true;
                                                return;
                                            }
                                    }
                                }
                                if (FN.MEM != null && !AntecedentVars.ContainsKey(FN))
                                {
                                    AntecedentVars.Add(FN, varname);
                                    Fact F = new Fact(R.THENPART);
                                    F.ProcessFact();                                    
                                    F.DegreeOfBelieve = R.FuzzyResult;
                                    bool addF = true;
                                    for (int k = 0; k < Main.Facts.Count; k++)
                                    {
                                        Fact OldF = (Fact)Main.Facts[k];
                                        if (OldF.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim() == F.Data.Split(new string[] { "is" }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Trim())
                                        {
                                            Main.Facts.Remove(OldF);
                                            F.Index = RandomGenerator.Next(0, 999);
                                            Main.Facts.Add(F);
                                            addF = false;
                                            break;
                                        }
                                    }
                                    if (addF)
                                        Main.Facts.Add(F);
                                    j = AntecedentVars.Count;
                                    //i = 0;
                                    if (DrawTrace)
                                    {
                                        AddRule AR = new AddRule(DrawNewRule);
                                        AR.Invoke(R.Data);
                                    }

                                    break;
                                }
                            }
                        }
                                #endregion
                    }
                }
            }
        }


        bool FirstTime = true;
        private void DrawNewRule(string MSG)
        {
            if (FirstTime)
            {
                LBFiredRules.Invoke(new EventHandler(delegate { LBFiredRules.Items.Clear(); }));
                FirstTime = false;
            }
            LBFiredRules.Invoke(new EventHandler(delegate { LBFiredRules.Items.Add(MSG); }));
        }

        private void ClearR()
        {
            LBFiredRules.Invoke(new EventHandler(delegate { LBFiredRules.Items.Clear(); }));              
        }

        private void CaluculateCentroid()
        {
            float Result = 0;
            for (int i = 0; i < variables.Count; i++)
            {
                Variable V = (Variable)variables[i];
                if (V.VariableType == Variable.VarType.Input_Output || V.VariableType == Variable.VarType.Output)
                {
                    foreach (Membership MEM in V.MemberShipFunctions)
                    {
                        foreach (Rule R in Main.Rules)
                        {
                            string[] Info=R.THENPART.Split(new string[] { "is" },StringSplitOptions.RemoveEmptyEntries);
                            if (Info.Length == 2)
                            {
                                if (Info[0].ToLower().Trim() == V.VariableName.ToLower().Trim() && Info[1].ToLower().Trim() == MEM.MembershipName.ToLower().Trim())
                                {
                                    MEM.OutValue = Math.Max(R.FuzzyResult,MEM.OutValue);
                                }
                            }
                            else
                            {
                                if (!postError && Info.Length != 2)
                                {
                                    Main.TExtRef.Invoke(new EventHandler(delegate
                                    {
                                        Main.TExtRef.Text += "Rule synatx error in : \" " + R.Data + " \"\n>>";
                                    }));
                                    postError = true;
                                }
                            }
                            
                        }
                    }
                    int StepResult = (int)V.VariableRange.Start;
                    
                    float Numerator = 0, Denominator=0;
                    foreach (Membership MEM in V.MemberShipFunctions)
                    {
                        int Current = MEM.MembershipParameters[0];
                        if (StepResult < V.VariableRange.End)
                        {
                            while (StepResult <= MEM.MembershipParameters[MEM.MembershipParameters.Count - 1])
                            {
                                Current += StepResult;
                                StepResult += V.CSS;                                
                                Denominator += MEM.OutValue;
                            }
                            Numerator += StepResult * MEM.OutValue;
                        }
                    }
                    if (Denominator != 0)
                    {
                        Result = Numerator / Denominator;
                        V.CrispOutput = Result;
                        variables[i] = V;
                    }
                    else
                    {
                        ;//MessageBox.Show("Denominator can't be Zero.");//Throw Zero Denominator exception
                    }
                    //break;
                }                
            }
        }

        private void ClearResult()
        {
            foreach (Variable V in variables)
            {
                    foreach (Membership Mem in V.MemberShipFunctions)
                    {
                        Mem.OutValue = 0;
                    }
            }
            foreach (Rule R in Main.Rules)
            {
                R.FuzzyResult = 0;
            }
            AntecedentVars.Clear();
        }

        #endregion


    }
}
