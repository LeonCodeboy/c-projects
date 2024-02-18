using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using MathNet.Symbolics;

namespace GraphApplication
{
    public partial class MainForm : Form
    {
        DrawGraph G;
        FormulaContructor F;
        List<Vertex> V;
        List<Edge> E;
        int[,] AMatrix; //матрица смежности
        int[,] IMatrix; //матрица инцидентности
        int VCount;
        int ECount;
        Dictionary<char, string> EF;
        List<String> CD;
        List<String> CDP;
        const int pShift = 100;
        string lastFilePath;
        enum TYPE_DATA { UNDEFINED = 0, MATRIX, FUNCTIONS };
        const string MATRIX_STR = "__MATRIX__";
        const string FUNCTIONS_STR = "__FUNCTIONS__";

        public MainForm()
        {
            InitializeComponent();
            clearAll();
            clearDict();
            listBoxCircuitMatrix.HorizontalScrollbar = true;
            listBoxIndicMatrix.HorizontalScrollbar = true;
            listBoxMatrix.HorizontalScrollbar = true;
            listBoxParalMatrix.HorizontalScrollbar = true;
            listBoxSeqMatrix.HorizontalScrollbar = true;
            listBoxPatches.HorizontalScrollbar = true;
        }

        private void clearAll()
        {
            V = new List<Vertex>();
            G = new DrawGraph(sheet.Width, sheet.Height);
            E = new List<Edge>();
            VCount = 0;
            ECount = 0;
            sheet.Image = G.GetBitmap();
        }

        private void clearDict()
        {
            EF = new Dictionary<char, string>();
        }

        //создание матрицы смежности и вывод в листбокс
        private void createAdjAndOut()
        {
            AMatrix = new int[V.Count, V.Count];
            G.fillAdjacencyMatrix(V.Count, E, AMatrix);
            listBoxMatrix.Items.Clear();
            string sOut = "     ";
            for (int i = 0; i < V.Count; i++)
                sOut += (i + 1) + " ";
            listBoxMatrix.Items.Add(sOut);
            for (int i = 0; i < V.Count; i++)
            {
                sOut = (i + 1) + " | ";
                for (int j = 0; j < V.Count; j++)
                    sOut += AMatrix[i, j] + " ";
                listBoxMatrix.Items.Add(sOut);
            }
        }

        //создание сжатой матрицы инцидентности
        private void createICAndOut()
        {
            AMatrix = new int[2, E.Count];
            G.fillICMatrix(E, AMatrix);
            listBoxIndicMatrix.Items.Clear();
            string sOut = "     ";
            for (int i = 0; i < E.Count; i++)
                sOut += (i + 1) + " ";
            listBoxIndicMatrix.Items.Add(sOut);
            for (int i = 0; i < 2; i++)
            {
                sOut = (i + 1) + " | ";
                for (int j = 0; j < E.Count; j++)
                    sOut += AMatrix[i, j] + " ";
                listBoxIndicMatrix.Items.Add(sOut);
            }
        }

        //создание матрицы контуров
        private void createCircuitAndOut()
        {
            List<String> AResult = new List<String>(); // через узлы
            List<String> EResult = new List<String>(); // через дуги
            G.fillCurcuitMatrix(V, E, AResult, EResult);
            listBoxCircuitMatrix.Items.Clear();
            // сохранение результата для дальнейшего построения матрицы касаний
            if (CD != null)
                CD.Clear();
            CD = new List<String>(AResult);

            string sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                sOut += AResult.ElementAt(j) + ";";
            }
            try
            {
                sOut = sOut.Substring(0, sOut.Length - 1);
                sOut += " - " + "через узлы";
            } catch
            {
                sOut = "Контуров нет";
            }
            string sOutE = "";
            for (int j = 0; j < EResult.Count; j++)
            {
                sOutE += EResult.ElementAt(j) + ";";
            }
            try
            {
                sOutE = sOutE.Substring(0, sOutE.Length - 1);
                sOutE += " - " + "через дуги";
            }
            catch
            {
                sOutE = "";
            }
            listBoxCircuitMatrix.Items.Add(sOut);
            listBoxCircuitMatrix.Items.Add(sOutE);
        }

