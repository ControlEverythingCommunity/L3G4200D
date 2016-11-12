// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace L3G4200D
{
	struct Gyroscope
	{
		public double X;
		public double Y;
		public double Z;
	};

	/// <summary>
	/// Sample app that reads data over I2C from an attached L3G4200D Gyroscope
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private const byte GYRO_I2C_ADDR = 0x68;	// 7-bit I2C address of the L3G4200D
		private const byte GYRO_REG_CONTROL1 = 0x20;	// Address of the Control register 1
		private const byte GYRO_REG_CONTROL4 = 0x23;	// Address of the Control register 2
		private const byte GYRO_REG_X = 0x28;		// Address of the X Axis data register
		private const byte GYRO_REG_Y = 0x2A;		// Address of the Y Axis data register
		private const byte GYRO_REG_Z = 0x2C;		// Address of the Z Axis data register

		private I2cDevice I2CGyro;
		private Timer periodicTimer;

		public MainPage()
		{
			this.InitializeComponent();

			// Register for the unloaded event so we can clean up upon exit
			Unloaded += MainPage_Unloaded;

			// Initialize the I2C bus, Gyroscope, and Timer
			InitI2CGyro();
		}

		private async void InitI2CGyro()
		{
			string aqs = I2cDevice.GetDeviceSelector();		// Get a selector string that will return all I2C controllers on the system
			var dis = await DeviceInformation.FindAllAsync(aqs);	// Find the I2C bus controller device with our selector string
			if (dis.Count == 0)
			{
				Text_Status.Text = "No I2C controllers were found on the system";
				return;
			}

			var settings = new I2cConnectionSettings(GYRO_I2C_ADDR);
			settings.BusSpeed = I2cBusSpeed.FastMode;
			I2CGyro = await I2cDevice.FromIdAsync(dis[0].Id, settings);	// Create an I2cDevice with our selected bus controller and I2C settings
			if (I2CGyro == null)
			{
				Text_Status.Text = string.Format(
					"Slave address {0} on I2C Controller {1} is currently in use by " +
					"another application. Please ensure that no other applications are using I2C.",
				settings.SlaveAddress,
				dis[0].Id);
				return;
			}

			/* 
				Initialize the Gyroscope:
				For this device, we create 2-byte write buffers:
				The first byte is the register address we want to write to.
				The second byte is the contents that we want to write to the register. 
			*/
			byte[] WriteBuf_Control1 = new byte[] { GYRO_REG_CONTROL1, 0x0F };	// 0x0F sets Normal Mode and Output Data Rate = 100 Hz, X, Y, Z axes enabled
			byte[] WriteBuf_Control4 = new byte[] { GYRO_REG_CONTROL4, 0x30 };	// 0x30 sets Continous update, Data LSB at lower address, FSR 2000dps, Self test disabled, 4-wire interface

			// Write the register settings
			try
			{
				I2CGyro.Write(WriteBuf_Control1);
				I2CGyro.Write(WriteBuf_Control4);
			}
			// If the write fails display the error and stop running
			catch (Exception ex)
			{
				Text_Status.Text = "Failed to communicate with device: " + ex.Message;
				return;
			}

			// Now that everything is initialized, create a timer so we read data every 300mS
			periodicTimer = new Timer(this.TimerCallback, null, 0, 300);
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			// Cleanup
			I2CGyro.Dispose();
		}

		private void TimerCallback(object state)
		{
			string xText, yText, zText;
			string addressText, statusText;

			// Read and format Gyroscope data
			try
			{
				Gyroscope gyro = ReadI2CGyro();
				addressText = "I2C Address of the Gyroscope L3G4200D: 0x68";
				xText = String.Format("X Axis: {0:F0}", gyro.X);
				yText = String.Format("Y Axis: {0:F0}", gyro.Y);
				zText = String.Format("Z Axis: {0:F0}", gyro.Z);
				statusText = "Status: Running";
			}
			catch (Exception ex)
			{
				xText = "X Axis: Error";
				yText = "Y Axis: Error";
				zText = "Z Axis: Error";
				statusText = "Failed to read from Gyroscope: " + ex.Message;
			}

			// UI updates must be invoked on the UI thread
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Text_X_Axis.Text = xText;
				Text_Y_Axis.Text = yText;
				Text_Z_Axis.Text = zText;
				Text_Status.Text = statusText;
			});
		}

		private Gyroscope ReadI2CGyro()
		{
			byte[] RegAddrBuf = new byte[] { GYRO_REG_X };	// Register address we want to read from
			byte[] ReadBuf = new byte[1];			// We read 1 byte to get X-Axis LSB register in one read

			/*
				Read from the Gyroscope 
				We call WriteRead() so we write the address of the X-Axis LSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf, ReadBuf);
			
			byte[] RegAddrBuf1 = new byte[] { GYRO_REG_X + 1 };	// Register address we want to read from
			byte[] ReadBuf1 = new byte[1];				// We read 1 byte to get X-Axis MSB register in one read

			/*
				Read from the Gyroscope 
				We call WriteRead() so we write the address of the X-Axis MSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf1, ReadBuf1);

			/*
				In order to get the raw 16-bit data value, we need to concatenate two 8-bit bytes from the I2C read for X-Axis.
			*/
			int GyroRawX = (int)(ReadBuf[0] & 0xFF );
			GyroRawX |= (int)((ReadBuf1[0] & 0xFF) * 256);
			if (GyroRawX > 32767)
			{
				GyroRawX = GyroRawX - 65536;
			}

			byte[] RegAddrBuf2 = new byte[] { GYRO_REG_Y };	// Register address we want to read from
			byte[] ReadBuf2 = new byte[1];			// We read 1 byte to get Y-Axis LSB register in one read

			/*
				Read from the Gyroscope 
				We call WriteRead() so we write the address of the Y-Axis LSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf2, ReadBuf2);
			
			byte[] RegAddrBuf3 = new byte[] { GYRO_REG_Y + 1 };	// Register address we want to read from
			byte[] ReadBuf3 = new byte[1];				// We read 1 byte to get Y-Axis MSB register in one read

			/*
				Read from the Gyroscope
				We call WriteRead() so we write the address of the Y-Axis MSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf3, ReadBuf3);

			/*
				In order to get the raw 16-bit data value, we need to concatenate two 8-bit bytes from the I2C read for Y-Axis.
			*/
			int GyroRawY = (int)(ReadBuf2[0] & 0xFF );
			GyroRawY |= (int)((ReadBuf3[0] & 0xFF) * 256);
			if (GyroRawY > 32767)
			{
				GyroRawY = GyroRawY - 65536;
			}

			byte[] RegAddrBuf4 = new byte[] { GYRO_REG_Z };	// Register address we want to read from
			byte[] ReadBuf4 = new byte[1];			// We read 1 byte to get Z-Axis LSB register in one read

			/*
				Read from the Gyroscope 
				Read from the Gyroscope 
				We call WriteRead() so we write the address of the Z-Axis LSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf4, ReadBuf4);
			
			byte[] RegAddrBuf5 = new byte[] { GYRO_REG_Z + 1 };	// Register address we want to read from
			byte[] ReadBuf5 = new byte[1];				// We read 1 byte to get Z-Axis MSB register in one read

			/*
			Read from the Gyroscope 
			We call WriteRead() so we write the address of the Z-Axis MSB I2C register
			*/
			I2CGyro.WriteRead(RegAddrBuf5, ReadBuf5);

			/*
			In order to get the raw 16-bit data value, we need to concatenate two 8-bit bytes from the I2C read for Z-Axis.
			*/
			int GyroRawZ = (int)(ReadBuf4[0] & 0xFF );
			GyroRawZ |= (int)((ReadBuf5[0] & 0xFF) * 256);
			if (GyroRawZ > 32767)
			{
				GyroRawZ = GyroRawZ - 65536;
			}

			Gyroscope gyro;
			gyro.X = GyroRawX;
			gyro.Y = GyroRawY;
			gyro.Z = GyroRawZ;

			return gyro;
		}
	}
}
