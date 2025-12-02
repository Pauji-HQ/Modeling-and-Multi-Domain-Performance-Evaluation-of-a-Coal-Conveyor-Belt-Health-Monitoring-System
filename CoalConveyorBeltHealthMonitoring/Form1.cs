using System;
using System.Numerics;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;

namespace CoalConveyorBeltHealthMonitoring
{
    public partial class Form1 : Form
    {
        private Timer updateTimer;
        private Random random = new Random();

        // Data buffers for realtime charts
        private List<double> tmp36TimeData = new List<double>();
        private List<double> sen0381TimeData = new List<double>();
        private List<double> mma7361lTimeData = new List<double>();
        private List<double> loadCellTimeData = new List<double>();
        private List<double> rfidTimeData = new List<double>();

        private const int MaxDataPoints = 100; // Number of points to display in realtime charts

        public Form1()
        {
            InitializeComponent();
            InitializeCharts();
            InitializeTimers();
            InitializeTrackBarEvents();
            Load3DModelImages();
            UpdateAllSensorLabels(); // Initial label update
        }

        private void InitializeCharts()
        {
            // Clear default series and areas for all charts
            ClearChart(chartTMP36Time);
            ClearChart(chartTMP36Freq);
            ClearChart(chartSEN0381Time);
            ClearChart(chartSEN0381Freq);
            ClearChart(chartMMA7361LTime);
            ClearChart(chartMMA7361LFreq);
            ClearChart(chartLoadCellTime);
            ClearChart(chartLoadCellFreq);
            ClearChart(chartRFIDTime);
            ClearChart(chartRFIDFreq);
            ClearChart(chartLaplace); // Clear Laplace initially
            ClearChart(chartZDomain); // Clear Z-Domain initially

            // Setup Time Domain Charts
            SetupRealtimeChart(chartTMP36Time, "Temperature Output", "Time (s)", "Voltage (V)");
            SetupRealtimeChart(chartSEN0381Time, "Proximity Measured", "Time (s)", "Distance (cm)");
            SetupRealtimeChart(chartMMA7361LTime, "Acceleration Output", "Time (s)", "Acceleration (g)");
            SetupRealtimeChart(chartLoadCellTime, "Load Cell Output", "Time (s)", "Voltage (V)");
            SetupRealtimeChart(chartRFIDTime, "RFID Status", "Time (s)", "Value"); // Placeholder

            // Setup Frequency Domain Charts
            SetupFrequencyChart(chartTMP36Freq, "Temperature Freq Domain", "Frequency (Hz)", "Amplitude");
            SetupFrequencyChart(chartSEN0381Freq, "Proximity Freq Domain", "Frequency (Hz)", "Amplitude");
            SetupFrequencyChart(chartMMA7361LFreq, "Accelerometer Freq Domain", "Frequency (Hz)", "Amplitude");
            SetupFrequencyChart(chartLoadCellFreq, "Load Cell Freq Domain", "Frequency (Hz)", "Amplitude");
            SetupFrequencyChart(chartRFIDFreq, "RFID Freq Domain", "Frequency (Hz)", "Amplitude"); // Placeholder

            // Setup Laplace and Z Domain Charts (initially hidden)
            // Call SetupComplexChart to ensure ChartArea and basic series are created
            SetupComplexChart(chartLaplace, "Laplace Domain (s-plane)", "Real", "Imaginary");
            SetupComplexChart(chartZDomain, "Z Domain (z-plane)", "Real", "Imaginary");

            // Ensure they are truly hidden initially.
            chartLaplace.Visible = false;
            chartZDomain.Visible = false;
            // Also hide the panel if it's not visible initially
            tlpAnalysisCharts.Visible = false;
        }

        private void ClearChart(Chart chart)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Titles.Clear();
            chart.Annotations.Clear(); // Clear annotations for robust reset
        }

        private void SetupRealtimeChart(Chart chart, string title, string xTitle, string yTitle)
        {
            ChartArea ca = new ChartArea(title);
            chart.ChartAreas.Add(ca);
            ca.AxisX.Title = xTitle;
            ca.AxisY.Title = yTitle;
            ca.AxisX.LabelStyle.Format = "0.00";
            ca.AxisY.LabelStyle.Format = "0.00";
            Series s = new Series("Data") { ChartType = SeriesChartType.Line };
            chart.Series.Add(s);
            s.BorderWidth = 2;
            chart.Titles.Add(title);
        }