        //создание матрицы последовательной смежности
        private void createSeqAndOut()
        {
            List<String> AResult = new List<String>();
            G.fillSeqMatrix(V, E, AResult);
            listBoxSeqMatrix.Items.Clear();

            string sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                String item = AResult.ElementAt(j);
                string[] items = item.Split(new string[] { "&" }, StringSplitOptions.None);
                sOut += items[0] + " ";
            }
            listBoxSeqMatrix.Items.Add(sOut);
            sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                String item = AResult.ElementAt(j);
                string[] items = item.Split(new string[] { "&" }, StringSplitOptions.None);
                sOut += items[1] + " ";
            }
            listBoxSeqMatrix.Items.Add(sOut);
        }

        //создание матрицы параллельной смежности
        private void createParalAndOut()
        {
            List<String> AResult = new List<String>();
            G.fillParallMatrix(V, E, AResult);
            listBoxParalMatrix.Items.Clear();

            string sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                String item = AResult.ElementAt(j);
                string[] items = item.Split(new string[] { "&" }, StringSplitOptions.None);
                sOut += items[0] + " ";
            }
            listBoxParalMatrix.Items.Add(sOut);
            sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                String item = AResult.ElementAt(j);
                string[] items = item.Split(new string[] { "&" }, StringSplitOptions.None);
                sOut += items[1] + " ";
            }
            listBoxParalMatrix.Items.Add(sOut);
        }

        //создание матрицы путей
        private void createPathAndOut()
        {
            List<String> AResult = new List<String>();
            List<String> EResult = new List<String>();
            int vFrom = -1;
            int vTo = -1;
            try
            {
                vFrom = Convert.ToInt32(FromV.Text);
                vTo = Convert.ToInt32(ToV.Text);
                vFrom--;
                vTo--;
            } catch
            {
                AppendTextLog(richTextBoxLog, "Значения вершин указаны неверно!" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                return;
            }
            if (!(vFrom >= 0 && vFrom <= VCount && vTo >= 0 && vTo <= VCount))
            {
                AppendTextLog(richTextBoxLog, "Значения вершин указаны неверно!" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                return;
            }

            G.fillPathMatrix(V, E, vFrom, vTo, AResult, EResult);
            listBoxPatches.Items.Clear();

            // сохранение результата для дальнейшего построения матрицы касаний
            if (CDP != null)
                CDP.Clear();
            CDP = new List<String>(AResult);

            string sOut = "";
            for (int j = 0; j < AResult.Count; j++)
            {
                sOut += AResult.ElementAt(j) + ";";
            }
            try
            {
                sOut = sOut.Substring(0, sOut.Length - 1);
                sOut += " - " + "через узлы";
            }
            catch
            {
                sOut = "Путей нет";
            }
            string sOutE = "";
            for (int j = 0; j < EResult.Count; j++)
            {
                sOutE += EResult.ElementAt(j) + ";";
            }
            try
            {
                sOutE = sOutE.Substring(0, sOutE.Length - 1);
                sOutE += " - " + "через дуги";
            }
            catch
            {
                sOutE = "";
            }
            listBoxPatches.Items.Add(sOut);
            listBoxPatches.Items.Add(sOutE);
        }

        // заполнение вкладки ребер
        private void createEdgeList()
        {
            listBoxEdges.Items.Clear();
            listBoxEdges.Items.Add("Ребра:");
            string sOut = "";
            foreach (Edge e in E)
            {
                if (e.isEdgeLeft())
                    sOut = e.NAME + ": " + (e.V1 + 1) + " -> " + (e.V2 + 1);
                else if (e.isEdgeRight())
                    sOut = e.NAME + ": " + (e.V2 + 1) + " -> " + (e.V1 + 1);
                listBoxEdges.Items.Add(sOut);
            }
        }

        // создание матрицы касаний
        private void createCircuitTouchAndOut()
        {
            AMatrix = new int[CD.Count, CD.Count];
            int [,] APMatrix = new int[CD.Count, CDP.Count];
            if (CD.Count >= 2)
            {
                G.fillITMatrix(E, CD, AMatrix);
                G.fillITPMatrix(E, CD, CDP, APMatrix);
                listBoxTouchMatrix.Items.Clear();
                string sOut = "    ";
                for (int i = 0; i < CD.Count; i++)
                    sOut += (i + 1) + " ";
                sOut += "| ";
                for (int i = 0; i < CDP.Count; i++)
                    sOut += (char)('A' + i) + " ";
                listBoxTouchMatrix.Items.Add(sOut);
                for (int i = 0; i < CD.Count; i++)
                {
                    sOut = (i + 1) + " | ";
                    for (int j = 0; j < CD.Count; j++)
                        sOut += AMatrix[i, j] + " ";
                    sOut += "| ";
                    for (int j = 0; j < CDP.Count; j++)
                        sOut += APMatrix[i, j] + " ";
                    listBoxTouchMatrix.Items.Add(sOut);
                }
            }
            else
            {
                listBoxTouchMatrix.Items.Clear();
                string sOut = "Матрицы касаний нет";
                listBoxTouchMatrix.Items.Add(sOut);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (sheet.Image != null)
            {
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить картинку как...";
                savedialog.OverwritePrompt = true;
                savedialog.CheckPathExists = true;
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                savedialog.ShowHelp = true;
                if (savedialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        sheet.Image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            updateMatrixView();
        }

        private void updateMatrixView()
        {
            try
            {
                int vertex_count = Int32.Parse(textBoxVertexCount.Text);
                int edge_count = Int32.Parse(textBoxEdgeCount.Text);
                if (vertex_count > 0 && edge_count > 0)
                {
                    // создание интерфейса ввода
                    tableLayoutPanelMatrix.Controls.Clear();
                    tableLayoutPanelMatrix.ColumnStyles.Clear();
                    tableLayoutPanelMatrix.RowStyles.Clear();
                    tableLayoutPanelMatrix.AutoScroll = false;
                    tableLayoutPanelMatrix.AutoScroll = true;
                    tableLayoutPanelMatrix.ColumnCount = edge_count;
                    tableLayoutPanelMatrix.RowCount = vertex_count;
                    for (int i = 0; i < edge_count; i++)
                    {
                        tableLayoutPanelMatrix.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                        for (int j = 0; j < vertex_count; j++)
                        {
                            if (i == 0)
                            {
                                tableLayoutPanelMatrix.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            }
                            TextBox item = new TextBox();
                            item.Width = 20;
                            item.Height = 20;
                            item.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                            item.TextChanged += delegate (object sender, EventArgs args)
                            {
                                TextBox sourceText = (TextBox)sender;
                                if (!String.IsNullOrWhiteSpace(sourceText.Text))
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(sourceText.Text, "^(?:(?:[-01])|(?:-1))$"))
                                    {
                                        AppendTextLog(richTextBoxLog, "Допустимые значения элемента матрицы: 0, 1, -1" + 
                                            Environment.NewLine, Color.Red, new Font("Courier New", 8));
                                        sourceText.Text = sourceText.Text.Remove(sourceText.Text.Length - 1);
                                    }
                                }
                            };
                            tableLayoutPanelMatrix.Controls.Add(item, i, j);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Невозможно инициализировать исходные данные по умолчанию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxVertexCount_TextChanged(object sender, EventArgs e)
        {
            TextBox sourceText = (TextBox) sender;
            if (!String.IsNullOrWhiteSpace(sourceText.Text))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sourceText.Text, "^[1-9][0-9]{0,1}$"))
                {
                    AppendTextLog(richTextBoxLog, "Кол-во вершин графа должно быть от 1 до 20" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                    sourceText.Text = sourceText.Text.Remove(sourceText.Text.Length - 1);
                }
                else
                {
                    updateMatrixView();
                }
            }
        }

        private void textBoxEdgeCount_TextChanged(object sender, EventArgs e)
        {
            TextBox sourceText = (TextBox)sender;
            if (!String.IsNullOrWhiteSpace(sourceText.Text))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(sourceText.Text, "^[1-9][0-9]{0,1}$"))
                {
                    AppendTextLog(richTextBoxLog, "Кол - во ребер графа должно быть от 1 до 20" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                    sourceText.Text = sourceText.Text.Remove(sourceText.Text.Length - 1);
                }
                else
                {
                    updateMatrixView();
                }
            }
        }

        private void AppendTextLog(RichTextBox box, string text, Color color, Font font)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionFont = font;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        private void buttonRandomMatrix_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            for (int i = 0; i < tableLayoutPanelMatrix.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanelMatrix.ColumnCount; j++)
                {
                    int rInt = r.Next(-1, 2);
                    TextBox destBox = (TextBox) tableLayoutPanelMatrix.GetControlFromPosition(j, i);
                    try
                    {
                        destBox.Text = rInt.ToString();
                    } catch 
                    {
                        MessageBox.Show("Невозможно рандомно инициализировать матрицу", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonBuildGraph_Click(object sender, EventArgs e)
        {
            // очистка данных
            clearAll();

            // заполнение матрицы инцидентности
            VCount = tableLayoutPanelMatrix.RowCount;
            ECount = tableLayoutPanelMatrix.ColumnCount;
            IMatrix = new int[VCount, ECount];
            for (int i = 0; i < VCount; i++)
            {
                for (int j = 0; j < ECount; j++)
                {
                    TextBox destBox = (TextBox)tableLayoutPanelMatrix.GetControlFromPosition(j, i);
                    try
                    {
                        IMatrix[i, j] = Int32.Parse(destBox.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно построить граф", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            
            // инициализация временного контейнера
            List<Edge> edges = new List<Edge>();
            for (int j = 0; j < ECount; j++)
            {
                Edge edge = new Edge(-1, -1);
                edge.NAME = (char)('A' + j);
                edges.Add(edge);
            }

            for (int j = 0; j < VCount; j++)
            {
                Vertex vertex = new Vertex(-1, -1);
                V.Add(vertex);
            }

            for (int i = 0; i < ECount; i++)
            {
                for (int j = 0; j < VCount; j++)
                {
                    if (edges.ElementAtOrDefault(i).V1 != -1)
                    {
                        if (IMatrix[j, i] == 1)
                        {
                            Edge edge = new Edge(j, edges[i].V1);
                            edge.NAME = (char)('A' + i);
                            edge.setEdgeRight();
                            edges[i] = edge;
                            E.Add(edge);
                        }
                        else if (IMatrix[j, i] == -1)
                        {
                            Edge edge = new Edge(j, edges[i].V1);
                            edge.NAME = (char)('A' + i);
                            edge.setEdgeLeft();
                            edges[i] = edge;
                            E.Add(edge);
                        }
                    }
                    else
                    {
                        if (IMatrix[j, i] == 1)
                        {
                            Edge edge = new Edge(j, -1);
                            edge.NAME = (char)('A' + i);
                            edge.setEdgeRight();
                            edges[i] = edge;
                        }
                        else if (IMatrix[j, i] == -1)
                        {
                            Edge edge = new Edge(j, -1);
                            edge.NAME = (char)('A' + i);
                            edge.setEdgeLeft();
                            edges[i] = edge;
                        }
                    }
                }
                if (edges[i].V2 == -1 && edges[i].V1 != -1)
                {
                    Edge edge = new Edge(edges[i].V1, edges[i].V1);
                    edge.NAME = (char)('A' + i);
                    edge.setEdgeRight();
                    edges[i] = edge;
                    E.Add(edge);
                }
            }

            // настройка формул
            foreach (Edge edge in E)
            {
                if (EF.ContainsKey(edge.NAME))
                {
                    string value = "";
                    if (EF.TryGetValue(edge.NAME, out value))
                        edge.FORMULA_STR = value;
                }
            }

            // настройка отображения
            int sheetW = sheet.Width;
            int sheetH = sheet.Height;
            G.setUpCoordinates(V, sheetW, sheetH, pShift);
            G.correctNameEdges(E);

            G.clearSheet();
            G.drawALLGraph(V, E);
            sheet.Image = G.GetBitmap();
            // создание матрицы смежности
            createAdjAndOut();
            // создание сжатой матрицы инцидентности
            createICAndOut();
            // создание матрицы контуров
            createCircuitAndOut();
            // создание последовательной матрицы смежности
            createSeqAndOut();
            // создание параллельной матрицы смежности
            createParalAndOut();
            // создание матрицы путей
            createPathAndOut();
            // заполнение вкладки ребер
            createEdgeList();
            // создание матрицы касаний
            createCircuitTouchAndOut();
            // сообщение об успешном завершении
            AppendTextLog(richTextBoxLog, "Построение графа успешно завершено" + Environment.NewLine, Color.Green, new Font("Courier New", 8));
            buttonBuildFunction.Enabled = true;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // при изменении размера окна необходимо изменить параметры графа
            //clearAll();
        }

        private void zeroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tableLayoutPanelMatrix.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanelMatrix.ColumnCount; j++)
                {
                    TextBox destBox = (TextBox)tableLayoutPanelMatrix.GetControlFromPosition(j, i);
                    destBox.Text = "0";
                }
            }
        }

        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String fileContent = string.Empty;
            String filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName;
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    Stream fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            lastFilePath = filePath;
            // анализ файла
            List<String> lines = fileContent.Split(new string[] { Environment.NewLine }, 
                StringSplitOptions.None).ToList<String>();
            int ColumnCount = -1;
            // определение кол-ва строк матрицы
            int RowCount = 0;
            bool flg = false;
            foreach (String line in lines)
            {
                if (line.CompareTo(MATRIX_STR) == 0)
                {
                    flg = true;
                    continue;
                }
                else if (line.CompareTo(FUNCTIONS_STR) == 0)
                    flg = false;
                else if (line.Trim().Length == 0)
                    flg = false;
                if (flg)
                    RowCount++;
            }
            textBoxVertexCount.Text = Convert.ToString(RowCount);
            int i = 0;
            TYPE_DATA type = TYPE_DATA.UNDEFINED;
            foreach (String line in lines) {
                if (line.CompareTo(MATRIX_STR) == 0)
                {
                    type = TYPE_DATA.MATRIX;
                    continue;
                }
                else if (line.CompareTo(FUNCTIONS_STR) == 0)
                {
                    type = TYPE_DATA.FUNCTIONS;
                    continue;
                }
                else if (line.Trim().Length == 0)
                {
                    type = TYPE_DATA.UNDEFINED;
                    continue;
                }
                if (type == TYPE_DATA.MATRIX)
                {
                    Regex regex = new Regex(@"\s*([01-]+)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = regex.Matches(line);
                    if (ColumnCount == -1)
                    {
                        ColumnCount = matches.Count;
                        textBoxEdgeCount.Text = Convert.ToString(ColumnCount);
                    }
                    else if (ColumnCount != matches.Count)
                    {
                        AppendTextLog(richTextBoxLog, "Формат файла не корректен! (количество столбцов должно не меняться)" +
                            Environment.NewLine, Color.Red, new Font("Courier New", 8));
                        return;
                    }

                    if (matches.Count > 0)
                    {
                        int j = 0;
                        foreach (Match match in matches)
                        {
                            TextBox destBox = (TextBox)tableLayoutPanelMatrix.GetControlFromPosition(j, i);
                            String item = match.Value;
                            destBox.Text = item.Trim();
                            j++;
                        }
                    }
                    i++;
                }
                else if (type == TYPE_DATA.FUNCTIONS)
                {
                    Regex regex = new Regex(@"^\s*([a-zA-Z]\=[^\=]+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            List<string> edge_formula = new List<string>();
                            edge_formula.AddRange(match.Value.Split('='));
                            if (edge_formula.Count == 2)
                            {
                                EF[edge_formula.ElementAt(0)[0]] = edge_formula.ElementAt(1);
                            }
                            else
                            {
                                AppendTextLog(richTextBoxLog, "Формат файла не корректен! (формула ребра указана неверно)" +
                                    Environment.NewLine, Color.Red, new Font("Courier New", 8));
                                return;
                            }
                        }
                    }
                }
            }
            FromV.Text = "1";
            ToV.Text = RowCount.ToString();
        }

        private void buttonUpdatePath_Click(object sender, EventArgs e)
        {
            // создание матрицы путей
            createPathAndOut();
            // создание матрицы касаний
            createCircuitTouchAndOut();
        }

        // конструктор формул
        private void bNewFormula_Click(object sender, EventArgs e)
        {
            if (tabControlGraph.SelectedIndex == 1 && ckUseEditor.isTurnOn)
            {
                pictureBoxFormula.InitialImage = null;
                F = new FormulaContructor(pictureBoxFormula.Width, pictureBoxFormula.Height);
                F.updateBitmap += F_updateBitmap;
                F.errorMessage += F_errorMessage;
                F.showFormula += F_showFormula;
                pictureBoxFormula.Image = F.GetBitmap();
            }
        }

        private void F_showFormula(object sender, EventArgs e)
        {
            SystAnalys_lr1.FCEventArgs eGr = (SystAnalys_lr1.FCEventArgs)e;
            textBoxFLinear.Text = eGr.cParam;
        }

        private void F_errorMessage(object sender, EventArgs e)
        {
            SystAnalys_lr1.FCEventArgs eGr = (SystAnalys_lr1.FCEventArgs)e;
            AppendTextLog(richTextBoxLog, eGr.cParam + Environment.NewLine, 
                Color.Red, new Font("Courier New", 8));
        }

        private void F_updateBitmap(object sender, EventArgs e)
        {
            pictureBoxFormula.Image = F.GetBitmap();
        }

        private void buttonSaveForEdge_Click(object sender, EventArgs e)
        {
            if (listBoxEdges.SelectedIndex != -1)
            {
                int index = listBoxEdges.SelectedIndex;
                index--;
                E[index].FORMULA_STR = textBoxFLinear.Text;
            }
        }

        private void listBoxEdges_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxEdges.SelectedIndex > 0)
            {
                textBoxFLinear.Enabled = true;
                int index = listBoxEdges.SelectedIndex;
                index--;
                textBoxFLinear.Text = E[index].FORMULA_STR;
            }
        }

        private void textBoxFLinear_Validating(object sender, CancelEventArgs e)
        {
            // проверка корректности формулы
            string txt = textBoxFLinear.Text.ToUpper();
            short cnt_brackets = 0;
            // допустимый алфавит
            char[] aAlpha = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                '-', '+', '*', '/', ' ', 'S', '(', ')', '^' };
            bool nSym = true;
            foreach (char s in txt)
            {
                if (!aAlpha.Contains(s))
                {
                    nSym = false;
                    break;
                }
                if (s == '(')
                    cnt_brackets++;
                else if (s == ')')
                    cnt_brackets--;
            }

            if (!nSym || cnt_brackets != 0)
            {
                AppendTextLog(richTextBoxLog, "Неверная формула" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                e.Cancel = true;
            }
            else
            {
                // проверка вычисляемости
                try
                {
                    var exp1 = Infix.ParseOrThrow("(1-x*(7-2*x)+(10-6*x))/(1-x*9)+9*x-7*x-5");
                    string exp1_1 = Infix.Format(exp1);
                    var exp = Infix.ParseOrThrow(txt);
                    AppendTextLog(richTextBoxLog, "Корректная формула" + Environment.NewLine, Color.Green, new Font("Courier New", 8));
                    e.Cancel = false;
                }
                catch (Exception)
                {
                    AppendTextLog(richTextBoxLog, "Неверная формула" + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                    e.Cancel = true;
                }
            }
        }

        private void tabControlGraph_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControlGraph.SelectedIndex == 1 && ckUseEditor.isTurnOn && F != null)
            {
                char theKeyAsAString = Convert.ToChar(e.KeyValue);
                if ((theKeyAsAString >= '0' && theKeyAsAString <= '9') || theKeyAsAString == 'S' || theKeyAsAString == 's')
                {
                    String keyStr = theKeyAsAString.ToString();
                    F.printTextCharCursorPosition(keyStr);
                }
            }
        }

        private void textBoxFLinear_Enter(object sender, EventArgs e)
        {
            ckUseEditor.isTurnOn = false;
        }

        private void bDelFormula_Click(object sender, EventArgs e)
        {
            if (tabControlGraph.SelectedIndex == 1 && ckUseEditor.isTurnOn)
                F.undoState();
        }

        private void buttonBuildFunction_Click(object sender, EventArgs e)
        {

        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastFilePath == null || lastFilePath.Trim().Length == 0)
            {
                AppendTextLog(richTextBoxLog, "Текущий файл неизвестен (выполните загрузку из файла)" 
                    + Environment.NewLine, Color.Red, new Font("Courier New", 8));
                return;
            }
            else
            {
                FileStream fcreate = File.Open(lastFilePath, FileMode.Create);
                using (var sw = new StreamWriter(fcreate))
                {
                    if (VCount > 0)
                        sw.WriteLine(MATRIX_STR);
                    for (int i = 0; i < VCount; i++)
                    {
                        string data_line = "";
                        for (int j = 0; j < ECount; j++)
                        {
                            TextBox destBox = (TextBox)tableLayoutPanelMatrix.GetControlFromPosition(j, i);
                            data_line += destBox.Text;
                            data_line += " ";
                        }
                        sw.WriteLine(data_line);
                    }
                    if (ECount > 0)
                        sw.WriteLine(FUNCTIONS_STR);
                    foreach (Edge edge in E)
                    {
                        sw.WriteLine(edge.NAME + "=" + edge.FORMULA_STR);
                    }
                }
                AppendTextLog(richTextBoxLog, "Сохранение данных успешно выполнено" 
                    + Environment.NewLine, Color.Green, new Font("Courier New", 8));
            }
        }

        private void bRightFormula_Click(object sender, EventArgs e)
        {
            if (tabControlGraph.SelectedIndex == 1 && ckUseEditor.isTurnOn && F != null)
            {
                F.rightCursorShift();
            }
        }

        private void bLeftFormula_MouseClick(object sender, MouseEventArgs e)
        {
            if (tabControlGraph.SelectedIndex == 1 && ckUseEditor.isTurnOn && F != null)
            {
                F.leftCursorShift();
            }
        }

        private void bTrashFormula_Click(object sender, EventArgs e)
        {
            F.deleteCharFromCurrentPos();
        }
    }
}
