// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// L3G4200D
// This code is designed to work with the L3G4200D_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Gyro?sku=L3G4200D_I2CS#tabs-0-product_tabset-2

#include <application.h>
#include <spark_wiring_i2c.h>

// L3G4200D I2C address is 0x68(104)
#define Addr 0x68

int xGyro = 0, yGyro = 0, zGyro = 0;
void setup()
{
  // Set variable
  Particle.variable("i2cdevice", "L3G4200D");
  Particle.variable("xGyro", xGyro);
  Particle.variable("yGyro", yGyro);
  Particle.variable("zGyro", zGyro);
  
  // Initialise I2C communication as MASTER
  Wire.begin();
  // Initialise serial communication, set baud rate = 9600
  Serial.begin(9600);
  
  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select control register1
  Wire.write(0x20);
  // Normal mode, X, Y, Z-Axis enabled
  Wire.write(0x0F);
  // Stop I2C transmission
  Wire.endTransmission();
  
  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select control register4
  Wire.write(0x23);
  // Continous update, FSR = 2000dps
  Wire.write(0x30);
  // Stop I2C transmission
  Wire.endTransmission();
  delay(300);
}

void loop()
{
  unsigned int data[6];
  for(int i = 0; i < 6; i++)
  {
    // Start I2C Transmission
    Wire.beginTransmission(Addr);
    // Select data register
    Wire.write((40 + i));
    // Stop I2C transmission
    Wire.endTransmission();
    
    // Request 1 byte of data
    Wire.requestFrom(Addr, 1);
    
    // Read 6 bytes of data
    // xGyro lsb, xGyro msb, yGyro lsb, yGyro msb, zGyro lsb, zGyro msb
    if(Wire.available() == 1)
    {
      data[i] = Wire.read();
    }
    delay(300);
  }
    
  // Convert the data
  xGyro = ((data[1] * 256) + data[0]);
  if (xGyro > 32767)
  {
      xGyro -= 65536;
  }
  yGyro = ((data[3] * 256) + data[2]);
  if (yGyro > 32767)
  {
      yGyro -= 65536;
  }
  zGyro = ((data[5] * 256) + data[4]);
  if (zGyro > 32767)
  {
      zGyro -= 65536;
  }
    
  // Output data to dashboard
  Particle.publish("X-Axis of Rotation :", String(xGyro));
  Particle.publish("Y-Axis of Rotation :", String(yGyro));
  Particle.publish("Z-Axis of Rotation :", String(zGyro));
  delay(800);
}
