using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollatzGraph {
    public partial class FormMain : Form {
        private object syncObj = new object();
        private List<Point[]> points = new List<Point[]>();
        private int curveMax = 1;
        private bool abort = false;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.Paint += (s, e) => {
                Graphics g = e.Graphics;

                using Pen p = new Pen(Color.FromArgb(10, Color.White), 2);
                lock(syncObj) {
                    foreach(Point[] pts in points) g.DrawLines(p, pts);
                    g.DrawString($"{curveMax:N0}", this.Font, Brushes.WhiteSmoke, 5, 5);

                }
            };

            //curveMax = 10000;
            //GenerateCurve();

            Task.Run(() => {
                int k = 10;
                while(true) {
                    curveMax += k;
                    if((curveMax > 10000) || (curveMax <= 1)) k *= -1;
                    GenerateCurveSafe();

                    Thread.Sleep(1);
                }
            });

            this.Resize += (_, __) => { abort = true; };
        }

        private void GenerateCurveSafe() {
            lock(syncObj) GenerateCurve();
            this.Invalidate();
        }

        private void GenerateCurve() {
            List<Point> pointsInSequence = new List<Point>();

            points.Clear();

            const double bias = 1.64;
            const double angleStep = Math.PI / 24.0;
            double segmentLength = 5.0;
            double angle;

            int x;
            int y;

            void Reset() {
                x = this.DisplayRectangle.Width / 2;
                y = this.DisplayRectangle.Height;
                angle = Math.PI / 2.0;

                pointsInSequence.Clear();
                pointsInSequence.Add(new Point(x, y));
            }

            void AddPoint(int n) {
                angle += n % 2 == 0 ? angleStep : -angleStep * bias;

                x += (int)(segmentLength * Math.Cos(angle));
                y += (int)(-segmentLength * Math.Sin(angle));

                pointsInSequence.Add(new Point(x, y));
            }

            for(int i = 1; i < curveMax; i++) {
                if(abort) {
                    abort = false;
                    return;
                }

                Reset();
                int n = i;

                do {
                    AddPoint(n);
                    n = Collatz(n);
                } while(n > 1);
                //AddPoint(1);
                //pointsInSequence.Reverse();
                points.Add(pointsInSequence.ToArray());
            }
        }

        private int Collatz(int n) {
            return n % 2 == 0 ? n / 2 : 3 * n + 1;
        }
    }
}