using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP
{
    public partial class Mainform : Form
    {
        private ProblemAndSolver _cityData;

        public Mainform()
        {
            InitializeComponent();

            _cityData = new ProblemAndSolver();
            tbSeed.Text = _cityData.Seed.ToString();
        }

        /// <summary>
        /// overloaded to call the redraw method for CityData. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SetClip(new Rectangle(0,0,Width, Height - toolStrip1.Height-35));
            _cityData.Draw(e.Graphics);
        }

        private void SetSeed()
        {
            if (Regex.IsMatch(tbSeed.Text, "^[0-9]+$"))
            {
                toolStrip1.Focus();
                _cityData = new ProblemAndSolver(int.Parse(tbSeed.Text));
                Invalidate();
            }
            else
                MessageBox.Show("Seed must be an integer.");
        }

        private HardMode.Modes getMode()
        {
            return HardMode.GetMode(cboMode.Text);
        }

        private int getProblemSize()
        {
            if (Regex.IsMatch(tbProblemSize.Text, "^[0-9]+$"))
            {
                return Int32.Parse(tbProblemSize.Text);
            }
            MessageBox.Show("Problem size must be an integer.");
            return 20;
            ;
        }

        private int getTimeLimit()
        {
            if (Regex.IsMatch(tbTimeLimit.Text, "^[0-9]+$"))
            {
                return Int32.Parse(tbTimeLimit.Text);
            }
            MessageBox.Show("Time limit must be an integer (number of seconds).");
            return 20;
            ;
        }

        // not necessarily a new problem but resets the state using the specified settings
        private void reset()
        {
            SetSeed(); // also resets the CityData variable

            var size = getProblemSize();
            var timelimit = getTimeLimit();
            var mode = getMode();
            
            tbCostOfTour.Text = " --";
            tbElapsedTime.Text = " --";
            tbNumSolutions.Text = " --";              // re-blanking the text boxes that may have been modified by a solver

            _cityData.GenerateProblem ( size, mode, timelimit );
        }


#region GUI Event Handlers

        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void tbSeed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                reset();
//                this.SetSeed();
        }

#endregion // Event Handlers

        private void Form1_Load(object sender, EventArgs e)
        {
            // use the parameters in the GUI controls
            reset();
        }

        private void tbProblemSize_Leave(object sender, EventArgs e)
        {
            reset();
        }

        private void tbProblemSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                reset();
        }

        private void tbTimeLimit_Leave(object sender, EventArgs e)
        {
            reset();
        }

        private void tbTimeLimit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                reset();
        }

        private void cboMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            reset();
        }

        private void newProblem_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(tbProblemSize.Text, "^[0-9]+$"))
            {
                var random = new Random();
                var seed = int.Parse(tbSeed.Text);
                
                reset();
                
                Invalidate(); 
            }
            else
            {
                MessageBox.Show("Problem size must be an integer.");
            };
        }

        private void randomProblem_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(tbProblemSize.Text, "^[0-9]+$"))
            {
                var random = new Random();
                var seed = random.Next(1000); // 3-digit random number
                tbSeed.Text = "" + seed;

                reset();

                Invalidate();
            }
            else
            {
                MessageBox.Show("Problem size must be an integer.");
            };
        }

        private async void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();

            tbElapsedTime.Text = " Running...";
            tbCostOfTour.Text = " Running...";
            Refresh();

            var results = await Task.Run(() => _cityData.DefaultSolveProblem());

            tbCostOfTour.Text = results[ProblemAndSolver.Cost];                        
            tbElapsedTime.Text = results[ProblemAndSolver.Time];
            tbNumSolutions.Text = results[ProblemAndSolver.Count];               
            Invalidate();                          // force a refresh.
        }

        private async void bBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();

            tbElapsedTime.Text = " Running...";
            tbCostOfTour.Text = " Running...";
            Refresh();

            var results = await Task.Run(() => _cityData.BranchBoundSolveProblem());

            tbCostOfTour.Text = results[ProblemAndSolver.Cost];
            tbElapsedTime.Text = results[ProblemAndSolver.Time];
            tbNumSolutions.Text = results[ProblemAndSolver.Count];
            Invalidate();                          // force a refresh.
        }

        private async void greedyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();

            tbElapsedTime.Text = " Running...";
            tbCostOfTour.Text = " Running...";
            Refresh();

            var results = await Task.Run(() => _cityData.GreedySolveProblem());

            tbCostOfTour.Text = results[ProblemAndSolver.Cost];
            tbElapsedTime.Text = results[ProblemAndSolver.Time];
            tbNumSolutions.Text = results[ProblemAndSolver.Count];
            Invalidate();                          // force a refresh.
        }

        private async void myTSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();

            tbElapsedTime.Text = " Running...";
            tbCostOfTour.Text = " Running...";
            Refresh();

            var results = await Task.Run(() => _cityData.FancySolveProblem());

            tbCostOfTour.Text = results[ProblemAndSolver.Cost];
            tbElapsedTime.Text = results[ProblemAndSolver.Time];
            tbNumSolutions.Text = results[ProblemAndSolver.Count];
            Invalidate();                          // force a refresh.
        }

        private void AlgorithmMenu2_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AlgorithmMenu2.Text = e.ClickedItem.Text;
            AlgorithmMenu2.Tag = e.ClickedItem;
        }

        private void AlgorithmMenu2_ButtonClick_1(object sender, EventArgs e)
        {
            if (AlgorithmMenu2.Tag != null)
            {
                (AlgorithmMenu2.Tag as ToolStripMenuItem)?.PerformClick();
            }
            else
            {
                AlgorithmMenu2.ShowDropDown();
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}