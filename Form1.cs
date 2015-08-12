using System;
using System.Collections.Generic;
// using System.ComponentModel;
// using System.Data;
using System.Drawing;
// using System.Linq;
// using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Printing;

namespace SMEditor
{
    public partial class Form1 : Form
    {
        Graphics graphics;
        List<State> StateList;
        List<Transition> TransitionList;
        Figure SelectedFigure;
        string SelectedTransitionMenu;
        public Form1()
        {
            InitializeComponent();
            graphics = panel1.CreateGraphics();
            StateList = new List<State>();
            TransitionList = new List<Transition>();
            panel1.ContextMenu = new ContextMenu();
            toolStripButton2.Checked = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = true;
            toolStripButton3.Checked = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = true;
            toolStripButton2.Checked = false;
            toolStripButton3.Checked = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = false;
            toolStripButton3.Checked = true;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            SelectedFigure = null;
            panel1.ContextMenu.MenuItems.Clear();
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (toolStripButton1.Checked)
                    StateList.Add(new State(e.X, e.Y, generateNewStateName()));

                if (toolStripButton2.Checked || toolStripButton3.Checked)
                    SelectedFigure = getFigureByPoint(e.Location);
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                SelectedFigure = getFigureByPoint(e.Location);
                if (SelectedFigure != null)
                {
                    if (SelectedFigure is State)
                        createStateContentMenu();
                    if (SelectedFigure is InfPoint)
                        createInfPointContentMenu();
                }
            }
            Draw();
        }

        private void createStateContentMenu()
        {
            ContextMenu menu = new System.Windows.Forms.ContextMenu();
            MenuItem markAsStartItem = new MenuItem("Пометить как начальное");
            markAsStartItem.Click += new System.EventHandler(this.markAsStartClick);
            MenuItem markAsFinalItem = new MenuItem("Пометить как конечное");
            markAsFinalItem.Click += new System.EventHandler(this.markAsFinalClick);
            MenuItem deleteItem = new MenuItem("Удалить");
            deleteItem.Click += new System.EventHandler(this.deleteClick);

            List<Transition> trans = findAllTrasitionsFromState((SelectedFigure as State).Name);
            List<MenuItem> menu_mas = new List<MenuItem>();            
            foreach(Transition t in trans)
            {
                MenuItem[] tr_menu_mas = new MenuItem[3];
                tr_menu_mas[0] = new MenuItem("Переименовать");
                tr_menu_mas[1] = new MenuItem("Добавить точку перегиба");
                tr_menu_mas[2] = new MenuItem("Удалить");
                tr_menu_mas[0].Click += new EventHandler(this.renameTransitionClick);
                tr_menu_mas[1].Click += new EventHandler(this.addInfPointClick);
                tr_menu_mas[2].Click += new EventHandler(this.deleteTransitionClick);
                MenuItem mi = new MenuItem(t.Name, tr_menu_mas);
                mi.Select += new EventHandler(this.transitionMenuSelect);
                menu_mas.Add(mi);
            }
            
            MenuItem t_menu = new MenuItem("Переходы", menu_mas.ToArray());
            

            menu.MenuItems.Add(markAsStartItem);
            menu.MenuItems.Add(markAsFinalItem);
            menu.MenuItems.Add(deleteItem);
            if (menu_mas.Count != 0)
            {
                menu.MenuItems.Add(t_menu);
            }

            panel1.ContextMenu = menu;
        }

