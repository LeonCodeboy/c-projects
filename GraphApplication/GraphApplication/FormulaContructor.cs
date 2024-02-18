using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphApplication
{
    class FormulaContructor
    {
        Bitmap bitmap;
        Graphics gr;
        Pen blackPen;
        Pen whitePen;
        Font fo;
        Brush br;
        const int sWgt = 30;
        const int sHgt = 30;

        // событие обновления картинки
        public event EventHandler updateBitmap;
        // событие вывода сообщения
        public event EventHandler errorMessage;
        // отрисовка текущей формулы
        public event EventHandler showFormula;

        // по курсору
        PointF cursorPos; // текущая позиция курсора ввода
        const int cursorDefShift = 5; // смещение кусора по умолчанию
        bool cursorVisible = false;
        Bitmap prevState;   // сохранение предыдущего состояния
        PointF prevCursorPos; // сохранение предыдущей позиции курсора

        String currentFormulaString; // текущая формула
        String prevFormulaString; // предыдущая формула
        short currentCharUnderCursor; // символ (индекс) под курсором
        short prevCharUnderCursor; // предыдущий (индекс) под курсором

        public FormulaContructor(int width, int height)
        {
            bitmap = new Bitmap(width, height);
            gr = Graphics.FromImage(bitmap);
            clearSheet();
            blackPen = new Pen(Color.Black);
            blackPen.Width = 2;
            whitePen = new Pen(Color.White);
            whitePen.Width = 2;
            fo = new Font("Courer New", 12);
            br = Brushes.Black;

            currentFormulaString = "";
            prevFormulaString = currentFormulaString;
            currentCharUnderCursor = 0;
            prevCharUnderCursor = 0;

            // нарисовать центр
            int centerX = width / 2;
            int centerY = height / 2;
            blackPen.DashStyle = DashStyle.DashDot;
            cursorPos = new PointF(centerX - (sWgt / 2) + cursorDefShift, centerY);

            System.Timers.Timer cursonTimer = new System.Timers.Timer();
            cursonTimer.Interval = 700;
            cursonTimer.AutoReset = true;
            cursonTimer.Elapsed += (sender, e) => {
                if (cursorVisible)
                {
                    whitePen.DashStyle = DashStyle.Solid;
                    whitePen.Width = 5;
                    gr.DrawLine(whitePen, cursorPos, new PointF(cursorPos.X, cursorPos.Y + cursorDefShift));
                }
                else
                {
                    blackPen.DashStyle = DashStyle.Solid;
                    blackPen.Width = 5;
                    gr.DrawLine(blackPen, cursorPos, new PointF(cursorPos.X, cursorPos.Y + cursorDefShift));
                }
                cursorVisible = !cursorVisible;
                callUpdateBitmapFromSpot(); // выбрасываем событие
            };
            cursonTimer.Start();
        }

        private void callUpdateBitmapFromSpot()
        {
            updateBitmap(this, new SystAnalys_lr1.FCEventArgs());
        }

        private void callErrorMessage(String msg)
        {
            SystAnalys_lr1.FCEventArgs evt = new SystAnalys_lr1.FCEventArgs();
            evt.cParam = msg;
            errorMessage(this, evt);
        }

        private void callShowFormula(String msg)
        {
            SystAnalys_lr1.FCEventArgs evt = new SystAnalys_lr1.FCEventArgs();
            evt.cParam = msg;
            showFormula(this, evt);
        }

        // запоминание предыдущего состояния
        private void memPrevState()
        {
            prevState = (Bitmap)bitmap.Clone();
            prevCursorPos = cursorPos;
            prevFormulaString = currentFormulaString;
            prevCharUnderCursor = currentCharUnderCursor;
        }

        // очистка текущей позиции курсора
        private void clearUnderCursor()
        {
            whitePen.DashStyle = DashStyle.Solid;
            whitePen.Width = 5;
            gr.DrawLine(whitePen, cursorPos, new PointF(cursorPos.X, cursorPos.Y + cursorDefShift));
        }

        private PointF convertCursorForText(PointF point)
        {
            clearUnderCursor();
            PointF nPoint = new PointF(point.X - 7, point.Y - 20);
            return nPoint;
        }

        private void alignRightCursor()
        {
            cursorPos = new PointF(cursorPos.X + 12, cursorPos.Y);
        }

        private void alignLeftCursor()
        {
            cursorPos = new PointF(cursorPos.X - 12, cursorPos.Y);
        }

        public void printTextCharCursorPosition(string sym)
        {
            // сохранение текущей формулы
            if (currentFormulaString.Length <= currentCharUnderCursor)
                currentFormulaString += sym;
            else
                currentFormulaString = currentFormulaString.
                    Replace(currentFormulaString[currentCharUnderCursor], sym[0]);
            // запоминаем прошлое состояние
            memPrevState();
            // предварительная очистка поля для вывода символа
            if (currentCharUnderCursor < currentFormulaString.Length)
            {
                gr.FillRectangle(Brushes.White, 
                    new Rectangle(new Point((int) cursorPos.X - 6, (int) cursorPos.Y - 18), 
                    new Size(13, 16)));
            }
            // вывод текущего символа
            gr.DrawString(sym, fo, Brushes.Black, convertCursorForText(cursorPos));
            // смещение текущей позиции курсора
            alignRightCursor();
            callUpdateBitmapFromSpot();
            callShowFormula(currentFormulaString);
            currentCharUnderCursor++;
        }

        public void leftCursorShift()
        {
            if (currentCharUnderCursor > 0)
            {
                clearUnderCursor();
                // смещение текущей позиции курсора
                alignLeftCursor();
                prevCharUnderCursor = currentCharUnderCursor;
                currentCharUnderCursor--;
                callUpdateBitmapFromSpot();
            }
            else
            {
                callErrorMessage("Невозможно сдвинуть курсор влево");
            }
        }

        public void rightCursorShift()
        {
            if (currentCharUnderCursor < currentFormulaString.Length)
            {
                clearUnderCursor();
                // смещение текущей позиции курсора
                alignRightCursor();
                prevCharUnderCursor = currentCharUnderCursor;
                currentCharUnderCursor++;
                callUpdateBitmapFromSpot();
            }
            else
            {
                callErrorMessage("Невозможно сдвинуть курсор вправо");
            }
        }

        public void undoState()
        {
            if (prevState != null)
            {
                cursorPos = prevCursorPos;
                bitmap = (Bitmap)prevState.Clone();
                gr = Graphics.FromImage(bitmap);
            }
            callUpdateBitmapFromSpot();
        }

        public void deleteCharFromCurrentPos()
        {
            if (currentFormulaString.Length > 0 && currentCharUnderCursor > 0)
            {
                // запоминаем прошлое состояние
                memPrevState();
                // смещение текущей позиции курсора
                clearUnderCursor();
                alignLeftCursor();
                currentCharUnderCursor--;
                // предварительная очистка поля для вывода символа
                if (currentCharUnderCursor < currentFormulaString.Length)
                {
                    gr.FillRectangle(Brushes.White,
                        new Rectangle(new Point((int)cursorPos.X - 6, (int)cursorPos.Y - 18),
                        new Size(13, 16)));
                }
                // отрисовка хвоста текущей формулы
                if (currentCharUnderCursor < currentFormulaString.Length - 1)
                {
                    string strEnd = currentFormulaString.Substring(currentCharUnderCursor + 1);
                    gr.FillRectangle(Brushes.White,
                        new Rectangle(new Point((int)cursorPos.X - 6, (int)cursorPos.Y - 18),
                        new Size(1000, 16)));
                    // нужна корректная отрисовка
                    gr.DrawString(strEnd, fo, Brushes.Black, convertCursorForText(cursorPos));
                }
                currentFormulaString = currentFormulaString.Remove(currentCharUnderCursor, 1);
                callUpdateBitmapFromSpot();
                callShowFormula(currentFormulaString);
            }
            else
            {
                callErrorMessage("Невозможно удалить данный символ!");
            }
        }

        public Bitmap GetBitmap()
        {
            return bitmap;
        }

        public void clearSheet()
        {
            gr.Clear(Color.White);
        }
    }
}
