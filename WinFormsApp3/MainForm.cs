using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class MainForm : Form
    {
        // Aktuální nastavení nástroje
        private Color currentColor = Color.Black; // Barva pera
        private float penWidth = 2;               // Tloušťka čáry
        private DashStyle dashStyle = DashStyle.Solid; // Styl čáry

        // Proměnné pro kreslení
        private bool isDrawing = false;
        private Point startPoint;
        private Point endPoint;

        // Uložené tvary
        private List<Shape> shapes = new List<Shape>();

        // Aktuálně vybraný nástroj
        private string currentTool = "Line"; // Line, Rectangle, Ellipse, Eraser, Fill

        public MainForm()
        {
            // Nastavení okna
            this.Text = "Jednoduchý Malování";
            this.Size = new Size(1000, 700);
            this.DoubleBuffered = true;
            this.BackColor = Color.White;

            InitializeUI(); // Vytvoření ovládacích prvků

            // Připojení událostí myši a vykreslení
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
            this.Paint += MainForm_Paint;
        }

        // Ovládací prvky pro výběr nástroje, tloušťky, stylu čáry a barvy
        private void InitializeUI()
        {
            // Výběr nástroje (čára, obdélník, elipsa, guma, výplň)
            ComboBox toolBox = new ComboBox() { Left = 10, Top = 10 };
            toolBox.Items.AddRange(new string[] { "Line", "Rectangle", "Ellipse", "Eraser", "Fill" });
            toolBox.SelectedIndex = 0;
            toolBox.SelectedIndexChanged += (s, e) => currentTool = toolBox.SelectedItem.ToString();
            this.Controls.Add(toolBox);

            // Výběr stylu čáry
            ComboBox dashBox = new ComboBox() { Left = 120, Top = 10 };
            dashBox.Items.AddRange(new string[] { "Solid", "Dash", "Dot" });
            dashBox.SelectedIndex = 0;
            dashBox.SelectedIndexChanged += (s, e) =>
            {
                switch (dashBox.SelectedItem.ToString())
                {
                    case "Dash": dashStyle = DashStyle.Dash; break;
                    case "Dot": dashStyle = DashStyle.Dot; break;
                    default: dashStyle = DashStyle.Solid; break;
                }
            };
            this.Controls.Add(dashBox);

            // Výběr tloušťky pera
            NumericUpDown penWidthBox = new NumericUpDown() { Left = 230, Top = 10, Width = 50, Value = 2, Minimum = 1, Maximum = 10 };
            penWidthBox.ValueChanged += (s, e) => penWidth = (float)penWidthBox.Value;
            this.Controls.Add(penWidthBox);

            // Tlačítko pro výběr barvy
            Button colorButton = new Button() { Left = 300, Top = 10, Text = "Barva" };
            colorButton.Click += (s, e) =>
            {
                ColorDialog cd = new ColorDialog();
                if (cd.ShowDialog() == DialogResult.OK)
                    currentColor = cd.Color;
            };
            this.Controls.Add(colorButton);
        }

        // Kliknutí myší – začátek kreslení nebo výplň
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTool == "Fill")
            {
                // Výplň barvou – vytvoří bitmapu a zaplní kliknutou oblast
                using Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                using Graphics g = Graphics.FromImage(bmp);
                DrawAllShapes(g);
                FloodFill(bmp, e.Location, bmp.GetPixel(e.X, e.Y), currentColor);
                using Graphics g2 = this.CreateGraphics();
                g2.DrawImage(bmp, 0, 0);
                return;
            }

            // Začátek kreslení tvaru
            isDrawing = true;
            startPoint = e.Location;
        }

        // Pohyb myší při kreslení – aktualizace koncového bodu
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                endPoint = e.Location;
                this.Invalidate(); // Vyvolá překreslení
            }
        }

        // Uvolnění myši – ukončení kreslení a uložení tvaru
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;

            endPoint = e.Location;

            // Vytvoření tvaru podle vybraného nástroje
            Shape shape = currentTool switch
            {
                "Rectangle" => new RectangleShape(startPoint, endPoint, currentColor, penWidth, dashStyle),
                "Ellipse" => new EllipseShape(startPoint, endPoint, currentColor, penWidth, dashStyle),
                "Eraser" => new LineShape(startPoint, endPoint, Color.White, penWidth, DashStyle.Solid),
                _ => new LineShape(startPoint, endPoint, currentColor, penWidth, dashStyle),
            };

            shapes.Add(shape); // Uložení tvaru do seznamu
            isDrawing = false;
            this.Invalidate(); // Překreslit
        }

        // Vykreslení všech tvarů a náhledu aktuálně kresleného
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // Vykreslit všechny uložené tvary
            foreach (var shape in shapes)
                shape.Draw(e.Graphics);

            // Náhled aktuálně kresleného tvaru
            if (isDrawing && currentTool != "Fill")
            {
                Pen previewPen = new Pen(currentTool == "Eraser" ? Color.White : currentColor, penWidth)
                {
                    DashStyle = dashStyle
                };

                switch (currentTool)
                {
                    case "Rectangle":
                        e.Graphics.DrawRectangle(previewPen, GetRectangle(startPoint, endPoint));
                        break;
                    case "Ellipse":
                        e.Graphics.DrawEllipse(previewPen, GetRectangle(startPoint, endPoint));
                        break;
                    default:
                        e.Graphics.DrawLine(previewPen, startPoint, endPoint);
                        break;
                }
            }
        }

        // Pomocná metoda pro vytvoření obdélníku mezi dvěma body
        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        // Vykreslí všechny tvary na daný Graphics
        private void DrawAllShapes(Graphics g)
        {
            foreach (var shape in shapes)
                shape.Draw(g);
        }

        // Výplň oblasti rekurzivním algoritmem (Flood Fill)
        private void FloodFill(Bitmap bmp, Point pt, Color targetColor, Color replacementColor)
        {
            if (targetColor.ToArgb() == replacementColor.ToArgb()) return;
            if (!IsInside(bmp, pt) || bmp.GetPixel(pt.X, pt.Y).ToArgb() != targetColor.ToArgb()) return;

            Queue<Point> q = new Queue<Point>();
            q.Enqueue(pt);

            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                if (!IsInside(bmp, p)) continue;
                if (bmp.GetPixel(p.X, p.Y).ToArgb() != targetColor.ToArgb()) continue;

                bmp.SetPixel(p.X, p.Y, replacementColor);

                q.Enqueue(new Point(p.X + 1, p.Y));
                q.Enqueue(new Point(p.X - 1, p.Y));
                q.Enqueue(new Point(p.X, p.Y + 1));
                q.Enqueue(new Point(p.X, p.Y - 1));
            }
        }

        // Kontrola, jestli je bod uvnitř obrázku
        private bool IsInside(Bitmap bmp, Point pt)
        {
            return pt.X >= 0 && pt.Y >= 0 && pt.X < bmp.Width && pt.Y < bmp.Height;
        }
    }

    // Abstraktní třída pro tvar
    public abstract class Shape
    {
        protected Point Start, End;
        protected Color Color;
        protected float Width;
        protected DashStyle Dash;

        public Shape(Point start, Point end, Color color, float width, DashStyle dash)
        {
            Start = start;
            End = end;
            Color = color;
            Width = width;
            Dash = dash;
        }

        // Každý tvar musí umět vykreslit sám sebe
        public abstract void Draw(Graphics g);
    }

    // Čára
    public class LineShape : Shape
    {
        public LineShape(Point start, Point end, Color color, float width, DashStyle dash)
            : base(start, end, color, width, dash) { }

        public override void Draw(Graphics g)
        {
            using Pen pen = new Pen(Color, Width) { DashStyle = Dash };
            g.DrawLine(pen, Start, End);
        }
    }

    // Obdélník
    public class RectangleShape : Shape
    {
        public RectangleShape(Point start, Point end, Color color, float width, DashStyle dash)
            : base(start, end, color, width, dash) { }

        public override void Draw(Graphics g)
        {
            using Pen pen = new Pen(Color, Width) { DashStyle = Dash };
            g.DrawRectangle(pen, new Rectangle(
                Math.Min(Start.X, End.X),
                Math.Min(Start.Y, End.Y),
                Math.Abs(Start.X - End.X),
                Math.Abs(Start.Y - End.Y)));
        }
    }

    // Elipsa
    public class EllipseShape : Shape
    {
        public EllipseShape(Point start, Point end, Color color, float width, DashStyle dash)
            : base(start, end, color, width, dash) { }

        public override void Draw(Graphics g)
        {
            using Pen pen = new Pen(Color, Width) { DashStyle = Dash };
            g.DrawEllipse(pen, new Rectangle(
                Math.Min(Start.X, End.X),
                Math.Min(Start.Y, End.Y),
                Math.Abs(Start.X - End.X),
                Math.Abs(Start.Y - End.Y)));
        }
    }
}
