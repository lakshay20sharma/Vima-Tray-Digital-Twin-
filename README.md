# Vima – Tray
### Digital Twin Platform for Industrial Motor Monitoring

> A real-time, data-driven virtual replica of industrial motor assets, integrating IoT sensing, edge computing, and extended reality for Industry 4.0.

---

## 📋 Project Overview

**Vima Tray** is an advanced Digital Twin system developed as a final-year B.E. project at **B.M.S. College of Engineering, Bengaluru**, under the Department of Electronics & Instrumentation Engineering (VTU).

The platform creates a live virtual counterpart of an industrial induction motor, enabling real-time monitoring, predictive maintenance, and immersive VR-based training — all powered by edge computing and Unity 3D.

---

## 👥 Team

| Name | USN |
|------|-----|
| Jahnavi S | 1BM22EI021 |
| Lakshay Sharma | 1BM22EI026 |

**Guide:** Dr. Santosh R. Desai, Professor, Dept. of EIE, BMSCE  
**Academic Year:** 2025–26

---

## Key Features

-  **Bidirectional Digital Twin** — Real-time sync between physical motor and virtual model (< 2.5% deviation)
-  **Edge Computing Architecture** — Ultra-low latency data processing (avg. 8.3ms ± 1.2ms)
-  **Predictive Maintenance** — LSTM-based anomaly detection with ~90% accuracy; alerts up to 3.2 hours before failure
-  **Dual Operational Modes** — Simulation Mode for stress testing + VR Mode for immersive monitoring
-  **Multi-Sensor Fusion** — RPM, temperature, vibration, voltage, and current monitoring
-  **VR Integration** — Ajna XR headset with hand tracking and spatial audio feedback
-  **Unity 3D Visualization** — High-fidelity 3D motor rendering at 58+ FPS

---

##  System Architecture

The platform is built on a **5-layer architecture**:

```
┌─────────────────────────────────────────┐
│         User Interaction Layer           │  ← VR/Desktop interfaces, training modules
├─────────────────────────────────────────┤
│          Visualization Layer             │  ← Unity 3D, AR/VR, real-time overlays
├─────────────────────────────────────────┤
│     Data Processing Layer (Edge)         │  ← Anomaly detection, physics modeling, ML
├─────────────────────────────────────────┤
│          Communication Layer             │  ← 5G/Network, QoS, <10ms latency
├─────────────────────────────────────────┤
│            Physical Layer                │  ← Induction motor, sensor array, DAQ
└─────────────────────────────────────────┘
```

---

##  Hardware Components

| Component | Details |
|-----------|---------|
| **Induction Motor** | A25P2C Three-Phase, 25 HP, 1800 RPM, 460V |
| **DC Motor (Load)** | 180W Permanent Magnet DC Motor |
| **Vibration Sensor** | ADXL335 Accelerometer |
| **Temperature Sensor** | DS18B20 (embedded in motor windings) |
| **Edge Server** | Ubuntu 22.04 LTS, 16GB RAM, Quad-core |
| **VR Headset** | Ajna XR (with hand tracking & spatial audio) |
| **Motor Analyzer** | SYSCO Single Phase Induction Motor Remote Analyser |

---

##  Software Stack

| Layer | Technology |
|-------|-----------|
| Visualization | Unity 3D |
| Backend / Edge | Ubuntu Server |
| Data Processing | Python, C# |
| VR Runtime | Ajna XR SDK |

---

## Performance Metrics

| Metric | Result |
|--------|--------|
| End-to-end Latency | 8.3ms ± 1.2ms |
| Packet Loss | < 0.5% |
| Anomaly Detection Accuracy | ~90% |
| Predictive Alert Lead Time | 3.2 hours before failure |
| Frame Rate (Unity) | 60 FPS |
| System Uptime | > 95% |
| Sensor Accuracy | ±1.5% |

---

##  Operational Modes

###  Simulation Mode
Test extreme conditions — thermal runaway, voltage fluctuations, mechanical imbalance — safely without any physical risk to equipment.

###  VR Mode
Immersive environment using the Ajna XR headset for:
- Remote motor diagnostics
- Guided maintenance training
- Collaborative multi-engineer inspection
- Spatial audio alerts for critical thresholds

---

## Sensor & Alert Thresholds

```
Temperature Alert  : > 70°C → Motor Shutdown Protocol
Temperature Alert  : > 80°C → Emergency Alert
Vibration Alert    : > ±1mm
Sampling Rate      : 100 Hz (every 10ms)
Load Test Points   : 25% | 50% | 100% capacity
```

---

## References

Key literature that informed this project:

1. Bill & Shah (2021) — Digital Twins in Assembly Lines, *Industrial Engineering Journal*
2. Cimino et al. (2019) — Digital twin applications in manufacturing, *Computers in Industry*
3. Fuller et al. (2020) — Digital twin enabling technologies, *IEEE Access*
4. Singh et al. (2023) — Digital Twin for Predictive Maintenance of AC Machines, *Machines*
5. Phutane et al. (2025) — Digital Twin innovations for induction motor monitoring, *Next Research*
6. Kerkeni et al. (2024) — Digital Twin for Industry 4.0 predictive maintenance, *ASME JNDE*

---


##  Institution

**B.M.S. College of Engineering** (Autonomous College under VTU)  
Department of Electronics and Instrumentation Engineering  
Bengaluru – 560019, Karnataka, India  
*Visvesvaraya Technological University, Belagavi – 590018*

---

<p align="center">Made with ❤️ by Team Vima Tray | BMSCE EIE 2025–26</p>
