using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.Threading;

namespace SomethingDinosaurRelated
{
    class KinectControl
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        KinectSensor sensor;
        /// <summary>
        /// Reader for body frames
        /// </summary>
        BodyFrameReader bodyFrameReader;
        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;
        /// <summary>
        /// Screen width and height for determining the exact mouse sensitivity
        /// </summary>
        int screenWidth, screenHeight;

        /// <summary>
        /// timer for pause-to-click feature
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// How far the cursor move according to your hand's movement
        /// </summary>
        public float mouseSensitivity = MOUSE_SENSITIVITY;

        /// <summary>
        /// Time required as a pause-clicking
        /// </summary>
        public float timeRequired = TIME_REQUIRED;
        /// <summary>
        /// The radius range your hand move inside a circle for [timeRequired] seconds would be regarded as a pause-clicking
        /// </summary>
        public float pauseThresold = PAUSE_THRESOLD;
        /// <summary>
        /// Decide if the user need to do clicks or only move the cursor
        /// </summary>
        /// <summary>
        /// Use Grip gesture to click or not
        /// </summary>
        public bool useGripGesture = USE_GRIP_GESTURE;
        public bool useLassoGesture = USE_LASSO_GESTURE;
        public bool mouseCanMove = MOUSE_CAN_MOVE;
        /// <summary>
        /// Value 0 - 0.95f, the larger it is, the smoother the cursor would move
        /// </summary>
        public float cursorSmoothing = CURSOR_SMOOTHING;

        // Default values
        public const float MOUSE_SENSITIVITY = 2.0f;
        public const float TIME_REQUIRED = 2f;
        public const float PAUSE_THRESOLD = 60f;
        public const bool USE_GRIP_GESTURE = true;
        public const bool USE_LASSO_GESTURE = true;
        public const float CURSOR_SMOOTHING = 0.95f;
        public const bool MOUSE_CAN_MOVE = false;
        public bool startSpeech = false;
        /// <summary>
        /// Determine if we have tracked the hand and used it to move the cursor,
        /// If false, meaning the user may not lift their hands, we don't get the last hand position and some actions like pause-to-click won't be executed.
        /// </summary>
        bool alreadyTrackedPos = false;
        float x = 0;
        float y = 0;
        float smoothing = 0.95f;
        float pxd = 0;
        bool held = false;
        /// <summary>
        /// For storing last cursor position
        /// </summary>
        Point lastCurPos = new Point(0, 0);
        float timeCount = 0;
        /// <summary>
        /// If true, user did a right hand Grip gesture
        /// </summary>
        bool wasRightGrip = false;

        

        public KinectControl()
        {
            // get Active Kinect Sensor
            sensor = KinectSensor.GetDefault();
            // open the reader for the body frames
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // get screen with and height
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // set up timer, execute every 0.1s
           timer.Interval = new TimeSpan(0, 0, 0, 0, 100); 
　　　　    timer.Tick += new EventHandler(Timer_Tick);
　　　　    timer.Start();

            // open the sensor
            sensor.Open();
        }

        public void minimise()
        {
            MouseControl.minimise();
        }


        /// <summary>
        /// Pause to click timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Tick(object sender, EventArgs e)
        {
            if (useGripGesture || useLassoGesture) return;

            if (!alreadyTrackedPos) {
                timeCount = 0;
                return;
            }
            
            Point curPos = MouseControl.GetCursorPosition();
            lastCurPos = curPos;
        }

        /// <summary>
        /// Read body frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived) 
            {
                alreadyTrackedPos = false;
                return;
            }

            foreach (Body body in this.bodies)
            {

                // get first tracked body only, notice there's a break below.
                if (body.IsTracked && startSpeech)
                {
                    
                    // get various skeletal positions
                    CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                    CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;
                    CameraSpacePoint handTip = body.Joints[JointType.HandTipRight].Position;
                    if (mouseCanMove)
                    {
                        /* hand x calculated by this. we don't use shoulder right as a reference cause the shoulder right
                         * is usually behind the lift right hand, and the position would be inferred and unstable.
                         * because the spine base is on the left of right hand, we plus 0.05f to make it closer to the right. */
                        x = handRight.X - spineBase.X + 0.05f;
                        /* hand y calculated by this. ss spine base is way lower than right hand, we plus 0.51f to make it
                         * higer, the value 0.51f is worked out by testing for a several times, you can set it as another one you like. */
                        y = spineBase.Y - handRight.Y + 0.51f;
                        // get current cursor position
                        Point curPos = MouseControl.GetCursorPosition();
                        // set cursor position
                        MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));
                    }
                        alreadyTrackedPos = true;