        private void SetupFrequencyChart(Chart chart, string title, string xTitle, string yTitle)
        {
            ChartArea ca = new ChartArea(title);
            chart.ChartAreas.Add(ca);
            ca.AxisX.Title = xTitle;
            ca.AxisY.Title = yTitle;
            ca.AxisX.LabelStyle.Format = "0.0";
            ca.AxisY.LabelStyle.Format = "0.00";
            Series s = new Series("Magnitude") { ChartType = SeriesChartType.Column };
            chart.Series.Add(s);
            chart.Titles.Add(title);
        }

        private void SetupComplexChart(Chart chart, string title, string xTitle, string yTitle)
        {
            // Clear existing ChartArea if any, before adding a new one
            if (chart.ChartAreas.Count > 0)
            {
                chart.ChartAreas.Clear();
            }

            ChartArea ca = new ChartArea(title);
            chart.ChartAreas.Add(ca);
            ca.AxisX.Title = xTitle;
            ca.AxisY.Title = yTitle;
            ca.AxisX.LabelStyle.Format = "0.0";
            ca.AxisY.LabelStyle.Format = "0.0";
            ca.AxisX.IsStartedFromZero = false;
            ca.AxisY.IsStartedFromZero = false;

            ca.AxisX.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            ca.AxisX.Crossing = 0; // X-axis crosses Y-axis at 0
            ca.AxisY.Crossing = 0; // Y-axis crosses X-axis at 0
            ca.AxisX.MajorGrid.Enabled = true;
            ca.AxisY.MajorGrid.Enabled = true;

            // Ensure Series are cleared before adding, to prevent duplicates on re-setup
            chart.Series.Clear();
            chart.Series.Add(new Series("Poles") { ChartType = SeriesChartType.Point, MarkerStyle = MarkerStyle.Cross, MarkerSize = 10, MarkerColor = Color.Red });
            chart.Series.Add(new Series("Zeros") { ChartType = SeriesChartType.Point, MarkerStyle = MarkerStyle.Circle, MarkerSize = 8, MarkerColor = Color.Blue });

            // Clear existing titles before adding a new one
            chart.Titles.Clear();
            chart.Titles.Add(title);
        }

        private void InitializeTimers()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 100; // 100 ms
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void InitializeTrackBarEvents()
        {
            // TMP36
            tbTMP36Temp.Scroll += (sender, e) => UpdateTMP36Sensor();
            tbTMP36Offset.Scroll += (sender, e) => UpdateTMP36Sensor();
            tbTMP36Noise.Scroll += (sender, e) => UpdateTMP36Sensor();

            // SEN0381
            tbSEN0381Dtrue.Scroll += (sender, e) => UpdateSEN0381Sensor();
            tbSEN0381Rho.Scroll += (sender, e) => UpdateSEN0381Sensor();
            tbSEN0381Temp.Scroll += (sender, e) => UpdateSEN0381Sensor();

            // MMA7361L
            tbMMA7361LAture.Scroll += (sender, e) => UpdateMMA7361LSensor();
            tbMMA7361LTemp.Scroll += (sender, e) => UpdateMMA7361LSensor();
            tbMMA7361LNoise.Scroll += (sender, e) => UpdateMMA7361LSensor();

            // Load Cell
            tbLoadCellWeight.Scroll += (sender, e) => UpdateLoadCellSensor();
            tbLoadCellTemp.Scroll += (sender, e) => UpdateLoadCellSensor();
            tbLoadCellVibration.Scroll += (sender, e) => UpdateLoadCellSensor();

            // RFID
            cbRFIDLocation.SelectedIndexChanged += (sender, e) => UpdateRFIDSensor();
        }

        private void UpdateAllSensorLabels()
        {
            UpdateTMP36Sensor();
            UpdateSEN0381Sensor();
            UpdateMMA7361LSensor();
            UpdateLoadCellSensor();
            UpdateRFIDSensor();
        }

        private void UpdateTMP36Sensor()
        {
            double T = tbTMP36Temp.Value;
            double V_offset = (double)tbTMP36Offset.Value / 100.0;
            double N = (double)tbTMP36Noise.Value / 100.0;

            lblTMP36Temp.Text = $"T ({T}°C):";
            lblTMP36Offset.Text = $"V_offset ({V_offset:F2}V):";
            lblTMP36Noise.Text = $"N ({N:F2}V):";
            // Update formula label based on new values
            lblTMP36Formula.Text = $"Vout(T) = (0.010 * {T}°C) + {V_offset:F2}V + ({N:F2}V * rand)";
        }

