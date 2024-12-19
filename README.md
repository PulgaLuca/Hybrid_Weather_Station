# 🌦️ Weather Station System - Sensors, Clients, and Data Management 📡

## Description:
This project outlines the architecture of a Weather Station System, combining various sensors, client-server communication, and data management for precise environmental monitoring. 📊🌍

## Features:
🌡️ Sensor Integration: Includes wind sensors, rain gauges, temperature, humidity, and barometric pressure sensors, all connected via buses like SPI and 1-Wire.
🌐 Client-Server Communication: Utilizes TCP/IP sockets to transmit data between a Raspberry Pi (server) and a PC (client).
📂 Data Management: Environmental data is structured in JSON format for seamless transmission and integration into a database server.
💻 Multiplatform Compatibility: Works with Raspberry Pi (Raspbian) and Windows-based hosts.
How It Works:

Sensors capture weather data and transmit it through communication buses like SPI and 1-Wire.
The Raspberry Pi processes the data and sends it in JSON format over a network connection to the client.
Data is stored in a database and can be displayed on a web server or analyzed further.

## Tech Stack:
Sensors: DHT22 (temperature and humidity), rain gauge, wind vane, barometric pressure sensor.
Communication: SPI, I2C, 1-Wire, TCP/IP sockets.
Platforms: Raspberry Pi, Raspbian, Windows PC.
Data Format: JSON 📄