                    if(held && body.HandRightState == HandState.Open)
                    {
                        held = false;
                    }
                    // Grip gesture
                    else if(body.HandLeftState == HandState.Open)
                    {
                        mouseCanMove = true;
                        if (useGripGesture)
                        {
                            if (body.HandRightState == HandState.Closed)
                            {
                                if (!wasRightGrip)
                                {
                                    if (body.HandRightState == HandState.Closed)
                                    {
                                        MouseControl.MouseLeftDown();
                                        wasRightGrip = true;
                                    }
                                }
                            }
                            else if (body.HandRightState == HandState.Open)
                            {
                                if (wasRightGrip)
                                {
                                    MouseControl.MouseLeftUp();
                                    wasRightGrip = false;
                                }
                            }
                        }
                        //Use Lasso Gesture
                        if (useLassoGesture)
                        {
                            if (body.HandRightState == HandState.Lasso)
                            {
                                Point newPos = MouseControl.GetCursorPosition();
                                float yd = (int)(newPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - newPos.Y) * smoothing);
                                float xd = (int)(newPos.X + ((y + 0.25f) * mouseSensitivity * screenHeight - newPos.X) * smoothing);
                                if (yd > newPos.Y)
                                {
                                    MouseControl.MouseScrollUpDown(4);
                                }
                                else if (yd < newPos.Y)
                                {
                                    MouseControl.MouseScrollUpDown(-4);
                                }
                                else if (xd < newPos.X)
                                {
                                    MouseControl.MouseScrollLeftRight(4);
                                }
                                else if (xd > newPos.X)
                                {
                                    MouseControl.MouseScrollLeftRight(-4);
                                }
                            }
                        }
                    }
                    else if(handLeft.Y > spineBase.Y && body.HandLeftState == HandState.Closed)
                    {
                        mouseCanMove = false;
                        if (useGripGesture)
                        {
                            if (body.HandRightState == HandState.Closed)
                            {
                                if (body.HandRightState == HandState.Closed && !held)
                                {
                                    MouseControl.DoMouseClick();
                                    held = true;
                                }
                            }
                        }
                        if (useLassoGesture)
                        {
                            if (body.HandRightState == HandState.Lasso && !held)
                            {
                                MouseControl.DoDoubleClick();
                                held = true;
                            }
                        }
                    }
                    else if(handLeft.Y > spineBase.Y && body.HandLeftState == HandState.Lasso)
                    {
                        mouseCanMove = false;
                        if (useGripGesture)
                        {
                            if (body.HandRightState == HandState.Closed)
                            {
                                if (body.HandRightState == HandState.Closed)
                                {
                                    MouseControl.DoRightClick();
                                }
                            }
                        }
                        if (useLassoGesture)
                        {
                            if (body.HandRightState == HandState.Lasso)
                            {
                                float xd = (float)Math.Truncate((handRight.X - handLeft.X) *10)/10;
                                Console.WriteLine(xd);
                                if (xd > 0.2 && pxd != 0)
                                {
                                    MouseControl.CtrlDown();
                                    if (pxd > xd)
                                    {
                                        MouseControl.MouseScrollUpDown(-1);
                                    }
                                    else if(pxd < xd)
                                    {
                                        MouseControl.MouseScrollUpDown(1);
                                    }
                                    MouseControl.CtrlUp();
                                }
                                pxd = xd;
                            }
                        }
                    }
                    else
                    {
                        wasRightGrip = true;
                        alreadyTrackedPos = false;
                    }

                    // get first tracked body only
                    break;
                }
            }
        }

        public void Close()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }

    }
}
