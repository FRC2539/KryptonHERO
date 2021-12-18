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

    public class Button {

        static CTRE.Phoenix.Controller.GameController controller = null;
        uint buttonID;
        private bool buttonPressed = false;
        private bool ran = false;
        dummy toDo = null;
        dummy onStop = null;

        public delegate void dummy(); 
        public Button(uint id, dummy execute, dummy end) { // to assign to button, cannot take or return values.
            buttonID = id;
            toDo = execute;
            onStop = end;

            if(controller == null) { controller = new GameController(UsbHostDevice.GetInstance()); } // Create controller.

        }

        //public bool rawIsPressed() { return controller.GetButton(buttonID); } // Sends whatever controller reads.

        //public bool isPressed() { // Sends one true signal upon push then returns false. 
        //    if(!rawIsPressed() || buttonPressed) { buttonPressed = false; return false; } 
        //    else { buttonPressed = true; return true;}
        //}

        //public void toggleWhenPressed() { // Although this method uses 'ran' it really should be 'run'.
        //    if(isPressed() && ran) { onStop(); ran = false; } // Ends upon second toggle. 
        //    else if(isPressed() && !ran) { toDo(); ran = true; } // Starts upon first toggle.
        //    else if(ran) { toDo(); } // This part loops.
        //}

        //public void whileHeld() { // Use rawIsPressed to always watch its true value.
        //    if(rawIsPressed()) { toDo(); ran = true; } // Repeate this while it's being held.
        //    else if(ran) { onStop(); ran = false; } // Assuming we were running and the button is no longer being touched, do this once at the end.
        //}

        //public void whenPressed() { // Used isPressed to prevent something from happening multiple times.
        //    if(isPressed()) { toDo(); onStop(); } 
        //}

    }

    public class Program
    {
        // Create the drive train motors
        static TalonSRX right = new TalonSRX(1);
        //static TalonSRX left = new TalonSRX(9);

        //static TalonSRX elevationMotor = new TalonSRX(11);
        
        // Create the index motors (moving balls to the shooter)
        //static TalonSRX index1 = new TalonSRX(3);
        //static TalonSRX index2 = new TalonSRX(7);

        //static TalonSRX shooter = new TalonSRX(8);

        static GameControllerValues v = new GameControllerValues();

        //static bool isPressed = false;
        static bool lightsIsPressed = false;
        //static bool blinkLightsIsPressed = false;
        static bool lightsOn = false;
        //static bool needToRun = false;
        //static int counter = 0;
        //static int motorDirection = 0;

        //static bool buttonPressed = false;

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        public static void Main()
        {

            //isPressed = false;
            lightsIsPressed = false;
            //blinkLightsIsPressed = false;
            lightsOn = true;

            // Configure the drivetrain motors
            right.SetNeutralMode(NeutralMode.Brake);
            //elevationMotor.ConfigOpenloopRamp(0.25f);

            // Run the robot loop
            while (true)
            {
                // Drive the robot with the gamepad
                OneMotor(1);

                // Feed watchdog to keep the Talons enabled
                CTRE.Phoenix.Watchdog.Feed();
                CTRE.Phoenix.Watchdog.Feed();

                // Set the loop speed to be every 20 ms
                Thread.Sleep(20);

            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */

        static void BlinkLights(uint id)
        {
            //if (_gamepard.GetButton(id))
        }

        static void ToggleLights(uint id)
        {
            if(_gamepad.GetButton(id) && !lightsIsPressed)
            {
                lightsOn = !lightsOn;
                //lightsSpike.Write(lightsOn);
                lightsIsPressed = true;
            }

            else if(!_gamepad.GetButton(id))
            {
                lightsIsPressed = false;
            }


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

            //left.Set(ControlMode.PercentOutput, leftThrot);
            right.Set(ControlMode.PercentOutput, -rightThrot);

            _gamepad.GetAllValues(ref v).ToString();

           

        }

      /*  static void OneMotor()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            float y = _gamepad.GetAxis(1);

            right.Set(ControlMode.PercentOutput, y);

            _gamepad.GetAllValues(ref v).ToString();
        }*/
     
        static void OneMotor(uint id)
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());
            //Debug.Print(_gamepad.GetButton(id).ToString());
 

            right.Set((ControlMode.PercentOutput), 4*System.Math.Pow(_gamepad.GetAxis(id),2)); //This should not be commented out
            Debug.Print("Throttle: " + _gamepad.GetAxis(id));
        }

        /*static void OneMotorBackward(uint id)
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());
            //Debug.Print(_gamepad.GetButton(id).ToString());

            if (_gamepad.GetButton(id))
            {
                motorDirection = -1;
                right.Set(ControlMode.PercentOutput, -1);
                Debug.Print("pressed");
            }
            else if(motorDirection == -1)
            {
                right.Set(ControlMode.PercentOutput, 0);
                Debug.Print("released backward");
            }
        }*/
    }
}

