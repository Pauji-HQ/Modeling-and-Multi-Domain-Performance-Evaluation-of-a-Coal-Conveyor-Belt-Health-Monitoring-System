# Coal Conveyor Belt Health Monitoring  
Multi-Sensor & Multi-Domain Simulation

This project is the implementation of a health monitoring simulation system on a coal conveyor using five types of sensors. This system was built for research purposes, performance analysis, and demonstration of the concept of predictive maintenance in an industrial environment.

This framework models sensor responses and conveyor behavior in four analysis domains: time-domain, frequency-domain (FFT), Laplace (s-plane), and z-domain. All simulations are run through a C# (.NET Framework) application with real-time visualization.

---

## 📌 Key Features

- Simulation of five industrial sensors:
  - Accelerometer (vibration)
  - Temperature sensor (temperature)
  - Load cell (load)
  - Proximity sensor (marker/object)
  - RFID (tag identification)
- Uniform mathematical model (first-order transfer function)
- Multi-domain analysis:
  - Time-domain
  - FFT
  - S-plane (pole/stability)
  - Z-plane (digital pole mapping)
- Real-time visualization through an interactive dashboard
- Fault injection capabilities:
  - Roller imbalance
  - Thermal overload
  - Coal overload / lump anomalies
  - Missing tag / proximity errors
- Suitable for learning, research, and early prototyping of digital twins & predictive maintenance

---

## 🧩 Brief System Description

Each sensor is modeled as a first-order dynamic system so that it can be easily transformed into the frequency domain, s-plane, and z-plane. The conveyor itself is modeled as a mass-spring-damper system to generate disturbances such as vibrations, load changes, or temperature increases due to friction.

The application displays four main panels:
- Signal graph (time-domain)
- FFT spectrum
- S-plane pole map
- Z-plane stability diagram

Users can change sensor parameters such as gain, time constant, sampling rate, and noise level. Fault can be activated to see how each sensor reacts to abnormal conditions.

---

## 🖥️ How to Run

1. Clone the repository:
```bash
git clone https://github.com/<your-username>/CoalConveyorBeltHealthMonitoring.git
