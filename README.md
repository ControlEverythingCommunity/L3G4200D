[![L3G4200D](L3G4200D_I2CS.png)](https://www.controleverything.com/content/Gyro?sku=L3G4200D_I2CS)
# L3G4200D
L3G4200D 3 Axis Gyroscope Sensor

The L3G4200D provides 3-Axis ultra stable Digital Gyrometer output using I²C communications.

This Device is available from ControlEverything.com [SKU: L3G4200D_I2CS]

https://www.controleverything.com/content/Gyro?sku=L3G4200D_I2CS

This Sample code can be used with Raspberry pi.

##Java 
Download and install pi4j library on Raspberry pi. Steps to install pi4j are provided at:

http://pi4j.com/install.html

Download (or git pull) the code in pi.

Compile the java program.
```cpp
$> pi4j L3G4200D.java
```

Run the java program as.
```cpp
$> pi4j L3G4200D
```

##Python 
Download and install smbus library on Raspberry pi. Steps to install smbus are provided at:

https://pypi.python.org/pypi/smbus-cffi/0.5.1

Download (or git pull) the code in pi. Run the program

```cpp
$> python L3G4200D.py
```

The code output is the raw values of angular acceleration in X, Y and Z axis.
