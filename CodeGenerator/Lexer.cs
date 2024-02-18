using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    class Syntaxer
    {
        enum Token      // перечисление всех возможных лексем
        {
            Identifier = 0,
            Semicolon,
            CommentLine,
            FloatNumber,
            AssignOperator,
            GreaterOperator,
            LowerOperator,
            EqualOperator,
            OpenedBracket,
            ClosedBracket,
            CircuitStart,
            CircuitEnd,
            UNKNOWN,        // неизвестная лексема
        }

        string[] names_tokens = {   "Идентификатор", "Точка с запятой", "Комментарий",
                                    "Вещественное число", "Оператор присваивания", "Больше", 
                                    "Меньше", "Равно", "Левая скобка", "Правая скобка", 
                                    "Начало цикла", "Конец цикла", "Неизвестная лексема"};

        class TokenInfo     // класс лексемы
        {
            public Token token_type;        // тип лексемы
            public string token;            // строковое представление
            public int lineNumber;          // номер строки исходного файла
            public int columnNumber;        // номер колонки в строке
            public int level;               // уровень вложенности
            public bool decrease = false;   // признак уменьшения
        }

        const string GREATER = ">";
        const string LOWER = "<";
        const string EQUAL = "=";
        const string ASSIGN_OPERATOR = ":=";
        const string OPENED_BRACKET = "(";
        const string CLOSED_BRACKET = ")";
        const string SEMICOLON = ";";
        const string DIGIT_SEP = ".";
        const string FOR_KEYWORD = "for";
        const string DO_KEYWORD = "do";
        const string COMMENT_START = "{";
        const string COMMENT_END = "}";
        const string WHITE_SPACE = " ";

        string digits = "0123456789";
        string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_"; // alphabet for identifier

        List<TokenInfo> tokens = new List<TokenInfo>();         // список лексем
        List<TokenInfo> stageTokens = new List<TokenInfo>();    // временный список на шаге обработки
        List<TokenInfo> markedTokens = new List<TokenInfo>();

        string[] InputsAfterStage1;     // входные строки после шага 1 (для отслеживания ошибок)
        string[] InputsAfterStage2;     // входные строки после шага 2
        string[] InputsAfterStage3;     // входные строки после шага 3

        List<String> treeView = new List<String>();
        List<String> treeViewError = new List<String>();

        String graphViewGlobal;
        String triadesGlobal;
        String triadesGlobalOptimum;
        int lineCount;

        public static string ReplaceFirst(string str, string term, string replace)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(0, position) + replace + str.Substring(position + term.Length);
            return str;
        }

        public void ShowCorrectTokens()
        {
            Console.WriteLine("КОРРЕКТНЫЕ ЛЕКСЕМЫ:");
            tokens.ForEach(delegate (TokenInfo token) {
                if (token.token_type != Token.UNKNOWN)
                {
                    Console.WriteLine("{0, 30} \t---\t {1, 50}", names_tokens[(int)token.token_type], token.token);
                }
            });
        }

        public void ShowErrorTokens()
        {
            Console.WriteLine("ОШИБОЧНЫЕ ЛЕКСЕМЫ:");
            tokens.ForEach(delegate (TokenInfo token) {
                if (token.token_type == Token.UNKNOWN)
                {
                    Console.WriteLine("{0, 30} \t---\t {1, 50}", names_tokens[(int)token.token_type], token.token);
                }
            });
        }

        private int NumberComparator(int lVal, int rVal)
        {
            if (lVal > rVal)
            {
                return 1;
            }
            else if (lVal == rVal)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        private int NumberComparatorReverse(int lVal, int rVal)
        {
            if (lVal < rVal)
            {
                return 1;
            }
            else if (lVal == rVal)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        private int Find_F_NonTermanal(List<TokenInfo> partTokensLine, String parView, String FLabel, int level, 
                                        out String resultView, out List<TokenInfo> T_Tokens, 
                                        out List<TokenInfo> F_Tokens) // поиск F нетерминала
        {
            TokenInfo lastToken, firstToken;
            String graphView = parView;
            T_Tokens = new List<TokenInfo>();
            F_Tokens = new List<TokenInfo>();

            firstToken = partTokensLine.First();
            lastToken = partTokensLine.Last();
            if (firstToken.token_type == Token.Identifier && ((lastToken.token_type == Token.Identifier) || (lastToken.token_type == Token.FloatNumber))) // a := a (терминал)
            {
                TokenInfo middleToken = partTokensLine.ElementAt(partTokensLine.IndexOf(firstToken) + 1);
                if (partTokensLine.Count == 3 && middleToken.token_type == Token.AssignOperator)
                {
                    if (firstToken.decrease)
                        firstToken.level = level - 1;
                    else
                        firstToken.level = level;
                    if (lastToken.decrease)
                        lastToken.level = level - 1;
                    else
                        lastToken.level = level;
                    if (middleToken.decrease)
                        middleToken.level = level - 1;
                    else
                        middleToken.level = level;
                    markedTokens.Add(firstToken);
                    markedTokens.Add(lastToken);
                    markedTokens.Add(middleToken);
                    graphView = ReplaceFirst(graphView, FLabel, firstToken.token + WHITE_SPACE + ASSIGN_OPERATOR + WHITE_SPACE + lastToken.token);
                }
                else
                {
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала F: некорректный оператор присваивания");
                    resultView = "";
                    return 1;
                }
            }
            else if (firstToken.token_type == Token.CircuitStart)
            {
                int startIndex = partTokensLine.IndexOf(firstToken);
                int balanced_bracket = 0;
                int pos_open_bracket = 0;
                int pos_closed_bracket = 0;
                int i = 1;
                String tmp = firstToken.token + WHITE_SPACE;
                while ((startIndex + i < partTokensLine.Count) && partTokensLine.ElementAt(startIndex + i).token_type != Token.CircuitEnd)
                {
                    TokenInfo curToken = partTokensLine.ElementAt(startIndex + i);
                    if (curToken.token_type == Token.OpenedBracket)
                    {
                        if (pos_open_bracket == 0)
                            pos_open_bracket = i;
                        balanced_bracket++;
                        curToken.level = level;
                        markedTokens.Add(curToken);
                        tmp += curToken.token + WHITE_SPACE;
                    }
                    else if (curToken.token_type == Token.ClosedBracket)
                    {
                        if (pos_closed_bracket == 0)
                            pos_closed_bracket = i;
                        balanced_bracket--;
                        curToken.level = level;
                        markedTokens.Add(curToken);
                        tmp += curToken.token + WHITE_SPACE;
                    }
                    else
                    {
                        T_Tokens.Add(curToken);
                    }
                    i++;
                }
                if ((startIndex + i < partTokensLine.Count) && partTokensLine.ElementAt(startIndex + i).token_type == Token.CircuitEnd &&
                    balanced_bracket == 0 && pos_open_bracket == startIndex + 1 &&
                    pos_closed_bracket == startIndex + i - 1)     // корректное определение цикла
                {
                    firstToken.level = level;
                    TokenInfo lstToken = partTokensLine.ElementAt(startIndex + i);
                    lstToken.level = level;
                    markedTokens.Add(firstToken);
                    markedTokens.Add(lstToken);

                    tmp += lstToken.token + WHITE_SPACE;
                    tmp = ReplaceFirst(tmp, "(", "( *");
                    tmp += "*";
                    tmp += Environment.NewLine + "(" + Environment.NewLine;
                    tmp += " -> T";
                    tmp += Environment.NewLine;
                    tmp += " -> " + FLabel;
                    tmp += Environment.NewLine + ")";
                    graphView = ReplaceFirst(graphView, FLabel, tmp);
                }
                else
                {
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала F: неверное тело цикла");
                    resultView = "";
                    return 1;
                }

                bool need_dec = true;
                for (int j = startIndex + i + 1; j < partTokensLine.Count; j++)
                {
                    TokenInfo curElem = partTokensLine.ElementAt(j);
                    if (curElem.token_type == Token.OpenedBracket)
                        need_dec = false;
                    else if (curElem.token_type == Token.ClosedBracket)
                        need_dec = true;

                    if (need_dec)
                        curElem.decrease = true;
                    F_Tokens.Add(curElem);
                }
            }
            else
            {
                if (firstToken.token_type == Token.CircuitStart)
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала F: неверное завершение оператора цикла");
                else if (firstToken.token_type == Token.Identifier)
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала F: неверное использование оператора присваивания");
                else
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала F: неизвестная начальная конструкция");
                resultView = "";
                return 1;
            }
            resultView = graphView;
            return 0;
        }

        private int Find_T_NonTermanal(List<TokenInfo> partTokensLine, String parView, int level, 
                                        out String resultView, out List<TokenInfo> F1_Tokens, 
                                        out List<TokenInfo> E_Tokens,
                                        out List<TokenInfo> F2_Tokens) // поиск T нетерминала
        {
            TokenInfo firstToken;
            String graphView = parView;
            F1_Tokens = new List<TokenInfo>();
            F2_Tokens = new List<TokenInfo>();
            E_Tokens = new List<TokenInfo>();

            firstToken = partTokensLine.First();
            TokenInfo curToken;
            int cnt_semicolon = 0;
            int startIndex = partTokensLine.IndexOf(firstToken);
            for (int k = startIndex; k < partTokensLine.Count; k++)
            {
                curToken = partTokensLine.ElementAt(k);

                if (curToken.token_type != Token.Semicolon && cnt_semicolon == 0)
                {
                    F1_Tokens.Add(curToken);
                }
                else if (curToken.token_type != Token.Semicolon && cnt_semicolon == 1)
                {
                    E_Tokens.Add(curToken);
                }
                else if (curToken.token_type != Token.Semicolon && cnt_semicolon == 2)
                {
                    F2_Tokens.Add(curToken);
                }
                else if (curToken.token_type == Token.Semicolon)
                {
                    curToken.level = level + 1;
                    markedTokens.Add(curToken);
                    cnt_semicolon++;
                }
            }
            if (cnt_semicolon == 2)
            {
                String tmp = "*" + SEMICOLON + "*" + SEMICOLON + "*";
                tmp += Environment.NewLine + "(" + Environment.NewLine;
                tmp += " -> F1";
                tmp += Environment.NewLine;
                tmp += " -> E";
                tmp += Environment.NewLine;
                tmp += " -> F2";
                tmp += Environment.NewLine + ")";
                graphView = ReplaceFirst(graphView, "T", tmp);
            }
            else
            {
                treeViewError.Add("Отсутствует или некорректна лексема для нетерминала T: неверное кол-во точек с запятой");
                resultView = "";
                return 1;
            }

            resultView = graphView;
            return 0;
        }

        private int Find_E_NonTermanal(List<TokenInfo> partTokensLine, String parView, int level, out String resultView) // поиск E нетерминала
        {
            TokenInfo firstToken;
            String graphView = parView;

            firstToken = partTokensLine.First();
            int startIndex = partTokensLine.IndexOf(firstToken);
            if (firstToken.token_type == Token.Identifier)
            {
                TokenInfo operatorToken = partTokensLine.ElementAt(startIndex + 1);
                TokenInfo lastToken = partTokensLine.ElementAt(startIndex + 2);
                if (partTokensLine.Count == 3 && (operatorToken.token_type == Token.GreaterOperator
                    || (operatorToken.token_type == Token.LowerOperator)
                    || (operatorToken.token_type == Token.EqualOperator))
                    && ((lastToken.token_type == Token.Identifier) 
                    || (lastToken.token_type == Token.FloatNumber)))
                {
                    firstToken.level = level;
                    operatorToken.level = level;
                    lastToken.level = level;
                    markedTokens.Add(firstToken);
                    markedTokens.Add(operatorToken);
                    markedTokens.Add(lastToken);

                    String tmp = firstToken.token + WHITE_SPACE + operatorToken.token + WHITE_SPACE + lastToken.token;
                    graphView = ReplaceFirst(graphView, "E", tmp);
                }
                else
                {
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала E: некорректный выражение сравнения");
                    resultView = "";
                    return 1;
                }
            }
            else
            {
                treeViewError.Add("Отсутствует или некорректна лексема для нетерминала E: некорректное начало, ожидается идентификатор");
                resultView = "";
                return 1;
            }

            resultView = graphView;
            return 0;
        }

        private int recursiveFNonTerminal(List<TokenInfo> partTokensLine, int level)
        {
            // поиск F нетерминала
            String outView;
            List<TokenInfo> T_NonTerminal;
            List<TokenInfo> F_NonTerminal;
            List<TokenInfo> currentTokens = new List<TokenInfo>();

            if (Find_F_NonTermanal(partTokensLine, graphViewGlobal, "F", level + 1, out outView, out T_NonTerminal, out F_NonTerminal) == 1)
            {
                return 1;
            }
            else
            { 
                graphViewGlobal = outView;
            }

            int cur_level = 0;
            if (F_NonTerminal.Count > 0 && T_NonTerminal.Count > 0)
                cur_level = level + 1;
            else if (F_NonTerminal.Count > 0)
                cur_level = level;
            else
                cur_level = -1;

            if (cur_level != -1)
                if (recursiveFNonTerminal(F_NonTerminal, cur_level) == 1)
                    return 1;

            List<TokenInfo> F1_NonTerminal;
            List<TokenInfo> F2_NonTerminal;
            List<TokenInfo> E_NonTerminal;
            List<TokenInfo> currentTTokens = T_NonTerminal;
            if (T_NonTerminal.Count > 0)
            {
                if (Find_T_NonTermanal(currentTTokens, graphViewGlobal, level + 1, out outView, out F1_NonTerminal, out E_NonTerminal, out F2_NonTerminal) == 1)
                {
                    return 1;
                }
                else
                    graphViewGlobal = outView;

                List<TokenInfo> currentETokens = E_NonTerminal;
                if (Find_E_NonTermanal(currentETokens, graphViewGlobal, level + 2, out outView) == 1)
                {
                    return 1;
                }
                else
                    graphViewGlobal = outView;

                if (F1_NonTerminal.Count > 0)
                { 
                    List<TokenInfo> currentF1Tokens = F1_NonTerminal;
                    if (Find_F_NonTermanal(currentF1Tokens, graphViewGlobal, "F1", level + 2, out outView, out T_NonTerminal, out F_NonTerminal) == 1)
                    {
                        return 1;
                    }
                    else
                        graphViewGlobal = outView;
                }
                else
                {
                    graphViewGlobal = ReplaceFirst(graphViewGlobal, "F1", "");
                }

                if (F2_NonTerminal.Count > 0)
                {
                    List<TokenInfo> currentF2Tokens = F2_NonTerminal;
                    if (Find_F_NonTermanal(currentF2Tokens, graphViewGlobal, "F2", level + 2, out outView, out T_NonTerminal, out F_NonTerminal) == 1)
                    {
                        return 1;
                    }
                    else
                        graphViewGlobal = outView;
                }
                else
                {
                    graphViewGlobal = ReplaceFirst(graphViewGlobal, "F2", "");
                }
            }

            return 0;
        }

        public void ShowSyntaxTree()
        {
            Console.WriteLine();
            int number = 1;
            foreach (String data in treeView)
            {
                if (data.Length > 0)
                {
                    Console.WriteLine("Строка № {0}: {1}", number, data);
                }
                number++;
            }
        }

        public void ShowSyntaxError()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            int number = 1;
            foreach(String data in treeViewError)
            {
                if (data.Length > 0)
                {
                    Console.WriteLine("Строка № {0}: {1}", number, data);
                }
                number++;
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void SyntaxTree()
        {
            List<TokenInfo> sortedTokens = new List<TokenInfo>(tokens);
            sortedTokens.Sort(new Comparison<TokenInfo>((x, y) => NumberComparator(x.lineNumber, y.lineNumber))); // отсортированные лексемы по номеру строки

            for (int idx = sortedTokens.First().lineNumber; idx <= sortedTokens.Last().lineNumber; idx += 1) // перебор построчный
            {
                String graphView = "";
                TokenInfo lastToken;

                List<TokenInfo> partTokensLine = sortedTokens.Where(b => b.lineNumber == idx && b.token_type != Token.CommentLine).ToList(); // токены только первой строки
                partTokensLine.Sort(new Comparison<TokenInfo>((x, y) => NumberComparator(x.columnNumber, y.columnNumber))); // отсортированные лексемы по номеру колонки

                graphView = "S";
                lastToken = partTokensLine.Last();
                // поиск S нетереминала
                if (lastToken.token_type == Token.Semicolon)
                {
                    lastToken.level = 0; // корень
                    markedTokens.Add(lastToken);
                    graphView += " -> " + Environment.NewLine + "F" + WHITE_SPACE + lastToken.token;
                    partTokensLine.RemoveAt(partTokensLine.IndexOf(lastToken)); // удаление лексемы

                }
                else
                {
                    treeViewError.Add("Отсутствует или некорректна лексема для нетерминала S: возможно отсутствует точка с запятой");
                    continue;
                }

                graphViewGlobal = graphView;
                int result = recursiveFNonTerminal(partTokensLine, 0);
                if (result == 1)
                {
                    treeView.Add(""); // пустая
                    continue;
                }
                else
                    treeViewError.Add("");  // пустая
                treeView.Add(graphViewGlobal);
            }
            int x = 0;
            x++;
        }

        public void showTriadsOptimize()
        {
            Console.WriteLine();
            Console.WriteLine(triadesGlobalOptimum);
        }

        public void showTriads()
        {
            Console.WriteLine();
            Console.WriteLine(triadesGlobal);
        }

        public void buildTriads()       // построение триад
        {
            int num = 1;
            triadesGlobal = num + WHITE_SPACE;
            triadesGlobalOptimum = triadesGlobal;
            for (int idx = 1; idx <= lineCount; idx += 1) // перебор построчный
            {
                List<TokenInfo> partTokensLine = markedTokens.Where(b => b.lineNumber == idx).ToList(); // отбор очередной строки

                List<TokenInfo> sortedTokens = new List<TokenInfo>(partTokensLine);
                sortedTokens.Sort(new Comparison<TokenInfo>((x, y) => NumberComparatorReverse(x.level, y.level)));

                int maxLevel = sortedTokens.First().level;
                String loopIndexLine = "";
                do
                {
                    List<TokenInfo> levelTokensLine = sortedTokens.Where(b => b.level == maxLevel).ToList(); // отбор токенов очередного уровня
                    List<TokenInfo> sortedLevelTokens = new List<TokenInfo>(levelTokensLine);
                    sortedLevelTokens.Sort(new Comparison<TokenInfo>((x, y) => NumberComparator(x.columnNumber, y.columnNumber)));
                    int cnt_semicolon = 0;

                    if (loopIndexLine.Length > 0)
                    {
                        bool shDelete = false;
                        int cntDel = 0;
                        int fromDelIndex = -1;
                        for (int k = 0; k < sortedLevelTokens.Count; k++) // ищем цикл
                        {
                            TokenInfo curToken = sortedLevelTokens.ElementAt(k);
                            if (curToken.token_type == Token.CircuitStart)
                            {
                                triadesGlobal += "LOOP" + OPENED_BRACKET + loopIndexLine.Substring(0, loopIndexLine.Length - 1) + CLOSED_BRACKET;
                                triadesGlobalOptimum += "LOOP" + OPENED_BRACKET + loopIndexLine.Substring(0, loopIndexLine.Length - 1) + CLOSED_BRACKET;
                                loopIndexLine = "";
                                num++;
                                triadesGlobal += Environment.NewLine + num + WHITE_SPACE;
                                triadesGlobalOptimum += Environment.NewLine + num + WHITE_SPACE;
                                fromDelIndex = k;
                                cntDel++;
                                shDelete = true;
                            }
                            else if (curToken.token_type == Token.CircuitEnd)
                            {
                                break;
                            }
                            if (shDelete)
                            {
                                cntDel++;
                            }
                        }
                        if (fromDelIndex != -1)
                            sortedLevelTokens.RemoveRange(fromDelIndex, cntDel - 1);
                    }

                    foreach(TokenInfo curToken in sortedLevelTokens)
                    {
                        if (curToken.token_type == Token.AssignOperator ||
                            curToken.token_type == Token.GreaterOperator ||
                            curToken.token_type == Token.LowerOperator ||
                            curToken.token_type == Token.EqualOperator)
                        {
                            int index = sortedLevelTokens.IndexOf(curToken);
                            TokenInfo leftToken = sortedLevelTokens.ElementAt(index - 1);
                            TokenInfo rightToken = sortedLevelTokens.ElementAt(index + 1);
                            triadesGlobal += curToken.token + OPENED_BRACKET + leftToken.token + "," + rightToken.token + CLOSED_BRACKET;
                            if (leftToken.token != rightToken.token)
                                triadesGlobalOptimum += curToken.token + OPENED_BRACKET + leftToken.token + "," + rightToken.token + CLOSED_BRACKET;
                            if (cnt_semicolon <= 2)
                            {
                                loopIndexLine += num + ",";
                                if (cnt_semicolon == 2)
                                    cnt_semicolon++;
                            }
                            num++;
                            triadesGlobal += Environment.NewLine + num + WHITE_SPACE;
                            if (leftToken.token != rightToken.token)
                                triadesGlobalOptimum += Environment.NewLine + num + WHITE_SPACE;

                        }
                        else if (curToken.token_type == Token.Semicolon)
                        {
                            cnt_semicolon++;
                        }
                    }
                } while (maxLevel-- != 0);
            }
            triadesGlobal += "EXIT";
            triadesGlobalOptimum += "EXIT";
        }

        public void generateLexems(string[] inputString)        // главный метод
        {
            int startLexem, endLexem;
            InputsAfterStage1 = new String[inputString.Length];
            InputsAfterStage2 = new String[inputString.Length];
            InputsAfterStage3 = new String[inputString.Length];

            lineCount = inputString.Length;

            int numLine = 0;
            // поиск комментариев, открывающейся/закрывающейся скобок, операторов сравнений, точки с запятой, оператор присваивания
            foreach (string line in inputString)
            {
                for (int i = 0; i < line.Length;)
                {
                    if (line.Substring(i, 1).Equals(COMMENT_START))
                    {
                        startLexem = i;
                        while (!line.Substring(i, 1).Equals(COMMENT_END)) { i++; };
                        endLexem = i + 1;
                        i += 1;
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.CommentLine, columnNumber = startLexem, 
                                                                token = line.Substring(startLexem, endLexem - startLexem) };
                        stageTokens.Add(newToken);
                    }
                    else if (line.Substring(i, 1).Equals(OPENED_BRACKET))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.OpenedBracket, columnNumber = i,
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else if (line.Substring(i, 1).Equals(CLOSED_BRACKET))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.ClosedBracket, columnNumber = i, 
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else if (line.Substring(i, 1).Equals(SEMICOLON))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.Semicolon, columnNumber = i, 
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else if ((i + 1) < line.Length && line.Substring(i, 2).Equals(ASSIGN_OPERATOR))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.AssignOperator, columnNumber = i, 
                                                                token = line.Substring(i, 2) };
                        stageTokens.Add(newToken);
                        i += 2;
                    }
                    else if (line.Substring(i, 1).Equals(LOWER))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.LowerOperator, columnNumber = i, 
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else if (line.Substring(i, 1).Equals(GREATER))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.GreaterOperator, columnNumber = i, 
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else if (line.Substring(i, 1).Equals(EQUAL))
                    {
                        TokenInfo newToken = new TokenInfo() {  token_type = Token.EqualOperator, columnNumber = i, 
                                                                token = line.Substring(i, 1) };
                        stageTokens.Add(newToken);
                        i += 1;
                    }
                    else
                    {
                        // unknown
                        i += 1;
                    }
                }

                string changedString = line;
                foreach (TokenInfo token in stageTokens)
                {
                    changedString = ReplaceFirst(changedString, token.token, string.Join("", Enumerable.Repeat(WHITE_SPACE, token.token.Length)));
                    token.lineNumber = numLine + 1;
                    tokens.Add(token);
                }
                InputsAfterStage1[numLine] = changedString;
                stageTokens.Clear();

                numLine++;
            }

            numLine = 0;
            // поиск идентификаторов и ключевых слов и некорректных идентификаторов
            foreach (string line in InputsAfterStage1)
            {
                for (int i = 0; i < line.Length;)
                {
                    if (letters.Contains(line[i]))
                    {
                        startLexem = i;
                        bool correct = true;
                        while (i < line.Length && !line.Substring(i, 1).Equals(WHITE_SPACE))
                        {
                            if (!(letters.Contains(line.Substring(i, 1)) || digits.Contains(line.Substring(i, 1))))
                            {
                                correct = false;
                            }
                            i++;
                        }
                        endLexem = i;
                        i += 1;
                        string newRec = line.Substring(startLexem, endLexem - startLexem);
                        TokenInfo newToken;
                        if (correct)
                        {
                            if (newRec.Equals(FOR_KEYWORD))
                            {
                                newToken = new TokenInfo() { token_type = Token.CircuitStart, columnNumber = startLexem,
                                                             token = newRec };
                            }
                            else if (newRec.Equals(DO_KEYWORD))
                            {
                                newToken = new TokenInfo() { token_type = Token.CircuitEnd, columnNumber = startLexem,
                                                            token = newRec };
                            }
                            else
                            {
                                newToken = new TokenInfo() {    token_type = Token.Identifier, columnNumber = startLexem, 
                                                                token = newRec };
                            }
                        }
                        else
                        {
                            newToken = new TokenInfo() {    token_type = Token.UNKNOWN, columnNumber = startLexem, 
                                                            token = newRec };
                        }
                        stageTokens.Add(newToken);
                    }
                    else
                    {
                        // unknown
                        i += 1;
                    }
                }

                string changedString = line;
                foreach (TokenInfo token in stageTokens)
                {
                    changedString = ReplaceFirst(changedString, token.token, string.Join("", Enumerable.Repeat(WHITE_SPACE, token.token.Length)));
                    token.lineNumber = numLine + 1;
                    tokens.Add(token);
                }
                InputsAfterStage2[numLine] = changedString;
                stageTokens.Clear();

                numLine++;
            }

            numLine = 0;
            // поиск чисел (вещественные)
            foreach (string line in InputsAfterStage2)
            {
                for (int i = 0; i < line.Length;)
                {
                    
                    if (digits.Contains(line[i]))
                    {
                        startLexem = i;
                        bool correct = true;
                        int sep_count = 0;
                        while (i < line.Length && !line.Substring(i, 1).Equals(WHITE_SPACE))
                        {
                            if (!(digits.Contains(line.Substring(i, 1))))
                            {
                                if (!line.Substring(i, 1).Equals(DIGIT_SEP))
                                {
                                    correct = false;
                                }
                                else
                                {
                                    if (sep_count > 0)
                                    {
                                        correct = false;
                                    }
                                    sep_count += 1;
                                }
                            }
                            i++;
                        }
                        endLexem = i;
                        i += 1;
                        string newRec = line.Substring(startLexem, endLexem - startLexem);
                        TokenInfo newToken;
                        if (correct)
                        {
                            if (sep_count == 1)
                            {
                                newToken = new TokenInfo() {    token_type = Token.FloatNumber, columnNumber = startLexem, 
                                                                token = newRec };
                            }
                            else
                            {
                                newToken = new TokenInfo() {    token_type = Token.UNKNOWN, columnNumber = startLexem, 
                                                                token = newRec };
                            }
                        }
                        else
                        {
                            newToken = new TokenInfo() {    token_type = Token.UNKNOWN, columnNumber = startLexem, 
                                                            token = newRec };
                        }
                        stageTokens.Add(newToken);
                    }
                    else
                    {
                        // unknown
                        i += 1;
                    }
                }

                string changedString = line;
                foreach (TokenInfo token in stageTokens)
                {
                    changedString = ReplaceFirst(changedString, token.token, string.Join("", Enumerable.Repeat(WHITE_SPACE, token.token.Length)));
                    token.lineNumber = numLine + 1;
                    tokens.Add(token);
                }
                InputsAfterStage3[numLine] = changedString;
                stageTokens.Clear();

                numLine++;
            }

            // неизвестные лексемы
            foreach (string line in InputsAfterStage3)
            {
                string lineTr = line.Trim();
                if (lineTr.Length > 0)          // не пусто (значит неизвестная лексема)
                {
                    TokenInfo newToken = new TokenInfo() { token_type = Token.UNKNOWN, token = lineTr };
                    tokens.Add(newToken);
                }
            }
        }
    }
}