        private void UpdateSEN0381Sensor()
        {
            double D_true = tbSEN0381Dtrue.Value;
            double rho = (double)tbSEN0381Rho.Value / 10.0;
            double T_env = tbSEN0381Temp.Value;

            lblSEN0381Dtrue.Text = $"D_true ({D_true}cm):";
            lblSEN0381Rho.Text = $"ρ ({rho:F1}):";
            lblSEN0381Temp.Text = $"T ({T_env}°C):";
            // Update formula label
            lblSEN0381Formula.Text = $"D_measured = {D_true} + ({rho:F1}*0.1) + ({T_env}*0.05) + N";
        }

        private void UpdateMMA7361LSensor()
        {
            double a_true = (double)tbMMA7361LAture.Value / 10.0;
            double T = tbMMA7361LTemp.Value;
            double N = (double)tbMMA7361LNoise.Value / 100.0;

            lblMMA7361LAture.Text = $"a_true ({a_true:F1}m/s²):";
            lblMMA7361LTemp.Text = $"T ({T}°C):";
            lblMMA7361LNoise.Text = $"N ({N:F2}g):";
            // Update formula label
            lblMMA7361LFormula.Text = $"a_out = (1 + 0.001*({T}-25)) * {a_true:F1} + ({N:F2} * rand)";
        }

        private void UpdateLoadCellSensor()
        {
            double W = tbLoadCellWeight.Value;
            double T = tbLoadCellTemp.Value;
            double V_b = (double)tbLoadCellVibration.Value / 10.0;

            lblLoadCellWeight.Text = $"W ({W}kg):";
            lblLoadCellTemp.Text = $"T ({T}°C):";
            lblLoadCellVibration.Text = $"Vb ({V_b:F1}):";
            // Update formula label
            lblLoadCellFormula.Text = $"Vout = ({W}*0.002) + ({T}*0.001) + ({V_b:F1}*0.05*rand)";
        }

        private void UpdateRFIDSensor()
        {
            string location = cbRFIDLocation.SelectedItem?.ToString() ?? "N/A";
            lblRFIDFormula.Text = $"Location: {location}";
            // Update RFID status label (if you have one, or use lblRFIDFormula for direct status)
            // Example: lblRFIDStatus.Text = $"Current Location: {location}";
        }


        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Simulate TMP36
            double T_tmp36 = tbTMP36Temp.Value;
            double V_offset_tmp36 = (double)tbTMP36Offset.Value / 100.0;
            double N_noise_tmp36 = (double)tbTMP36Noise.Value / 100.0;
            double Vout_TMP36 = (0.010 * T_tmp36) + V_offset_tmp36 + (N_noise_tmp36 * (random.NextDouble() - 0.5));
            AddDataPoint(tmp36TimeData, Vout_TMP36);
            UpdateChart(chartTMP36Time, tmp36TimeData);
            UpdateFrequencyChart(chartTMP36Freq, tmp36TimeData);

            // Simulate SEN0381
            double D_true_sen0381 = tbSEN0381Dtrue.Value;
            double rho_sen0381 = (double)tbSEN0381Rho.Value / 10.0;
            double T_env_sen0381 = tbSEN0381Temp.Value;
            double k1_sen0381 = rho_sen0381 * 0.1;
            double k2_sen0381 = T_env_sen0381 * 0.05;
            double D_measured_sen0381 = D_true_sen0381 + k1_sen0381 + k2_sen0381 + (random.NextDouble() - 0.5) * 0.5;
            AddDataPoint(sen0381TimeData, D_measured_sen0381);
            UpdateChart(chartSEN0381Time, sen0381TimeData);
            UpdateFrequencyChart(chartSEN0381Freq, sen0381TimeData);

            // Simulate MMA7361L
            double a_true_mma = (double)tbMMA7361LAture.Value / 10.0;
            double T_mma = tbMMA7361LTemp.Value;
            double N_noise_mma = (double)tbMMA7361LNoise.Value / 100.0;
            double S0_mma = 1.0;
            double alpha_mma = 0.001;
            double T0_mma = 25.0;
            double S_T_mma = S0_mma + alpha_mma * (T_mma - T0_mma);
            double a_out_mma = S_T_mma * a_true_mma + (N_noise_mma * (random.NextDouble() - 0.5));
            AddDataPoint(mma7361lTimeData, a_out_mma);
            UpdateChart(chartMMA7361LTime, mma7361lTimeData);
            UpdateFrequencyChart(chartMMA7361LFreq, mma7361lTimeData);

