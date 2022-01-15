using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;


namespace HERO_Arcade_Drive_Example1
{

    public class Button {

        static CTRE.Phoenix.Controller.GameController controller = null;
        uint buttonID;
        //private bool buttonPressed = false;
        //private bool ran = false;
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

    /* Controller Button Map
     * 1 = X
     * 2 = A
     * 3 = B
     * 4 = Y
     * 5 = LB
     * 6 = RB
     * 7 = LT
     * 8 = RT
     * 9 = Back
     * 10 = Start
     * 11 = LJ
     * 12 = RJ
     */

    /*
     * Left Stick:
     *  0 = 
    */

    public class Program
    {
        // Create the drive train motors
        static TalonSRX right = new TalonSRX(1);
        static TalonSRX left = new TalonSRX(2);

        static OutputPort blinkyLight = new OutputPort(CTRE.HERO.IO.Port3.Pin9, true);

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
        static bool isEnabled = false;

        static int count;

        static double speedControl = .75;

        static bool alert = false;
        static int alertCount = 0;

        static bool buttonSpamPrevention = false; //trying to fix start button

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

            count = 0;

            // Configure the drivetrain motors
            right.SetNeutralMode(NeutralMode.Brake);
            //elevationMotor.ConfigOpenloopRamp(0.25f);

            blinkyLight.Write(false);

            // Run the robot loop
            while (true)
            {
                //Enabled();

                if (Enabled() == true)
                {
                    // Drive the robot with the gamepad
                    OneMotor(0,2);

                    if (_gamepad.GetButton(2))
                    {
                        LeftShoot();
                        //Debug.Print("Left");
                    }

                    if (_gamepad.GetButton(3))
                    {
                        RightShoot();
                        //Debug.Print("Right");
                    }

                    if (_gamepad.GetButton(6))
                    {
                        speedControl += .01;
                        //Debug.Print("Increase Speed Control: " + speedControl.ToString());
                    }

                    if (_gamepad.GetButton(8))
                    {
                        
                        speedControl -= .01;
                        //Debug.Print("Reduce Speed Control: " + speedControl.ToString());
                    }

                    if (_gamepad.GetButton(9))
                    {

                        speedControl = .75;
                        alert = true;
                        //Debug.Print("Reset Default Speed Control: " + speedControl.ToString());
                    }


                }


                // Feed watchdog to keep the Talons enabled
                CTRE.Phoenix.Watchdog.Feed();
                CTRE.Phoenix.Watchdog.Feed();

                // Set the loop speed to be every 20 ms
                double defSpeed = 20 / speedControl;
                int setSpeed = (int)System.Math.Round(defSpeed);
                //Debug.Print(setSpeed.ToString());
                if(speedControl < 0)
                {
                    speedControl = 0.1;
                }
                else if (speedControl > 1)
                {
                    speedControl = 1;
                }

                if (alert == true && alertCount < 100)
                {
                    //Debug.Print("Alert");
                    Thread.Sleep(1);
                    alertCount += 1;
                } else
                {
                    //Debug.Print("Alert Off");
                    Thread.Sleep(setSpeed);
                    alert = false;
                    alertCount = 0;
                }
                
                

            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */

        static bool Enabled()
        {

            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            if (count == 20 && isEnabled)
            {
                blinkyLight.Write(lightsOn); // Blink off
                lightsOn = !lightsOn;
                count = 0;
            }
            else if(isEnabled)
            {
                count++;
            }

           

            if (_gamepad.GetButton(10))
            {
                if (buttonSpamPrevention == true)
                {
                    return true;
                }

                buttonSpamPrevention = true;

                if (isEnabled == true)
                {
                    isEnabled = !isEnabled;
                   // Debug.Print("Disabled");
                    blinkyLight.Write(false); // Just on
                    count = 0;
                    return false;
                }
                else
                {
                    isEnabled = !isEnabled;
                   // Debug.Print("Enabled");

                    return true;
                }
            } 
            else
            {
                //Debug.Print("ButtonReleased");
                buttonSpamPrevention = false;
                return isEnabled;
            }
        }

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
     
        static void OneMotor(uint leftAxis, uint rightAxis)
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());
            //Debug.Print(_gamepad.GetButton(id).ToString());
 
            //two for one
            right.Set(ControlMode.PercentOutput, _gamepad.GetAxis(rightAxis)*speedControl); //This should not be commented out
            left.Set(ControlMode.PercentOutput, _gamepad.GetAxis(leftAxis) * speedControl); //This should not be commented out
            //Debug.Print("Motor Left: " + _gamepad.GetAxis(leftAxis) *speedControl + " Motor Right: " + _gamepad.GetAxis(rightAxis)*speedControl);
        }

        static void RightShoot()
        {
            right.Set(ControlMode.PercentOutput, 80 * speedControl); //This should not be commented out
            left.Set(ControlMode.PercentOutput, 50 * speedControl); //This should not be commented out
        }

        static void LeftShoot()
        {
            right.Set(ControlMode.PercentOutput, 50 * speedControl); //This should not be commented out
            left.Set(ControlMode.PercentOutput, 80 * speedControl); //This should not be commented out
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