        private void deleteTransitionClick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < TransitionList.Count; i++ )
            {
                if ((SelectedFigure as State).Name == TransitionList[i].Start.Name && SelectedTransitionMenu == TransitionList[i].Name)
                { TransitionList.RemoveAt(i); break; }
            }
            Draw();
        }

        private void renameTransitionClick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < TransitionList.Count; i++)
            {
                if ((SelectedFigure as State).Name == TransitionList[i].Start.Name && SelectedTransitionMenu == TransitionList[i].Name)
                {
                    TransitionNameForm tnf = new TransitionNameForm();
                    tnf.ShowDialog();
                    if (Program.NewTransitionName != null)
                    {
                        foreach (Transition t in findAllTrasitionsFromState((SelectedFigure as State).Name))
                        {
                            if (t.Name == Program.NewTransitionName)
                            {
                                Program.NewTransitionName = null;
                                MessageBox.Show("Данное имя уже используется в другом переходе из состояния начала", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    TransitionList[i].Name = Program.NewTransitionName; Program.NewTransitionName = null; break; }
            }
            Draw();
        }

        private void addInfPointClick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < TransitionList.Count; i++)
            {
                if ((SelectedFigure as State).Name == TransitionList[i].Start.Name && SelectedTransitionMenu == TransitionList[i].Name)
                { TransitionList[i].InfList.Add(new InfPoint((int)((TransitionList[i].Start.X + TransitionList[i].End.X) / 2), (int)((TransitionList[i].Start.Y + TransitionList[i].End.Y) / 2))); break; }
            }
            Draw();
        }

        private void transitionMenuSelect(object sender, System.EventArgs e)
        {
            SelectedTransitionMenu = (sender as MenuItem).Text;
        }

        private void createInfPointContentMenu()
        {
            ContextMenu menu = new System.Windows.Forms.ContextMenu();
            MenuItem deleteItem = new MenuItem("Удалить");
            deleteItem.Click += new System.EventHandler(this.deleteClick);

            menu.MenuItems.Add(deleteItem);

            panel1.ContextMenu = menu;
        }

        private void markAsStartClick(object sender, System.EventArgs e)
        {
            if (getStartState() != null && getStartState().Name != (SelectedFigure as State).Name)
            {
                MessageBox.Show("Начальное состояние уже выбрано.\nЕсли Вы хотите выбрать другое начальное состояние, сначала снимите отметку с первого", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach(State s in StateList)
                if (s.Name == (SelectedFigure as State).Name)
                {
                    SelectedFigure = null;
                    s.isStartState ^= !s.isFinalState;
                    break;
                }
            Draw();
        }

        private void markAsFinalClick(object sender, System.EventArgs e)
        {
            foreach (State s in StateList)
                if (s.Name == (SelectedFigure as State).Name)
                {
                    SelectedFigure = null;
                    s.isFinalState ^= !s.isStartState;
                    break;
                }
            Draw();
        }
        
        private void deleteClick(object sender, System.EventArgs e)
        {
            if (SelectedFigure is State)
            {
                int name = (SelectedFigure as State).Name;
                DialogResult = MessageBox.Show("Сместить оставшиеся индексы состояний?", "Удаление состояния S" + name, MessageBoxButtons.YesNoCancel);
                if (DialogResult != System.Windows.Forms.DialogResult.Cancel)
                {
                    for (int i = 0; i < StateList.Count; i++)
                        if (StateList[i].Name == (SelectedFigure as State).Name)
                        {
                            SelectedFigure = null;
                            int j = 0;
                            while (j < TransitionList.Count)
                            {
                                for (j = 0; j < TransitionList.Count; j++)
                                {
                                    if (TransitionList[j].Start.Name == StateList[i].Name || TransitionList[j].End.Name == StateList[i].Name)
                                    {
                                        TransitionList.RemoveAt(j);
                                        break;
                                    }
                                }
                            }
                            StateList.RemoveAt(i);
                            break;
                        }
                }
                if (DialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (State s in StateList)
                        if (s.Name > name)
                            s.Name--;
                }
            }
            if (SelectedFigure is InfPoint)
            {
                foreach (Transition t in TransitionList)
                    for(int i=0; i<t.InfList.Count; i++)
                    {
                        if (SelectedFigure.X == t.InfList[i].X && SelectedFigure.Y == t.InfList[i].Y)
                            t.InfList.RemoveAt(i);
                    }
            }
            
            Draw();
        }

        private void Draw()
        {
            graphics.Clear(panel1.BackColor);
            DrawStates();
            DrawTransitions();
        }

        private void DrawStates()
        {
            foreach (State s in StateList)
            {
                Pen pen = new Pen(Color.Black);
                if (s.isStartState) pen.Color = Color.Blue;
                if (s.isFinalState) pen.Color = Color.Red;
                graphics.DrawEllipse(pen, new Rectangle(new Point(s.X - 15, s.Y - 15), new Size(30, 30)));
                graphics.DrawString("S" + s.Name, new Font("Verdana", 6, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(s.X - 6, s.Y - 4));
            }
        }

        private void DrawTransitions()
        {
            int EndX;
            int StartX;
            foreach (Transition t in TransitionList)
            {
                StartX = t.Start.X;
                EndX = t.End.X;
                Pen pen = new Pen(Color.Black);
                int count = t.InfList.Count;
                if (count > 0)
                {
                    if (StartX > t.InfList[0].X)
                        StartX -= 15;
                    else
                        StartX += 15;

                    if (EndX > t.InfList[count - 1].X)
                        EndX -= 15;
                    else
                        EndX += 15;

                    graphics.DrawLine(pen, new Point(StartX, t.Start.Y), new Point(t.InfList[0].X, t.InfList[0].Y));
                    for (int i = 1; i < count; i++)
                        graphics.DrawLine(pen, new Point(t.InfList[i - 1].X, t.InfList[i - 1].Y), new Point(t.InfList[i].X, t.InfList[i].Y));
                    pen.EndCap = LineCap.ArrowAnchor;
                    graphics.DrawLine(pen, new Point(t.InfList[count - 1].X, t.InfList[count - 1].Y), new Point(EndX, t.End.Y));

                    if (count % 2 == 1)
                        graphics.DrawString(t.Name, new Font("Verdana", 6, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(t.InfList[count / 2].X, t.InfList[count / 2].Y - 20));
                    else
                        graphics.DrawString(t.Name, new Font("Verdana", 6, FontStyle.Bold), new SolidBrush(Color.Black), new PointF((t.InfList[count / 2].X + t.InfList[count / 2 - 1].X) / 2, (t.InfList[count / 2].Y + t.InfList[count / 2 - 1].Y) / 2 - 20));
                }
                else
                {
                    if (StartX > EndX)
                    {
                        StartX -= 15;
                        EndX += 15;
                    }
                    else
                    {
                        StartX += 15;
                        EndX -= 15;
                    }
                    pen.EndCap = LineCap.ArrowAnchor;
                    graphics.DrawLine(pen, new Point(StartX, t.Start.Y), new Point(EndX, t.End.Y));
                    graphics.DrawString(t.Name, new Font("Verdana", 6, FontStyle.Bold), new SolidBrush(Color.Black), new PointF((StartX + EndX) / 2, (t.Start.Y + t.End.Y) / 2 - 20));
                }

                foreach (InfPoint ip in t.InfList)
                {
                    pen.Width = 3;
                    graphics.DrawEllipse(pen,ip.X,ip.Y,1,1);
                }
            }
        }

        private void AddTransition()
        {
            if (SelectedFigure != null && SelectedFigure is State)
            {
                Figure tempState = getFigureByPoint(panel1.PointToClient(Control.MousePosition));
                if (tempState != null && tempState is State)
                {
                    TransitionNameForm tnf = new TransitionNameForm();
                    tnf.ShowDialog();
                    if (Program.NewTransitionName != null)
                    {
                        foreach(Transition t in findAllTrasitionsFromState((SelectedFigure as State).Name))
                        {
                            if(t.Name == Program.NewTransitionName)
                            {
                                Program.NewTransitionName = null;
                                MessageBox.Show("Данное имя уже используется в другом переходе из состояния начала", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        TransitionList.Add(new Transition(SelectedFigure as State, tempState as State, Program.NewTransitionName));
                        Program.NewTransitionName = null;
                        if ((SelectedFigure as State).Name == (tempState as State).Name)
                        {
                            TransitionList[TransitionList.Count - 1].InfList.Add(new InfPoint(tempState.X - 20, tempState.Y - 40));
                            TransitionList[TransitionList.Count - 1].InfList.Add(new InfPoint(tempState.X + 20, tempState.Y - 40));
                        }
                    }
                    SelectedFigure = null;
                    tempState = null;
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (toolStripButton2.Checked)
                    if (SelectedFigure != null)
                    {
                        SelectedFigure.X = e.X;
                        SelectedFigure.Y = e.Y;
                        SelectedFigure = null;
                    }

                if (toolStripButton3.Checked)
                    AddTransition();
            }
            Draw();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Вы действительно хотите создать новый проект конечного автомата?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                StateList.Clear();
                TransitionList.Clear();
                Draw();
            }
        }

        private int generateNewStateName()
        {
            int result = 0;
            bool flag = false;
            do
            {
                flag = false;
                foreach (State s in StateList)
                    if (s.Name == result)
                    {
                        result++;
                        flag = true;
                        
                    }
            }
            while (flag);
            return result;
        }

        private State findStateByName(int name)
        {
            foreach (State s in StateList)
                if (s.Name == name)
                    return s;
            return null;
        }

        private List<Transition> findAllTrasitionsFromState(int name)
        {
            List<Transition> result = new List<Transition>();
            foreach (Transition t in TransitionList)
                if (t.Start.Name == name)
                    result.Add(t);
            return result;
        }

        private State getStartState()
        {
            foreach (State s in StateList)
                if (s.isStartState)
                    return s;
            return null;
        }

        private Figure getFigureByPoint(Point p)
        {
            foreach (State s in StateList)
                if (s.getBounds().Contains(p))
                    return s;

            foreach (Transition t in TransitionList)
                foreach (InfPoint ip in t.InfList)
                    if (ip.getBounds().Contains(p))
                        return ip;

            return null;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "State Machine Data (*.smd)|*.smd";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                // сериализация - начало
                Stream FileStream = File.Create(sfd.FileName);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(FileStream, StateList);
                serializer.Serialize(FileStream, TransitionList);
                FileStream.Close();
                // сериализация - конец
                MessageBox.Show("Проект конечного автомата успешно сохранён!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "State Machine Data (*.smd)|*.smd";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                List<State> tmp1 = new List<State>();
                List<Transition> tmp2 = new List<Transition>();
                foreach (State s in StateList) tmp1.Add(s);
                foreach (Transition t in TransitionList) tmp2.Add(t);
                try
                {
                    // десериализация - начало
                    Stream FileStream = File.OpenRead(ofd.FileName);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    StateList = (List<State>)deserializer.Deserialize(FileStream);
                    TransitionList = (List<Transition>)deserializer.Deserialize(FileStream);
                    FileStream.Close();
                    // десериализация - конец
                    foreach(Transition t in TransitionList)
                    {
                        t.Start = findStateByName(t.Start.Name);
                        t.End = findStateByName(t.End.Name);
                    }
                    Draw();
                }
                catch(Exception)
                {
                    MessageBox.Show("Произошла ошибка открытия файла! Возможно, файл повреждён!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StateList = new List<State>(); foreach (State s in tmp1) StateList.Add(s);
                    TransitionList = new List<Transition>(); foreach (Transition t in tmp2) TransitionList.Add(t);
                }
            }
        }

        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog myPrintDialog = new PrintDialog();
            if (myPrintDialog.ShowDialog() == DialogResult.OK)
            {
                PrinterSettings values = myPrintDialog.PrinterSettings;
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings = values;
                pd.DocumentName = "State Machine";
                CaptureScreen();
                pd.PrintPage += new PrintPageEventHandler(PD_PrintPage);
                pd.Print();
            }
        }

        private void PD_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }

        Bitmap memoryImage;

        private void CaptureScreen()
        {
            Size s = panel1.Size;
            memoryImage = new Bitmap(s.Width, s.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(panel1.PointToScreen(new Point(0,0)).X, panel1.PointToScreen(new Point(0,0)).Y, 0, 0, s);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = "Графический редактор конечных автоматов SMEditor.\n\n" +
                "Автор: Чистяков П.А." +;
            MessageBox.Show(s, "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void показатьСправкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, @"HELP\Help.chm");
        }
        
    }
}