            // Simulate Load Cell
            double W_loadCell = tbLoadCellWeight.Value;
            double T_loadCell = tbLoadCellTemp.Value;
            double V_b_loadCell = (double)tbLoadCellVibration.Value / 10.0;
            double G_loadCell = 0.002;
            double beta_T_loadCell = T_loadCell * 0.001;
            double N_vb_loadCell = V_b_loadCell * 0.05 * (random.NextDouble() - 0.5);
            double Vout_LoadCell = (W_loadCell * G_loadCell) + beta_T_loadCell + N_vb_loadCell;
            AddDataPoint(loadCellTimeData, Vout_LoadCell);
            UpdateChart(chartLoadCellTime, loadCellTimeData);
            UpdateFrequencyChart(chartLoadCellFreq, loadCellTimeData);

            // Simulate RFID (just a simple state change visualization)
            double rfidValue = cbRFIDLocation.SelectedIndex + 1; // 1 for Front, 2 for Middle, 3 for End
            AddDataPoint(rfidTimeData, rfidValue);
            UpdateChart(chartRFIDTime, rfidTimeData);
            UpdateFrequencyChart(chartRFIDFreq, rfidTimeData);
        }

        private void AddDataPoint(List<double> dataList, double value)
        {
            dataList.Add(value);
            if (dataList.Count > MaxDataPoints)
            {
                dataList.RemoveAt(0);
            }
        }

        private void UpdateChart(Chart chart, List<double> dataList)
        {
            if (chart.Series.Count == 0 || chart.ChartAreas.Count == 0) return; // Add check if series or chartarea is not initialized
            chart.Series[0].Points.Clear();
            for (int i = 0; i < dataList.Count; i++)
            {
                chart.Series[0].Points.AddXY(i * (updateTimer.Interval / 1000.0), dataList[i]);
            }
            chart.ChartAreas[0].RecalculateAxesScale();
        }

        // Simple FFT implementation
        private void UpdateFrequencyChart(Chart chart, List<double> timeDomainData)
        {
            if (chart.Series.Count == 0 || chart.ChartAreas.Count == 0 || timeDomainData.Count < 2)
            {
                if (chart.Series.Count > 0) chart.Series[0].Points.Clear();
                return;
            }

            // Pad with zeros to the next power of 2 for FFT
            int n = 1;
            while (n < timeDomainData.Count)
            {
                n <<= 1;
            }

            Complex[] complexData = new Complex[n];
            for (int i = 0; i < timeDomainData.Count; i++)
            {
                complexData[i] = new Complex(timeDomainData[i], 0);
            }
            for (int i = timeDomainData.Count; i < n; i++)
            {
                complexData[i] = new Complex(0, 0);
            }

            FastFourierTransform(complexData, false); // Perform FFT

            double samplingRate = 1000.0 / updateTimer.Interval; // Samples per second
            double df = samplingRate / n; // Frequency resolution

            chart.Series[0].Points.Clear();
            for (int i = 0; i < n / 2; i++) // Only positive frequencies
            {
                double magnitude = complexData[i].Magnitude;
                chart.Series[0].Points.AddXY(i * df, magnitude);
            }
            chart.ChartAreas[0].RecalculateAxesScale();
        }

        // Basic Cooley-Tukey FFT implementation
        private void FastFourierTransform(Complex[] buffer, bool inverse)
        {
            int n = buffer.Length;
            int m = (int)Math.Log(n, 2);

            // Bit-reversal permutation
            for (int i = 0; i < n; i++)
            {
                int j = 0; // Deklarasi 'j' di sini
                int k = i;
                for (int l = 0; l < m; l++)
                {
                    j = (j << 1) | (k & 1);
                    k >>= 1;
                }
                if (j > i)
                {
                    Complex temp = buffer[i];
                    buffer[i] = buffer[j];
                    buffer[j] = temp;
                }
            }

            // Cooley-Tukey butterfly operation
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = (inverse ? 2 : -2) * Math.PI / len;
                Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));
                for (int i = 0; i < n; i += len)
                {
                    Complex w = new Complex(1, 0); // Deklarasi 'w' di sini
                    for (int j = 0; j < len / 2; j++) // 'j' juga perlu dideklarasikan dalam loop ini
                    {
                        Complex u = buffer[i + j];
                        Complex v = buffer[i + j + len / 2] * w;
                        buffer[i + j] = u + v;
                        buffer[i + j + len / 2] = u - v;
                        w *= wlen;
                    }
                }
            }

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    buffer[i] /= n;
                }
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            // Pastikan panel yang menampung chart-chart ini terlihat
            tlpAnalysisCharts.Visible = true;

            // Pastikan chart-chart itu sendiri terlihat
            chartLaplace.Visible = true;
            chartZDomain.Visible = true;

            // ***** Perbaikan Penting: Panggil SetupComplexChart lagi untuk memastikan ChartArea dan series dasar ada *****
            // Ini akan menghapus ChartArea dan Series lama, lalu membuat yang baru dengan konfigurasi yang benar.
            SetupComplexChart(chartLaplace, "Laplace Domain (s-plane)", "Real", "Imaginary");
            SetupComplexChart(chartZDomain, "Z Domain (z-plane)", "Real", "Imaginary");

            // Pastikan untuk menghapus semua anotasi yang mungkin tersisa dari eksekusi sebelumnya
            chartLaplace.Annotations.Clear();
            chartZDomain.Annotations.Clear();

            // Example Poles and Zeros for Laplace Domain (s-plane)
            // Anda bisa mengganti ini dengan poles/zeros yang dihitung dari data sensor jika diperlukan
            chartLaplace.Series["Poles"].Points.AddXY(-0.5, 0);
            chartLaplace.Series["Poles"].Points.AddXY(-0.5, 2);
            chartLaplace.Series["Poles"].Points.AddXY(-0.5, -2);
            chartLaplace.Series["Zeros"].Points.AddXY(-2, 0);
            chartLaplace.Series["Zeros"].Points.AddXY(1, 1);
            chartLaplace.Series["Zeros"].Points.AddXY(1, -1);

            // Draw Region of Convergence (RoC) for Laplace.
            // Contoh: RoC adalah Re[s] > -0.5 (kanan dari pole paling kanan)
            double mostRightPole = -0.5; // Contoh
            DrawRegionOfConvergenceLaplace(chartLaplace, mostRightPole, "right");

            // Example Poles and Zeros for Z Domain (z-plane)
            // Anda bisa mengganti ini dengan poles/zeros yang dihitung dari data sensor jika diperlukan
            chartZDomain.Series["Poles"].Points.AddXY(0.5, 0);
            chartZDomain.Series["Poles"].Points.AddXY(-0.8, 0.3);
            chartZDomain.Series["Poles"].Points.AddXY(-0.8, -0.3);
            chartZDomain.Series["Zeros"].Points.AddXY(0, 0);
            chartZDomain.Series["Zeros"].Points.AddXY(0.9, 0);
            chartZDomain.Series["Zeros"].Points.AddXY(0.9, 0.9);
            chartZDomain.Series["Zeros"].Points.AddXY(0.9, -0.9);


            // Draw Unit Circle for Z-Domain
            DrawUnitCircle(chartZDomain);

            // Draw Region of Convergence (RoC) for Z-Domain.
            // Contoh: RoC adalah |z| > 0.8 (luar dari lingkaran dengan radius 0.8)
            double largestPoleMagnitude = Math.Sqrt(Math.Pow(-0.8, 2) + Math.Pow(0.3, 2)); // Magnitudo pole -0.8 + j0.3
            DrawRegionOfConvergenceZDomain(chartZDomain, largestPoleMagnitude + 0.05, "outside"); // Sedikit lebih besar dari pole terjauh

            // Contoh RoC annular: |z| > 0.5 dan |z| < 0.9
            // DrawRegionOfConvergenceZDomain(chartZDomain, 0.5, 0.9, "annular");
        }

        private void DrawRegionOfConvergenceLaplace(Chart chart, double boundary, string type)
        {
            ChartArea chartArea = chart.ChartAreas[0];
            chart.Annotations.Clear(); // Clear existing annotations

            // Set fixed axis limits for a consistent view
            chartArea.AxisX.Minimum = -4;
            chartArea.AxisX.Maximum = 4;
            chartArea.AxisY.Minimum = -4;
            chartArea.AxisY.Maximum = 4;

            // RoC Shading with RectangleAnnotation
            RectangleAnnotation rocRect = new RectangleAnnotation();
            rocRect.IsSizeAlwaysRelative = false; // Important for absolute positioning
            rocRect.ClipToChartArea = chartArea.Name;
            rocRect.LineColor = Color.Blue; // Border color
            rocRect.LineDashStyle = ChartDashStyle.Dash;
            rocRect.LineWidth = 1;
            rocRect.BackColor = Color.FromArgb(50, Color.LightBlue); // Semi-transparent blue shading

            // Text Annotation for RoC description
            TextAnnotation textAnno = new TextAnnotation();
            textAnno.ForeColor = Color.DarkBlue;
            textAnno.Font = new Font("Arial", 8, FontStyle.Bold);
            textAnno.IsSizeAlwaysRelative = false; // Important for X/Y coordinates
            textAnno.AnchorAlignment = ContentAlignment.TopRight; // Default alignment, adjust later

            if (type == "right")
            {
                rocRect.X = boundary;
                rocRect.Y = chartArea.AxisY.Minimum;
                rocRect.Width = chartArea.AxisX.Maximum - boundary;
                rocRect.Height = chartArea.AxisY.Maximum - chartArea.AxisY.Minimum;
                textAnno.Text = $"Re[s] > {boundary:F1}";
                textAnno.X = boundary + 0.1; // Position slightly right of the boundary
                textAnno.Y = chartArea.AxisY.Maximum - 0.2; // Near top right
            }
            else if (type == "left")
            {
                rocRect.X = chartArea.AxisX.Minimum;
                rocRect.Y = chartArea.AxisY.Minimum;
                rocRect.Width = boundary - chartArea.AxisX.Minimum;
                rocRect.Height = chartArea.AxisY.Maximum - chartArea.AxisY.Minimum;
                textAnno.Text = $"Re[s] < {boundary:F1}";
                textAnno.X = boundary - 0.1; // Position slightly left of the boundary
                textAnno.Y = chartArea.AxisY.Maximum - 0.2; // Near top left
                textAnno.AnchorAlignment = ContentAlignment.TopLeft; // Adjust alignment
            }
            else if (type == "middle") // For RoC as a strip, e.g., boundary1 < Re[s] < boundary2
            {
                // Example for a middle strip: Re[s] between -1.5 and 0.5
                double boundary1 = -1.5; // Example
                double boundary2 = 0.5; // Example
                rocRect.X = boundary1;
                rocRect.Y = chartArea.AxisY.Minimum;
                rocRect.Width = boundary2 - boundary1;
                rocRect.Height = chartArea.AxisY.Maximum - chartArea.AxisY.Minimum;
                rocRect.LineColor = Color.Green; // Different color for middle strip
                rocRect.BackColor = Color.FromArgb(50, Color.LightGreen);

                textAnno.Text = $"{boundary1:F1} < Re[s] < {boundary2:F1}";
                textAnno.X = (boundary1 + boundary2) / 2; // Center text
                textAnno.Y = chartArea.AxisY.Maximum - 0.2;
                textAnno.AnchorAlignment = ContentAlignment.TopCenter;
            }

            chart.Annotations.Add(rocRect);
            if (!string.IsNullOrEmpty(textAnno.Text))
            {
                chart.Annotations.Add(textAnno);
            }
            chartArea.RecalculateAxesScale();
        }

        // Overload for Z-Domain with single boundary
        private void DrawRegionOfConvergenceZDomain(Chart chart, double radius, string type)
        {
            DrawRegionOfConvergenceZDomain(chart, radius, -1.0, type); // -1.0 as a placeholder for no inner radius
        }

        // Overload for Z-Domain with inner and outer boundary (annular)
        private void DrawRegionOfConvergenceZDomain(Chart chart, double innerRadius, double outerRadius, string type)
        {
            ChartArea chartArea = chart.ChartAreas[0];
            chart.Annotations.Clear(); // Clear existing annotations

            // Set fixed axis limits for a consistent view
            chartArea.AxisX.Minimum = -1.5;
            chartArea.AxisX.Maximum = 1.5;
            chartArea.AxisY.Minimum = -1.5;
            chartArea.AxisY.Maximum = 1.5;

            // Clear previous RoC circle series if any
            if (chart.Series.Any(s => s.Name == "RoC Boundary Inner"))
            {
                chart.Series.Remove(chart.Series.FindByName("RoC Boundary Inner"));
            }
            if (chart.Series.Any(s => s.Name == "RoC Boundary Outer"))
            {
                chart.Series.Remove(chart.Series.FindByName("RoC Boundary Outer"));
            }
            if (chart.Series.Any(s => s.Name == "RoC Shading"))
            {
                chart.Series.Remove(chart.Series.FindByName("RoC Shading"));
            }


            Series rocCircleInner = new Series("RoC Boundary Inner");
            rocCircleInner.ChartType = SeriesChartType.Line;
            rocCircleInner.BorderDashStyle = ChartDashStyle.Dot;
            rocCircleInner.Color = Color.OrangeRed;
            rocCircleInner.BorderWidth = 2;
            rocCircleInner.IsVisibleInLegend = false;

            Series rocCircleOuter = new Series("RoC Boundary Outer");
            rocCircleOuter.ChartType = SeriesChartType.Line;
            rocCircleOuter.BorderDashStyle = ChartDashStyle.Dot;
            rocCircleOuter.Color = Color.OrangeRed;
            rocCircleOuter.BorderWidth = 2;
            rocCircleOuter.IsVisibleInLegend = false;

            TextAnnotation textAnno = new TextAnnotation();
            textAnno.ForeColor = Color.DarkRed;
            textAnno.Font = new Font("Arial", 8, FontStyle.Bold);
            textAnno.IsSizeAlwaysRelative = false; // important for X/Y coordinates
            textAnno.AnchorAlignment = ContentAlignment.BottomRight; // Default alignment for text

            // For Z-domain RoC shading, we need to draw a filled area.
            // Using PolygonAnnotation or a filled Series is a good approach.
            // For simplicity and direct control, we can draw many small lines to simulate shading,
            // or use a PolygonAnnotation for more complex shapes if needed.
            // For circular regions, a simpler approach is to use a custom drawing event or a dedicated series.
            // Here, we'll draw concentric circles to simulate shading for 'outside' and 'inside' types,
            // and use two boundary lines for 'annular' with a text description.

            if (type == "outside") // RoC: |z| > radius
            {
                for (int i = 0; i <= 360; i++)
                {
                    double angle = Math.PI * i / 180.0;
                    rocCircleInner.Points.AddXY(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle));
                }
                chart.Series.Add(rocCircleInner);

                // Simulate shading by drawing slightly larger concentric circles
                Series shadingSeries = new Series("RoC Shading");
                shadingSeries.ChartType = SeriesChartType.Line;
                shadingSeries.Color = Color.FromArgb(20, Color.OrangeRed); // Very light, semi-transparent
                shadingSeries.BorderWidth = 1;
                shadingSeries.IsVisibleInLegend = false;
                for (double r = innerRadius + 0.01; r < chartArea.AxisX.Maximum; r += 0.05) // Draw circles outwards
                {
                    for (int i = 0; i <= 360; i += 5) // Fewer points for performance
                    {
                        double angle = Math.PI * i / 180.0;
                        shadingSeries.Points.AddXY(r * Math.Cos(angle), r * Math.Sin(angle));
                    }
                }
                chart.Series.Add(shadingSeries);


                textAnno.Text = $"|z| > {innerRadius:F2}";
                textAnno.X = chartArea.AxisX.Maximum - 0.1;
                textAnno.Y = chartArea.AxisY.Maximum - 0.1;
            }
            else if (type == "inside") // RoC: |z| < radius
            {
                for (int i = 0; i <= 360; i++)
                {
                    double angle = Math.PI * i / 180.0;
                    rocCircleOuter.Points.AddXY(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle));
                }
                chart.Series.Add(rocCircleOuter);

                // Simulate shading by drawing slightly smaller concentric circles
                Series shadingSeries = new Series("RoC Shading");
                shadingSeries.ChartType = SeriesChartType.Line;
                shadingSeries.Color = Color.FromArgb(20, Color.OrangeRed); // Very light, semi-transparent
                shadingSeries.BorderWidth = 1;
                shadingSeries.IsVisibleInLegend = false;
                for (double r = innerRadius - 0.01; r > chartArea.AxisX.Minimum / 2; r -= 0.05) // Draw circles inwards
                {
                    if (r <= 0) break; // Don't draw negative radius
                    for (int i = 0; i <= 360; i += 5)
                    {
                        double angle = Math.PI * i / 180.0;
                        shadingSeries.Points.AddXY(r * Math.Cos(angle), r * Math.Sin(angle));
                    }
                }
                chart.Series.Add(shadingSeries);

                textAnno.Text = $"|z| < {innerRadius:F2}";
                textAnno.X = chartArea.AxisX.Minimum + 0.1;
                textAnno.Y = chartArea.AxisY.Maximum - 0.1;
                textAnno.AnchorAlignment = ContentAlignment.TopLeft; // Adjust alignment
            }
            else if (type == "annular") // RoC: innerRadius < |z| < outerRadius
            {
                // Draw inner boundary
                for (int i = 0; i <= 360; i++)
                {
                    double angle = Math.PI * i / 180.0;
                    rocCircleInner.Points.AddXY(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle));
                }
                chart.Series.Add(rocCircleInner);

                // Draw outer boundary
                for (int i = 0; i <= 360; i++)
                {
                    double angle = Math.PI * i / 180.0;
                    rocCircleOuter.Points.AddXY(outerRadius * Math.Cos(angle), outerRadius * Math.Sin(angle));
                }
                chart.Series.Add(rocCircleOuter);

                // Simulate shading by drawing concentric circles between inner and outer radius
                Series shadingSeries = new Series("RoC Shading");
                shadingSeries.ChartType = SeriesChartType.Line;
                shadingSeries.Color = Color.FromArgb(20, Color.OrangeRed); // Very light, semi-transparent
                shadingSeries.BorderWidth = 1;
                shadingSeries.IsVisibleInLegend = false;
                for (double r = innerRadius + 0.01; r < outerRadius - 0.01; r += 0.05) // Draw circles between boundaries
                {
                    for (int i = 0; i <= 360; i += 5)
                    {
                        double angle = Math.PI * i / 180.0;
                        shadingSeries.Points.AddXY(r * Math.Cos(angle), r * Math.Sin(angle));
                    }
                }
                chart.Series.Add(shadingSeries);

                textAnno.Text = $"{innerRadius:F2} < |z| < {outerRadius:F2}";
                textAnno.X = 0; // Center text horizontally
                textAnno.Y = (innerRadius + outerRadius) / 2; // Center text vertically in the annulus
                textAnno.AnchorAlignment = ContentAlignment.MiddleCenter; // Adjust alignment
            }

            if (!string.IsNullOrEmpty(textAnno.Text))
            {
                chart.Annotations.Add(textAnno);
            }

            chartArea.RecalculateAxesScale();
        }

        private void DrawUnitCircle(Chart chart)
        {
            // Clear previous unit circle if any
            if (chart.Series.Any(s => s.Name == "Unit Circle"))
            {
                chart.Series.Remove(chart.Series.FindByName("Unit Circle"));
            }

            // Draw unit circle
            Series unitCircle = new Series("Unit Circle");
            unitCircle.ChartType = SeriesChartType.Line;
            unitCircle.BorderDashStyle = ChartDashStyle.Dash;
            unitCircle.Color = Color.Gray;
            unitCircle.BorderWidth = 1;
            unitCircle.IsVisibleInLegend = false; // Usually don't want unit circle in legend
            for (int i = 0; i <= 360; i++)
            {
                double angle = Math.PI * i / 180.0;
                unitCircle.Points.AddXY(Math.Cos(angle), Math.Sin(angle));
            }
            chart.Series.Add(unitCircle);
        }

        private void Load3DModelImages()
        {
            // Placeholder images or local files
            // IMPORTANT: Replace with actual image paths if you have them, e.g., @"C:\MyImages\conveyor.png"
            // If you don't have images, it will use a default error icon or be blank.
            // For demonstration, let's try to load from a dummy path or leave blank.

            SetImageOrPlaceholder(pbConveyorPlant, "1.png");
            SetImageOrPlaceholder(pbTMP36Pos, "2.png");
            SetImageOrPlaceholder(pbSEN0381Pos, "3.png");
            SetImageOrPlaceholder(pbMMA7361LPos, "4.png");
            SetImageOrPlaceholder(pbLoadCellPos, "5.png");
            SetImageOrPlaceholder(pbRFIDPos, "6.png");
        }

        private void SetImageOrPlaceholder(PictureBox pb, string imageName)
        {
            try
            {
                // Ensure the 'Images' folder exists relative to the executable
                string imagesFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!System.IO.Directory.Exists(imagesFolderPath))
                {
                    System.IO.Directory.CreateDirectory(imagesFolderPath);
                }

                string imagePath = System.IO.Path.Combine(imagesFolderPath, imageName);
                if (System.IO.File.Exists(imagePath))
                {
                    // Using FromFile and then copying to avoid file lock issues
                    using (Image img = Image.FromFile(imagePath))
                    {
                        pb.Image = new Bitmap(img);
                    }
                }
                else
                {
                    // Create a simple placeholder image if not found
                    Bitmap placeholder = new Bitmap(pb.Width, pb.Height);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        g.FillRectangle(Brushes.LightGray, 0, 0, pb.Width, pb.Height);
                        g.DrawString("No Image", new Font("Arial", 10), Brushes.DarkGray, new PointF(10, 10));
                    }
                    pb.Image = placeholder;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image {imageName}: {ex.Message}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Fallback to a default error icon or blank
                pb.Image = SystemIcons.Error.ToBitmap();
            }
        }
    }
}