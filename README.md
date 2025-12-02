Coal Conveyor Belt Health Monitoring — Multi-Sensor & Multi-Domain Simulation

This repository contains the full implementation of a multi-sensor, multi-domain simulation framework for evaluating the health condition of a coal conveyor belt system.
The system is based on the research paper:

“Modeling and Multi-Domain Performance Evaluation of a Coal Conveyor Belt Health Monitoring System”
by Ahmad Fauzi Abdul Razzaq & Ir. Dwi Oktavianto Wahyu Nugroho, S.T., M.T.

🚀 Overview

Coal conveyor belts are critical components in mining and bulk-material handling operations. Failures such as:

Roller imbalance

Belt misalignment

Overheating

Abnormal vibration

Uneven load distribution

can lead to severe downtime and safety issues.

This project presents a real-time simulation platform that models five heterogeneous sensors using a unified first-order dynamic system, and evaluates them across four analytical domains:

Time Domain

Frequency Domain (FFT)

Laplace Domain (S-plane)

Z-Domain (Digital Pole Stability)

Developed using C# (.NET Framework), the system provides comprehensive visualization and diagnostic tools for research, education, and early-stage predictive maintenance development.

🧩 Key Features
📡 Five Simulated Sensors
Sensor	Purpose
MMA7361L Accelerometer	Vibration & roller fault detection
TMP36 Temperature Sensor	Detect overheating & friction
H8C Load Cell	Coal load changes & lump anomalies
SEN0381 Proximity Sensor	Belt marker & object detection
MFRC522 RFID Module	Tag-based position tracking

All sensors are modeled with the same first-order transfer function, ensuring consistent behavior analysis across domains.

🔍 Multi-Domain Capabilities
1️⃣ Time-Domain Analysis

View raw waveforms

Observe transient responses

Inject faults to study sensor reactions

2️⃣ Frequency-Domain (FFT)

Detect harmonics from roller imbalance

Identify low-frequency variations in load

View spectral signatures of faults

3️⃣ Laplace Domain (S-Plane)

Pole visualization

System dynamic behavior

Stability evaluation

4️⃣ Z-Domain (Digital Pole Stability)

Digital pole mapping

Check sampling adequacy

Ensure discrete-time stability

🖥️ Software Architecture

The system consists of:

Signal Generator → Generates sensor input signals

Domain Transformer → Computes FFT, S-plane, and Z-plane models

Real-Time Visualizer → Multi-panel charts for all analytical domains

Control Module → Adjust gain, time constants, sampling rate, noise, etc.

This architecture allows simultaneous or independent simulation of all sensors.

⚙️ How to Run

Clone this repository:

git clone https://github.com/<your-username>/CoalConveyorBeltHealthMonitoring.git


Open the project in Visual Studio (.NET Framework)

Build the solution

Run the application

Use the available dashboard to:

Monitor sensors

Visualize signals

Inject faults

Analyze each domain
