Coal Conveyor Belt Health Monitoring — Multi-Sensor & Multi-Domain Simulation

This repository contains the complete implementation of a multi-sensor, multi-domain simulation framework for evaluating the health condition of a coal conveyor belt system.
The system is based on the research paper:

“Modeling and Multi-Domain Performance Evaluation of a Coal Conveyor Belt Health Monitoring System”
by Ahmad Fauzi Abdul Razzaq & Ir. Dwi Oktavianto Wahyu Nugroho, S.T., M.T.

🚀 Overview

Coal conveyor belts are critical in mining and bulk material handling. Failures such as:

Roller imbalance

Overheating

Belt misalignment

Abnormal vibration

Uneven load distribution

can cause severe downtime and safety hazards.

This project introduces a real-time simulation software that models five heterogeneous sensors using a unified first-order dynamic model and analyzes them across four analytical domains:

Time Domain

Frequency Domain (FFT)

Laplace Domain (s-plane)

Z-Domain (Discrete Pole Stability)

The program is implemented in C# (.NET Framework) with real-time visualization to support research, simulation study, and educational demonstrations in predictive maintenance.

🧩 Key Features
✅ 5 Simulated Industrial Sensors
Sensor	Function
MMA7361L Accelerometer	Detect vibration & roller faults
TMP36 Temperature Sensor	Monitor overheating & friction
H8C Load Cell	Track coal load & lump anomalies
SEN0381 Proximity Sensor	Detect belt markers & object presence
MFRC522 RFID	Track position & tag-based events

All sensors are modeled using the same first-order transfer function, enabling unified analysis.

🔍 Multi-Domain Analysis
1️⃣ Time-Domain Visualization

Waveform responses

Transient behaviors

Fault injection analysis

2️⃣ Frequency-Domain (FFT)

Detects harmonics caused by roller imbalance

Identifies low-frequency load variations

Visualizes spectral signatures of faults

3️⃣ Laplace Domain (S-Plane)

Pole mapping

Stability evaluation

System dynamics comparison

4️⃣ Z-Domain (Discrete-Time)

Pole positions in the unit circle

Sampling rate validation

Digital model stability

🖥️ Software Architecture

The system includes:

Signal Generator — Produces simulated sensor signals

Domain Transformer — Computes FFT, s-plane poles, z-plane poles

Visualization Module — Real-time charts for all domains

Control Panel — Adjust gain, time constant, noise, sampling rate

⚙️ Technical Contributions

This repository provides:

A unified first-order mathematical model for five different conveyor sensors

A full multi-domain evaluation engine

A real-time multi-panel visualization dashboard

Simulation of roller faults, thermal rise, load anomalies, and RFID/proximity events

A baseline framework for:

Predictive maintenance

Sensor fusion research

Digital twin development

Industrial monitoring simulations

🔧 How to Run

Clone the repository:

git clone https://github.com/<your-username>/CoalConveyorBeltHealthMonitoring.git


Open the solution in Visual Studio (.NET Framework)

Build and run the project

Use the GUI dashboard to:

Select sensors

Inject faults

Adjust parameters

View domain transformations in real time

📁 Repository Structure
/src
   /Models           → Unified first-order sensor models  
   /Simulation       → Conveyor dynamics + disturbance injection  
   /Visualization    → Time, FFT, S-plane, Z-plane charts  
   /Sensors          → Accelerometer, Temperature, Load, RFID, Proximity  
   /UI               → C# WinForms/WPF GUI Dashboard  

/docs
   Paper.pdf         → Research paper  
   Images/           → 3D conveyor models & sensor placements  


(Saya sesuaikan bagian ini jika Anda ingin saya membuka isi .ZIP dan menuliskan struktur folder yang benar-benar ada di project Anda.)

📊 Demonstrated Fault Detection

The system can simulate and visualize fault signatures including:

Roller imbalance → harmonic spikes & vibration bursts

Thermal overload → slow exponential temperature rise

Overload / Lump coal → step changes in load cell output

Tag misalignment → missing RFID/proximity events

🧠 Intended Applications

Predictive maintenance research

Sensor fusion and signal processing study

Industrial monitoring simulation

Academic teaching tools

Conveyor belt diagnostics

📜 Citation

If you use this project, please cite the paper:

A. F. A. Razzaq and D. O. W. Nugroho,
"Modeling and Multi-Domain Performance Evaluation of a Coal Conveyor Belt Health Monitoring System,"
2025.

🤝 Contributors

Ahmad Fauzi Abdul Razzaq – Developer & Researcher

Ir. Dwi Oktavianto Wahyu Nugroho, S.T., M.T. – Supervisor & Co-Author
