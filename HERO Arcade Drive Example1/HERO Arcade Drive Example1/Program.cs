using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.HERO;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Text;
using System.Threading;


namespace HERO_Arcade_Drive_Example1
{

    public class Program
    {
        /* create a talon */
        static TalonSRX rightSlave = new TalonSRX(4);
        static TalonSRX right = new TalonSRX(3);
        static TalonSRX leftSlave = new TalonSRX(2);
        static TalonSRX left = new TalonSRX(1);

        static VictorSPX elevationMotor = new VictorSPX(5);

        static OutputPort shootSpike = new OutputPort(CTRE.HERO.IO.Port3.Pin9, true); // Pin 9
        static OutputPort lightsSpike = new OutputPort(CTRE.HERO.IO.Port3.Pin8, true); // Pin 8

        static GameControllerValues v = new GameControllerValues();

        static bool isPressed = false;
        static bool lightsIsPressed = false;
        static bool blinkLightsIsPressed = false;
        static bool lightsOn = false;
        static bool needToRun = false;
        static int counter = 0;

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        public static void Main()
        {

            isPressed = false;
            lightsIsPressed = false;
            blinkLightsIsPressed = false;
            lightsOn = true;

            right.SetNeutralMode(NeutralMode.Brake);
            rightSlave.SetNeutralMode(NeutralMode.Brake);
            left.SetNeutralMode(NeutralMode.Brake);
            leftSlave.SetNeutralMode(NeutralMode.Brake);

            elevationMotor.SetNeutralMode(NeutralMode.Brake);
            elevationMotor.ConfigOpenloopRamp(0.25f);

            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */

                Drive();
                ShootButton(8);
                ToggleLights(2);

                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();
                /* run this task every 20ms */
                Thread.Sleep(20);

            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        
        static void BlinkLights(uint id)
        {
            if(_gamepard.GetButton(id))    

        static void ToggleLights(uint id)
        {
            if(_gamepad.GetButton(id) && !lightsIsPressed)
            {
                lightsOn = !lightsOn;
                lightsSpike.Write(lightsOn);
                lightsIsPressed = true;
            }

            else if(!_gamepad.GetButton(id))
            {
                lightsIsPressed = false;
            }


        }

        static void ShootButton(uint id)
        {
            if((_gamepad.GetButton(id)) && (!isPressed))
            {
                // Begin shooting.
                startShooting();
                isPressed = true;
                needToRun = true;
            }

            else if(!_gamepad.GetButton(id))
            {
                isPressed = false;
            }

        }

        static void raiseBarrel()
        {
            elevationMotor.Set(ControlMode.PercentOutput, 0.3);
        }

        static void lowerBarrel() {
            elevationMotor.Set(ControlMode.PercentOutput, -0.2);
        }

        static void stopElevationMotor() {
            elevationMotor.Set(ControlMode.PercentOutput, 0);
        }

        static void startShooting() {
            shootSpike.Write(false);
        }

        static void stopShooting() {
            shootSpike.Write(true);
        }

        static void Deadband(ref float value)
        {
            if (value < -0.05)
            {
                /* outside of deadband */
            }
            else if (value > +0.05)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }
        static void Drive()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            float y = -1 * _gamepad.GetAxis(1);
            float twist = _gamepad.GetAxis(2);

            Deadband(ref y);
            Deadband(ref twist);

            float leftThrot = y + twist;
            float rightThrot = (y - twist) * 1.15f; // Multiply to match left side.

            left.Set(ControlMode.PercentOutput, leftThrot);
            leftSlave.Set(ControlMode.PercentOutput, leftThrot);
            right.Set(ControlMode.PercentOutput, -rightThrot);
            rightSlave.Set(ControlMode.PercentOutput, -rightThrot);

            _gamepad.GetAllValues(ref v).ToString();

            if(v.pov == 0 && (_gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)) { // Raise 
                raiseBarrel(); // Make sure its connected because lowers by default.
            }

            else if(v.pov == 4) { // Lower
                lowerBarrel();
            }

            else { stopElevationMotor(); }

            if(needToRun)
            {
                counter++;
                
                if(counter == 10)
                {
                    stopShooting();
                    counter = 0;
                    needToRun = false;
                }
            }

        }
    }
}
