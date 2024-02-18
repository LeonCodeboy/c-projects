using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphApplication
{
    class PointScreen
    {
        private int x, y;

        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public PointScreen(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class Vertex
    {
        private int x, y;
        // флаг для пометки построенной вершины
        private bool isEx;

        public bool ISEX
        {
            get
            {
                return this.isEx;
            }
            set
            {
                this.isEx = value;
            }
        }

        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public Vertex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class Edge
    {
        /* 
         * левое направление (из вершины v2 в v1) то есть V1
         * правое направление (из вершины v1 в v2) то есть V2
        */
        private enum DIRECTIONS { LEFT = -1, RIGHT = 1 }
        // номера вершин
        private int v1, v2;
        private char name;
        private DIRECTIONS direction;
        private string FormulaStr;

        public string FORMULA_STR
        {
            get
            {
                return this.FormulaStr;
            }
            set
            {
                this.FormulaStr = value;
            }
        }
        public char NAME
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public int V1
        {
            get
            {
                return this.v1;
            }
            set
            {
                this.v1 = value;
            }
        }

        public int V2
        {
            get
            {
                return this.v2;
            }
            set
            {
                this.v2 = value;
            }
        }

        public void setEdgeLeft()
        {
            this.direction = DIRECTIONS.LEFT;
        }

        public void setEdgeRight()
        {
            this.direction = DIRECTIONS.RIGHT;
        }

        public bool isEdgeLeft()
        {
            if (this.direction == DIRECTIONS.LEFT)
                return true;
            return false;
        }

        public bool isEdgeRight()
        {
            if (this.direction == DIRECTIONS.RIGHT)
                return true;
            return false;
        }

        public Edge(int v1, int v2, string formula_str = "")
        {
            this.v1 = v1;
            this.v2 = v2;
            this.FormulaStr = formula_str;
        }
    }

    class DrawGraph
    {
        Bitmap bitmap;
        Pen blackPen;
        Pen redPen;
        Pen darkGoldPen;
        Graphics gr;
        Font fo;
        Brush br;
        PointF point;
        public int R = 35; //радиус окружности вершины
        private const int arrowOffsetX = 5;
        private const int arrowOffsetY = 5;

        // для того чтобы при отрисовке названия ребер не наехжали друг на друга
        private List<RectangleF> pointsEdgeNameData;
        private int rRect = 15;
        private Dictionary<String, PointF> coordVrtx;

        public DrawGraph(int width, int height)
        {
            bitmap = new Bitmap(width, height);
            gr = Graphics.FromImage(bitmap);
            clearSheet();
            blackPen = new Pen(Color.Black);
            blackPen.Width = 2;
            redPen = new Pen(Color.Red);
            redPen.Width = 2;
            darkGoldPen = new Pen(Color.DarkGoldenrod);
            darkGoldPen.Width = 2;
            fo = new Font("Arial", 15);
            br = Brushes.Black;
            coordVrtx = new Dictionary<String, PointF>();
        }

        public Bitmap GetBitmap()
        {
            return bitmap;
        }

        public void clearSheet()
        {
            gr.Clear(Color.White);
        }

        public void setUpCoordinates(List<Vertex> V, int W, int H, int shift)
        {
            // правое диагональное заполнение
            V[0].X = W / 16;
            V[0].Y = H / 2;
            int k = 0;
            int c_ind = 0;
            for (int i = 1; i < V.Count; i++)
            {
                if (i % 3 == 0)
                {
                    V[i].X = V[k].X + shift;
                    V[i].Y = V[k].Y + shift;
                    if (i != 0)
                        k = c_ind;
                }
                else if (i % 3 == 1)
                {
                    V[i].X = V[k].X + shift + shift / 2;
                    V[i].Y = V[k].Y;
                    c_ind = i;
                }
                else
                {
                    V[i].X = V[k].X + shift;
                    V[i].Y = V[k].Y - shift;
                }
            }
        }

        public void drawVertex(int x, int y, int number)
        {
            gr.FillEllipse(Brushes.White, (x - R), (y - R), 2 * R, 2 * R);
            gr.DrawEllipse(blackPen, (x - R), (y - R), 2 * R, 2 * R);
            point = new PointF(x - 9, y - 9);
            gr.DrawString(Convert.ToString(number), fo, br, point);
        }

        private void recalcEdgeCoords(PointF p1, PointF p2, ref PointF newP1, ref PointF newP2)
        {
            float a = (p1.X - p2.X);
            float b = (p1.Y - p2.Y);
            float c = (float) Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

            // левая точка
            newP1.X = p1.X - (a / c * R);
            newP1.Y = p1.Y - (b / c * R);

            // правая точка
            newP2.X = p2.X + (a / c * R);
            newP2.Y = p2.Y + (b / c * R);
        }

        private PointF checkPointsEdgeNameData(PointF newItem)
        {
            // проверка что точка не содержится ни в одном прямоугольнике
            if (pointsEdgeNameData != null && pointsEdgeNameData.Count >= 0)
            {
                foreach (RectangleF rect in pointsEdgeNameData)
                {
                    if (rect.Contains(newItem))
                        newItem.X = newItem.X + rRect;
                }
            }

            RectangleF newRect = new RectangleF(new PointF(newItem.X - rRect / 2, newItem.Y - rRect / 2), new SizeF(rRect, rRect));
            this.pointsEdgeNameData.Add(newRect);
            return newItem;
        }

        private PointF alignPointForText(PointF point, PointF p2)
        {
            PointF nPoint = new PointF();
            if (point.X > p2.X)
            {
                nPoint.X = point.X + 7;
                nPoint.Y = (nPoint.X - point.X) * (p2.Y - point.Y) / (p2.X - point.X) + point.Y;
            }
            else if (point.X < p2.X)
            {
                nPoint.X = point.X - 7;
                nPoint.Y = (nPoint.X - point.X) * (p2.Y - point.Y) / (p2.X - point.X) + point.Y;
            }
            else
            {
                if (point.Y > p2.Y)
                {
                    nPoint.X = point.X;
                    nPoint.Y = point.Y;
                    nPoint.Y += 9;
                }
                else
                {
                    nPoint.X = point.X;
                    nPoint.Y = point.Y;
                    nPoint.Y -= 9;
                }
            }

            nPoint.Y -= 10;
            nPoint.X -= 7;
            return nPoint;
        }

        public void drawEdge(Vertex V1, Vertex V2, Edge E)
        {
            // помечаем что вершина построена
            V1.ISEX = true;
            V2.ISEX = true;

            darkGoldPen = new Pen(Color.DarkGoldenrod);
            darkGoldPen.Width = 2;
            Font cFo = new Font("Verdana", 10, FontStyle.Bold);
            if (E.V1 == E.V2)
            {
                int R_LOC = (int) (.75 * R);
                gr.DrawArc(darkGoldPen, (V1.X - 2 * R_LOC), (V1.Y - 2 * R_LOC), 2 * R_LOC, 2 * R_LOC, 90, 360);
                point = new PointF(V1.X - (int)(1.75 * R_LOC), V1.Y - (int)(1.75 * R_LOC));
                point = checkPointsEdgeNameData(point);
                gr.DrawString((E.NAME).ToString(), cFo, br, point);
                drawVertex(V1.X, V1.Y, (E.V1 + 1));
            }
            else
            {
                if (E.isEdgeLeft())
                {
                    drawVertex(V1.X, V1.Y, (E.V1 + 1));
                    drawVertex(V2.X, V2.Y, (E.V2 + 1));
                    AdjustableArrowCap bigArrow = new AdjustableArrowCap(arrowOffsetX, arrowOffsetY);
                    darkGoldPen.CustomEndCap = bigArrow;
                    PointF nP1 = new PointF();
                    PointF nP2 = new PointF();
                    recalcEdgeCoords(new PointF(V1.X, V1.Y), new PointF(V2.X, V2.Y), ref nP1, ref nP2);
                    gr.DrawLine(darkGoldPen, nP1, nP2);

                    //point = new PointF((V1.X + V2.X) / 2, (V1.Y + V2.Y) / 2);
                    point = nP1;
                    point = checkPointsEdgeNameData(point);
                    point = alignPointForText(point, nP2);
                    coordVrtx.Add(E.NAME.ToString(), point);
                }
                if (E.isEdgeRight())
                {
                    drawVertex(V1.X, V1.Y, (E.V1 + 1));
                    drawVertex(V2.X, V2.Y, (E.V2 + 1));
                    AdjustableArrowCap bigArrow = new AdjustableArrowCap(arrowOffsetX, arrowOffsetY);
                    darkGoldPen.CustomStartCap = bigArrow;
                    PointF nP1 = new PointF();
                    PointF nP2 = new PointF();
                    recalcEdgeCoords(new PointF(V1.X, V1.Y), new PointF(V2.X, V2.Y), ref nP1, ref nP2);
                    gr.DrawLine(darkGoldPen, nP1, nP2);

                    //point = new PointF((V1.X + V2.X) / 2, (V1.Y + V2.Y) / 2);
                    point = nP2;
                    point = checkPointsEdgeNameData(point);
                    point = alignPointForText(point, nP1);
                    coordVrtx.Add(E.NAME.ToString(), point);
                }
            }
        }

        public void correctNameEdges(List<Edge> E)
        {
            for (int i = 0; i < E.Count; i++)
            {
                for (int j = 0; j < E.Count; j++)
                {
                    if (i != j && E[i].V1 == E[j].V2 && E[i].V2 == E[j].V1)
                    {
                        E[i].NAME = new Char();
                    }
                }
            }
        }

        public void drawALLGraph(List<Vertex> V, List<Edge> E)
        {
            this.pointsEdgeNameData = new List<RectangleF>();
            //рисуем ребра
            for (int i = 0; i < E.Count; i++)
            {
                drawEdge(V[E[i].V1], V[E[i].V2], E[i]);
            }
            this.pointsEdgeNameData.Clear();
            // отрисовка несвязных вершин
            for (int i = 0; i < V.Count; i++)
            {
                if (V[i].ISEX == false)
                    drawVertex(V[i].X, V[i].Y, i + 1);
            }
            // отрисовка наименований ребер
            Font cFo = new Font("Verdana", 12, FontStyle.Bold);
            foreach (String key in coordVrtx.Keys)
            {
                gr.DrawString(key, cFo, Brushes.DarkOliveGreen, coordVrtx[key]);
            }
        }

        //заполняет матрицу смежности
        public void fillAdjacencyMatrix(int numberV, List<Edge> E, int[,] matrix)
        {
            for (int i = 0; i < numberV; i++)
                for (int j = 0; j < numberV; j++)
                    matrix[i, j] = 0;
            for (int i = 0; i < E.Count; i++)
            {
                if (E[i].isEdgeRight())
                    matrix[E[i].V1, E[i].V2] = 1;
                if (E[i].isEdgeLeft())
                matrix[E[i].V2, E[i].V1] = 1;
            }
        }

        //заполняет сжатую матрицу инцидентности
        public void fillICMatrix(List<Edge> E, int[,] matrix)
        {
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < E.Count; j++)
                    matrix[i, j] = 0;
            for (int i = 0; i < E.Count; i++)
            {
                if (E[i].isEdgeLeft())
                {
                    matrix[1, i] = E[i].V2 + 1;
                    matrix[0, i] = E[i].V1 + 1;
                }
                else if (E[i].isEdgeRight())
                {
                    matrix[1, i] = E[i].V1 + 1;
                    matrix[0, i] = E[i].V2 + 1;
                }
            }
        }

        private short hasAtLeastOneEqualChar(String str1, String str2)
        {
            short resT = 0;
            List<String> vertexReal1 = new List<String>();
            Regex regex = new Regex(@"(\d)|(\(\d+\))");
            MatchCollection matches = regex.Matches(str1);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    String mVal = match.Value;
                    mVal = mVal.Replace("(", "");
                    mVal = mVal.Replace(")", "");
                    vertexReal1.Add(mVal);
                }
            }

            List<String> vertexReal2 = new List<String>();
            matches = regex.Matches(str2);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    String mVal = match.Value;
                    mVal = mVal.Replace("(", "");
                    mVal = mVal.Replace(")", "");
                    vertexReal2.Add(mVal);
                }
            }

            foreach (String ch1 in vertexReal1)
                foreach (String ch2 in vertexReal2)
                    if (ch1.CompareTo(ch2) == 0)
                    {
                        resT = 1;
                        goto RET_LABEL;
                    }
            RET_LABEL:
            {
                return resT;
            }
        }

        //заполняет матрицу касаний на основе матрицы контуров
        public void fillITMatrix(List<Edge> E, List<String> circuitM, int[,] matrix)
        {
            for (int i = 0; i < circuitM.Count; i++)
            {
                for (int j = i; j < circuitM.Count; j++)
                {
                    String cI = circuitM.ElementAt(i);
                    String cJ = circuitM.ElementAt(j);
                    
                    int valT = hasAtLeastOneEqualChar(cI, cJ);
                    matrix[j, i] = matrix[i, j] = valT;
                }
            }
        }

        //заполняет матрицу касаний на основе матрицы контуров для путей
        public void fillITPMatrix(List<Edge> E, List<String> circuitM, List<String> circuitP, int[,] matrix)
        {
            for (int i = 0; i < circuitM.Count; i++)
            {
                for (int j = i; j < circuitP.Count; j++)
                {
                    String cI = circuitM.ElementAt(i);
                    String cJ = circuitM.ElementAt(j);

                    int valT = hasAtLeastOneEqualChar(cI, cJ);
                    matrix[j, i] = matrix[i, j] = valT;
                }
            }
        }

        private List<int> findFromVertexPathes(int fromVertex, List<Edge> E)
        {
            List<int> pathes = new List<int>();
            for (int i = 0; i < E.Count; i++)
            {
                if (E[i].isEdgeLeft())
                {
                    if (E[i].V1 == fromVertex)
                        pathes.Add(E[i].V2);
                }
                else if (E[i].isEdgeRight())
                {
                    if (E[i].V2 == fromVertex)
                        pathes.Add(E[i].V1);
                }
            }
            return pathes;
        }

        // поиск ребер из вершины
        private List<Edge> findFromVertexEdgesPathes(int fromVertex, List<Edge> E)
        {
            List<Edge> pathes = new List<Edge>();
            for (int i = 0; i < E.Count; i++)
            {
                if (E[i].isEdgeLeft())
                {
                    if (E[i].V1 == fromVertex)
                        pathes.Add(E[i]);
                }
                else if (E[i].isEdgeRight())
                {
                    if (E[i].V2 == fromVertex)
                        pathes.Add(E[i]);
                }
            }
            return pathes;
        }

        // поиск ребер в вершину
        private List<Edge> findToVertexEdgesPathes(int fromVertex, List<Edge> E)
        {
            List<Edge> pathes = new List<Edge>();
            for (int i = 0; i < E.Count; i++)
            {
                if (E[i].isEdgeLeft())
                {
                    if (E[i].V2 == fromVertex)
                        pathes.Add(E[i]);
                }
                else if (E[i].isEdgeRight())
                {
                    if (E[i].V1 == fromVertex)
                        pathes.Add(E[i]);
                }
            }
            return pathes;
        }

        // поиск ребра связанного с вершинами
        private Char findEdgeAssociatedWith(int V1, int V2, List<Edge> E)
        {
            foreach (Edge e in E)
            {
                if (e.isEdgeLeft())
                {
                    if ((e.V1 == V1) && (e.V2 == V2))
                    {
                        return (e.NAME);
                    }
                }
                else if (e.isEdgeRight())
                {
                    if ((e.V2 == V1) && (e.V1 == V2))
                    {
                        return (e.NAME);
                    }
                }
            }
            return Char.MinValue;
        }

        private void formPathString(String path, int startIndex, int fromVertex, List<Edge> E, int maxPathLength, List<String> patch_result)
        {
            List<int> patches = findFromVertexPathes(fromVertex, E);
            String sPath = path;
            foreach (int next in patches)
            {
                if (startIndex != next)
                {
                    if (path.Length >= maxPathLength)
                    {
                        return;
                    }
                    else
                    {
                        Int32 ch_str = next + 1;
                        if (path.Contains(ch_str.ToString()))
                            continue;
                        if (ch_str > 9)
                        {
                            string p_str = '(' + ch_str.ToString() + ')';
                            path += p_str;
                        }
                        else
                            path += ch_str;
                        formPathString(path, startIndex, next, E, maxPathLength, patch_result);
                    }
                }
                else
                {
                    if (!patch_result.Contains(path)) {
                        patch_result.Add(path);
                    }
                }
                path = sPath;
            }
            return;
        }

        // поиск пути из startIndex в endIndex
        private void formPathStringConcrete(String path, int startIndex, int endIndex, int fromVertex, List<Edge> E, int maxPathLength, List<String> patch_result)
        {
            List<int> patches = findFromVertexPathes(fromVertex, E);
            String sPath = path;
            foreach (int next in patches)
            {
                Int32 ch_str = next + 1;
                if (path.Contains(ch_str.ToString()))
                    continue;
                if (ch_str > 9)
                {
                    string p_str = '(' + ch_str.ToString() + ')';
                    path += p_str;
                }
                else
                    path += ch_str;
                if (next != endIndex)
                {
                    if (path.Length >= maxPathLength)
                    {
                        return;
                    }
                    else
                    {
                        formPathStringConcrete(path, next, endIndex, next, E, maxPathLength, patch_result);
                    }
                }
                else
                {
                    if (!patch_result.Contains(path))
                    {
                        if(ifUnique(path)) {
                            patch_result.Add(path);
                        }
                    }
                }
                path = sPath;
            }
            return;
        }

        private bool ifUnique(string toCheck)
        {
            string str = "";
            for (int i = 0; i < toCheck.Length; i++)
            {
                if (str.Contains("" + toCheck.ElementAt(i)))
                    return false;
                str += toCheck.ElementAt(i);
            }
            return true;
        }

        //заполняет матрицу контуров
        public void fillCurcuitMatrix(List<Vertex> V, List<Edge> E, List<String> resultOut, List<String> resultOutE)
        {
            List<String> pResults = new List<string>();
            // контуры разной длины
            for (int i = 0; i < V.Count; i++)
            {
                String path = "";
                if (i + 1 > 9)
                {
                    string p_str = '(' + (i + 1).ToString() + ')';
                    path = p_str;
                }
                else
                    path = Convert.ToString(i + 1);

                formPathString(path, i, i, E, V.Count, pResults);
            }
            List<String> resultOutF = new List<String>();
            // удаление повторных контуров
            foreach (String item in pResults)
            {
                String result = String.Concat(item.OrderBy(c => c));
                if (!resultOutF.Contains(result))
                {
                    resultOutF.Add(result);
                    resultOut.Add(item);
                }
            }
            // формирование через дуги
            foreach (String item in resultOut)
            {
                String resultE = "";
                String resultB = "";

                List<String> vertexReal = new List<String>();
                Regex regex = new Regex(@"(\d)|(\(\d+\))");
                MatchCollection matches = regex.Matches(item);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        String mVal = match.Value;
                        mVal = mVal.Replace("(", "");
                        mVal = mVal.Replace(")", "");
                        vertexReal.Add(mVal);
                    }
                }

                for (int i = 0; i < vertexReal.Count; i++)
                {
                    int V1 = Convert.ToInt32(vertexReal[i]);
                    V1--;
                    int V2 = 0;
                    if (i + 1 == vertexReal.Count)
                        V2 = Convert.ToInt32(vertexReal[0]);
                    else
                        V2 = Convert.ToInt32(vertexReal[i + 1]);
                    V2--;

                    Char ret = findEdgeAssociatedWith(V1, V2, E);
                    if (ret != Char.MinValue)
                        resultE += ret;
                    ret = findEdgeAssociatedWith(V2, V1, E);
                    if (ret != Char.MinValue)
                        resultB += ret;
                }
                
                String _resultE = String.Concat(resultE.OrderBy(c => c));
                String _resultB = String.Concat(resultB.OrderBy(c => c));
                if (_resultE.CompareTo(_resultB) == 0)
                {
                    if (resultE.Length == vertexReal.Count)
                    {
                        resultOutE.Add(resultE);
                    }
                }
                else
                {
                    if (resultE.Length == vertexReal.Count)
                        resultOutE.Add(resultE);
                    if (resultB.Length == vertexReal.Count)
                        resultOutE.Add(resultB);
                }
            }
        }

        //заполняет матрицу путей
        public void fillPathMatrix(List<Vertex> V, List<Edge> E, int fromV, int toV, List<String> resultOut, List<String> resultOutE)
        {
            List<String> pResults = new List<string>();
     
            String path = Convert.ToString(fromV + 1);
            formPathStringConcrete(path, fromV, toV, fromV, E, V.Count + 1, pResults);
            // удаление некорректных путей
            foreach (String item in pResults)
            {
                List<String> vertexReal = new List<String>();
                Regex regex = new Regex(@"(\d)|(\(\d+\))");
                MatchCollection matches = regex.Matches(item);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        String mVal = match.Value;
                        mVal = mVal.Replace("(", "");
                        mVal = mVal.Replace(")", "");
                        vertexReal.Add(mVal);
                    }
                }
                
                int V1 = Convert.ToInt32(vertexReal[0]);
                V1--;
                int V2 = Convert.ToInt32(vertexReal[vertexReal.Count - 1]);
                V2--;

                if (V1 == fromV && V2 == toV)
                {
                    resultOut.Add(item);
                }
            }
            // формирование через дуги
            foreach (String item in resultOut)
            {
                List<String> vertexReal = new List<String>();
                Regex regex = new Regex(@"(\d)|(\(\d+\))");
                MatchCollection matches = regex.Matches(item);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        String mVal = match.Value;
                        mVal = mVal.Replace("(", "");
                        mVal = mVal.Replace(")", "");
                        vertexReal.Add(mVal);
                    }
                }

                String resultE = "";
                for (int i = 0; i < vertexReal.Count; i++)
                {
                    int V1 = Convert.ToInt32(vertexReal[i]);
                    V1--;
                    int V2 = 0;
                    if (i + 1 == vertexReal.Count)
                        V2 = Convert.ToInt32(vertexReal[0]);
                    else
                        V2 = Convert.ToInt32(vertexReal[i + 1]);
                    V2--;

                    Char ret = findEdgeAssociatedWith(V1, V2, E);
                    if (ret != Char.MinValue)
                        resultE += ret;
                }

                resultOutE.Add(resultE);
            }
        }

        //заполняет матрицу последовательной смежности
        public void fillSeqMatrix(List<Vertex> V, List<Edge> E, List<String> resultOut)
        {
            for (int i = 0; i < V.Count; i++)
            {
                List<Edge> to = findToVertexEdgesPathes(i, E);
                List<Edge> from = findFromVertexEdgesPathes(i, E);
                foreach (Edge e1 in to)
                {
                    foreach (Edge e2 in from)
                    {
                        if (e1 != e2)
                            resultOut.Add(e1.NAME + "&" + e2.NAME);
                    }
                }
            }
        }

        //заполняет матрицу параллельной смежности 
        public void fillParallMatrix(List<Vertex> V, List<Edge> E, List<String> resultOut)
        {
            for (int i = 0; i < V.Count; i++)
            {
                List<Edge> to = findToVertexEdgesPathes(i, E);
                foreach (Edge e1 in to)
                {
                    foreach (Edge e2 in to)
                    {
                        if (e1 != e2)
                        {
                            if (!resultOut.Contains(e1.NAME + "&" + e2.NAME) && 
                                !resultOut.Contains(e2.NAME + "&" + e1.NAME))
                                    resultOut.Add(e1.NAME + "&" + e2.NAME);
                        }
                    }
                }

                List<Edge> from = findFromVertexEdgesPathes(i, E);
                foreach (Edge e1 in from)
                {
                    foreach (Edge e2 in from)
                    {
                        if (e1 != e2)
                        {
                            if (!resultOut.Contains(e1.NAME + "&" + e2.NAME) &&
                                !resultOut.Contains(e2.NAME + "&" + e1.NAME))
                                    resultOut.Add(e1.NAME + "&" + e2.NAME);
                        }
                    }
                }
            }
        }
    }
}