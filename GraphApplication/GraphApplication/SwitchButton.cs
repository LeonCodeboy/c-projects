using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DrawingButton
{
    public class SwitchButton : Button
    {
        const string turnOnStr = "Включить";
        const string turnOffStr = "Отключить";
        private bool turnOn;
        public bool isTurnOn
        {
            get
            {
                return turnOn;
            }
            set
            {
                turnOn = value;
                Invalidate();
            }
        }
        public SwitchButton()
        {
            turnOn = false;
            BackColor = Color.IndianRed;
            ForeColor = Color.Black;
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (!turnOn)
                BackColor = Color.Green;
            else
                BackColor = Color.IndianRed;
            turnOn = !turnOn;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            string txt;
            if (!turnOn)
                txt = turnOnStr;
            else
                txt = turnOffStr;
            txt = txt.ToUpper();
            TextRenderer.DrawText(pevent.Graphics, txt, Font, new Point(Width + 3, Height / 2), ForeColor, flags);
        }
    }
}